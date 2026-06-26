using AutoMapper;
using DomainTask = TMS.Domain.Task;

namespace TMS.Application
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<DomainTask, TaskResponse>()
                .ConstructUsing(s => new TaskResponse(
                    s.Id,
                    s.ProjectId,
                    s.Title,
                    s.Description,
                    s.Status,
                    s.DueDate,
                    s.Priority,
                    s.CreatedAt,
                    s.UpdatedAt));
        }
    }
}
