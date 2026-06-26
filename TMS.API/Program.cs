using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TMS.Application;
using System.Text;
using TMS.Infrastructure;


namespace TMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
                #region App Builder

                var builder = WebApplication.CreateBuilder(args);

                #region Built-In Services: Already dclared -> Need To Register

                #region Layers Registerations

                // Configuration
                var configuration = builder.Configuration;

                // Register layers
                builder.Services.AddApplicationService(configuration);
                builder.Services.AddInfrastructureService(configuration);

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

                #endregion Layers Registerations

                #region JWT Authentication Service

                // Add Authentication with JWT instead of Cookies
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["jwt:issuer"],
                        ValidAudience = builder.Configuration["jwt:audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"]))
                    };
                });

                #endregion JWT Authentication Service

                #region Controllers Service

                builder.Services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddMvc()
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<FluentValidationFilter>();
                });

                #endregion Controllers Service

                #region Swagger Service

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

                builder.Services.AddSwaggerGen(c =>
                {
                    c.EnableAnnotations();
                    c.IgnoreObsoleteProperties();

                    // Add JWT Authentication to Swagger
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter 'Bearer' [space] and then your valid JWT token."
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                });

                #endregion Swagger Service

                #region CORS Policy

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowFrontend",
                        policy =>
                        {
                            policy.WithOrigins(
                                    builder.Configuration["consumers:AllowFrontend"], 
                                    "http://localhost:4200",
                                    "http://127.0.0.1:5500" 
                                )
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials();
                        });
                });

                #endregion CORS Policy

                #endregion Built-In Services: Already dclared -> Need To Register

                var app = builder.Build();

                #endregion App Builder

                #region Middlewares

               
                app.UseCors("AllowFrontend");

                // Global exception handler must be registered early in the pipeline
                app.UseMiddleware<GlobalExceptionMiddleware>();

                app.UseDefaultFiles();
                app.UseStaticFiles();

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();

                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        foreach (var description in app.DescribeApiVersions())
                        {
                            c.SwaggerEndpoint(
                                $"/swagger/{description.GroupName}/swagger.json",
                                $"TMS API {description.GroupName.ToUpperInvariant()}");
                        }

                        c.RoutePrefix = "swagger";
                    });
                }

                app.UseAuthentication(); 
                app.UseAuthorization(); 

                app.MapControllers(); 

                app.Run(); 

                #endregion Middlewares

        }
    }
}
