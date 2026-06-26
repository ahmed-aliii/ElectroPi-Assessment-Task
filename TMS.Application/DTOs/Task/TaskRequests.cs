using DomainTaskStatus = TMS.Domain.TaskStatus;
using TaskPriority = TMS.Domain.TaskPriority;

namespace TMS.Application
{
    public record CreateTaskRequest(
        Guid ProjectId,
        string Title,
        string? Description,
        DomainTaskStatus Status = DomainTaskStatus.Todo,
        DateTime? DueDate = null,
        TaskPriority Priority = TaskPriority.Medium);

    public record UpdateTaskStatusRequest(DomainTaskStatus Status);
}
