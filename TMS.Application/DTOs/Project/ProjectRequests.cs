namespace TMS.Application
{
    public record CreateProjectRequest(string Name, string? Description);
    public record UpdateProjectRequest(string Name, string? Description);
    public record GetProjectsPagedRequest(int PageNumber = 1, int PageSize = 10);
}
