using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            #region AutoMapper

            var licenseKey = configuration["AutoMapper:LicenseKey"];
            services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = licenseKey;
            },
                Assembly.GetExecutingAssembly()
            );

            #endregion AutoMapper

            #region FluentValidation

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            #endregion FluentValidation

            #region Mediator

            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
            });

            #endregion Mediator

            #region UseCases
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            services.AddScoped<ILoginUseCase, LoginUseCase>();
            services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
            services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
            services.AddScoped<IGetProjectsPagedUseCase, GetProjectsPagedUseCase>();
            services.AddScoped<IGetProjectByIdUseCase, GetProjectByIdUseCase>();
            services.AddScoped<IUpdateProjectUseCase, UpdateProjectUseCase>();
            services.AddScoped<IDeleteProjectUseCase, DeleteProjectUseCase>();
            #endregion

            #region GenericService
            services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProjectService, ProjectService>();

            #endregion


            return services;
        }
    }
}
