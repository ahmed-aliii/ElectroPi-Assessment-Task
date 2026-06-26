using DomainTaskStatus = TMS.Domain.TaskStatus;
using TaskPriority = TMS.Domain.TaskPriority;

namespace TMS.Application
{
    public record TaskResponse(
        Guid Id,
        Guid ProjectId,
        string Title,
        string? Description,
        DomainTaskStatus Status,
        DateTime? DueDate,
        TaskPriority Priority,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
