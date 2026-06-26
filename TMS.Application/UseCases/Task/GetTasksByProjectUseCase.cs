using AutoMapper;

namespace TMS.Application
{
    public interface IGetTasksByProjectUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<TaskResponse>>> ExecuteAsync(Guid projectId);
    }

    public class GetTasksByProjectUseCase : IGetTasksByProjectUseCase
    {
        private readonly ITaskService _taskService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetTasksByProjectUseCase(
            ITaskService taskService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ICacheService cacheService)
        {
            _taskService = taskService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<TaskResponse>>> ExecuteAsync(Guid projectId)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<IReadOnlyList<TaskResponse>>.Unauthorized();

            var cacheKey = CacheKeys.TasksByProject(ownerId, projectId);
            var cachedTasks = await _cacheService.GetAsync<IReadOnlyList<TaskResponse>>(cacheKey);
            if (cachedTasks is not null)
                return ServiceResult<IReadOnlyList<TaskResponse>>.Ok(cachedTasks);

            var result = await _taskService.GetByProjectIdAndOwnerAsync(projectId, ownerId);
            if (!result.Success || result.Data is null)
                return ServiceResult<IReadOnlyList<TaskResponse>>.BadRequest(result.Messages);

            var mapped = _mapper.Map<IReadOnlyList<TaskResponse>>(result.Data);
            await _cacheService.SetAsync(cacheKey, mapped, TimeSpan.FromMinutes(2));

            return ServiceResult<IReadOnlyList<TaskResponse>>.Ok(mapped);
        }
    }
}
