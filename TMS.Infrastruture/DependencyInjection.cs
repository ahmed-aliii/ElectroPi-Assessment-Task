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

namespace TMS.Infrastructure
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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            #endregion

            #region Identity Services

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
               
                options.Password.RequireDigit = true;                
                options.Password.RequireLowercase = true;            
                options.Password.RequireUppercase = true;            
                options.Password.RequireNonAlphanumeric = true;     
                options.Password.RequiredLength = 6;                
                options.Password.RequiredUniqueChars = 1;          
                options.SignIn.RequireConfirmedEmail = true;        

                options.User.RequireUniqueEmail = true;           
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; 

            
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); 
                options.Lockout.MaxFailedAccessAttempts = 5;                       
                options.Lockout.AllowedForNewUsers = true;                         

               
                options.SignIn.RequireConfirmedEmail = false;     
                options.SignIn.RequireConfirmedPhoneNumber = false; 
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders(); 

            #endregion Identity Services


            #region Services
            #endregion

            return services;
        }
    }
}
