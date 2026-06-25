using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application;
using TMS.Domain;

namespace TMS.Infrastruture
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            #region AddDbContext

            var connectionString = configuration.GetConnectionString("TMS");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            #endregion AddDbContext

          

            #region Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            //services.AddScoped<IUserRepository, UserRepository>();
            #endregion

            #region Identity Services

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // -------------------
                // Password settings
                // -------------------
                options.Password.RequireDigit = true;                // Must contain a number
                options.Password.RequireLowercase = true;            // Must contain a lowercase letter
                options.Password.RequireUppercase = true;            // Must contain an uppercase letter
                options.Password.RequireNonAlphanumeric = true;     // Must contain a special character
                options.Password.RequiredLength = 6;                // Minimum length
                options.Password.RequiredUniqueChars = 1;           // Minimum unique characters
                options.SignIn.RequireConfirmedEmail = true;        // Optional: require email confirmation

                // -------------------
                // User settings
                // -------------------
                options.User.RequireUniqueEmail = true;           // Email must be unique
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Allowed username chars

                // -------------------
                // Lockout settings
                // -------------------
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lockout duration
                options.Lockout.MaxFailedAccessAttempts = 5;                       // Max failed attempts
                options.Lockout.AllowedForNewUsers = true;                         // Lockout enabled for new users

                // -------------------
                // SignIn settings
                // -------------------
                options.SignIn.RequireConfirmedEmail = false;     // Require email confirmation
                options.SignIn.RequireConfirmedPhoneNumber = false; // Require phone confirmation
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders(); // <-- THIS IS IMPORTANT

            #endregion Identity Services


            #region Services
            //services.AddScoped<IAuthService, AuthService>();
            #endregion

            return services;
        }
    }
}
