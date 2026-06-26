using AutoMapper;

namespace TMS.Application
{
    public interface IUpdateTaskStatusUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<TaskResponse>> ExecuteAsync(Guid id, UpdateTaskStatusRequest request);
    }

    public class UpdateTaskStatusUseCase : IUpdateTaskStatusUseCase
    {
        private readonly ITaskService _taskService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateTaskStatusUseCase(
            ITaskService taskService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _taskService = taskService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async System.Threading.Tasks.Task<ServiceResult<TaskResponse>> ExecuteAsync(Guid id, UpdateTaskStatusRequest request)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<TaskResponse>.Unauthorized();

            var getResult = await _taskService.GetTrackedByIdAndOwnerAsync(id, ownerId);
            if (!getResult.Success || getResult.Data is null)
                return ServiceResult<TaskResponse>.NotFound(getResult.Messages);

            var task = getResult.Data;
            task.ChangeStatus(request.Status, ProjectAuditHelper.TryParseAuditUserId(ownerId));

            var result = await _taskService.UpdateAsync(task);
            if (!result.Success || result.Data is null)
                return ServiceResult<TaskResponse>.BadRequest(result.Messages);

            return ServiceResult<TaskResponse>.Ok(_mapper.Map<TaskResponse>(result.Data));
        }
    }
}
