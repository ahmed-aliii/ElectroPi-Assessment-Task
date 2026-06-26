using TMS.Domain;

namespace TMS.Application
{
    public record AuthenticatedUserDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        bool IsActive,
        DateTime CreatedAt,
        IReadOnlyList<string> Roles)
    {
        public static AuthenticatedUserDto FromUser(ApplicationUser user, IList<string> roles) =>
            new(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.CreatedAt,
                roles.ToList().AsReadOnly());
    }

    public record AuthResponse(
        string Token,
        string RefreshToken,
        DateTime AccessTokenExpiration,
        DateTime RefreshTokenExpiration,
        AuthenticatedUserDto User);
}
