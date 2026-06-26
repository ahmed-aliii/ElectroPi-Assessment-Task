using AutoMapper;
using TMS.Domain;

namespace TMS.Application
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<Project, ProjectResponse>()
                .ConstructUsing(s => new ProjectResponse(
                    s.Id,
                    s.Name,
                    s.Description,
                    s.CreatedAt,
                    s.UpdatedAt,
                    s.Tasks.Count));
        }
    }
}
