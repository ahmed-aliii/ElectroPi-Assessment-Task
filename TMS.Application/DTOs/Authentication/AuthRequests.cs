using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Application
{
    //Request DTOs for authentication
    public record RegisterRequest(string Email, string FirstName, string LastName, string Password);
    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string AccessToken, string RefreshToken);
}
