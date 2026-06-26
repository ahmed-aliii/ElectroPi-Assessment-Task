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

        public GetTasksByProjectUseCase(
            ITaskService taskService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _taskService = taskService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<TaskResponse>>> ExecuteAsync(Guid projectId)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<IReadOnlyList<TaskResponse>>.Unauthorized();

            var result = await _taskService.GetByProjectIdAndOwnerAsync(projectId, ownerId);
            if (!result.Success || result.Data is null)
                return ServiceResult<IReadOnlyList<TaskResponse>>.BadRequest(result.Messages);

            var mapped = _mapper.Map<IReadOnlyList<TaskResponse>>(result.Data);
            return ServiceResult<IReadOnlyList<TaskResponse>>.Ok(mapped);
        }
    }
}
