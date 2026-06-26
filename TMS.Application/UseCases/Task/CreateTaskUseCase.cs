using AutoMapper;
using DomainTask = TMS.Domain.Task;

namespace TMS.Application
{
    public interface ICreateTaskUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<TaskResponse>> ExecuteAsync(CreateTaskRequest request);
    }

    public class CreateTaskUseCase : ICreateTaskUseCase
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateTaskUseCase(
            ITaskService taskService,
            IProjectService projectService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _taskService = taskService;
            _projectService = projectService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async System.Threading.Tasks.Task<ServiceResult<TaskResponse>> ExecuteAsync(CreateTaskRequest request)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<TaskResponse>.Unauthorized();

            var projectResult = await _projectService.GetByIdAndOwnerAsync(request.ProjectId, ownerId);
            if (!projectResult.Success || projectResult.Data is null)
                return ServiceResult<TaskResponse>.NotFound("Project not found.");

            var task = DomainTask.Create(
                request.ProjectId,
                request.Title,
                request.Description,
                request.Status,
                request.DueDate,
                request.Priority,
                ProjectAuditHelper.TryParseAuditUserId(ownerId));

            var result = await _taskService.CreateAsync(task);
            if (!result.Success || result.Data is null)
                return ServiceResult<TaskResponse>.BadRequest(result.Messages);

            return ServiceResult<TaskResponse>.Created(_mapper.Map<TaskResponse>(result.Data));
        }
    }
}
