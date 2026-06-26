namespace TMS.Application
{
    public record ProjectResponse(
        Guid Id,
        string Name,
        string? Description,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int TaskCount);
}
