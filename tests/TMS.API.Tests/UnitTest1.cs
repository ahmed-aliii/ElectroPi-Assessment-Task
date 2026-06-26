using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TMS.API;
using TMS.Application;
using TMS.Domain;
using TMS.Infrastructure;
using Task = System.Threading.Tasks.Task;

namespace TMS.API.Tests;

public class ApiTests : IClassFixture<ApiTests.TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ApiResponseFromResult_WhenResultIsOk_ReturnsMatchingStatusAndPayload()
    {
        var controller = new TestController();
        var result = ServiceResult<string>.Ok("payload", "All good");

        var actionResult = ApiResponse.FromResult(controller, result);

        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var response = objectResult.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeTrue();
        response.Messages.Should().Be("All good");
        response.Data.Should().Be("payload");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void ApiResponseFromResult_WhenResultIsNull_ReturnsInternalServerError()
    {
        var controller = new TestController();

        var actionResult = ApiResponse.FromResult<string>(controller, null!);

        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task ProtectedEndpoint_WhenRequestHasNoBearerToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/Projects");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WhenRequestIsInvalid_ReturnsBadRequestFromValidationFilter()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("not-an-email", "", "", "weak");

        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuthenticatedProjectFlow_WhenUserRegistersAndCreatesProject_ReturnsCreatedProject()
    {
        var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/register",
            new RegisterRequest(email, "Ahmed", "Mahmoud", "Password1!"));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LoginRequest(email, "Password1!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        loginBody.Should().NotBeNull();
        loginBody!.Data.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data!.Token);
        var createProjectResponse = await client.PostAsJsonAsync(
            "/api/v1/Projects",
            new CreateProjectRequest("Assessment Tests", "Created from integration test"));

        createProjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createProjectResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>();
        createBody.Should().NotBeNull();
        createBody!.Success.Should().BeTrue();
        createBody.Data.Should().NotBeNull();
        createBody.Data!.Name.Should().Be("Assessment Tests");
    }

    [Fact]
    public async Task ProjectEndpoint_WhenDifferentUserRequestsProject_ReturnsNotFound()
    {
        var ownerClient = _factory.CreateClient();
        var otherClient = _factory.CreateClient();
        var ownerToken = await RegisterAndLoginAsync(ownerClient, $"owner-{Guid.NewGuid():N}@example.com");
        var otherToken = await RegisterAndLoginAsync(otherClient, $"other-{Guid.NewGuid():N}@example.com");

        ownerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var createProjectResponse = await ownerClient.PostAsJsonAsync(
            "/api/v1/Projects",
            new CreateProjectRequest("Private Project", null));
        createProjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createProjectResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>();
        var projectId = createBody!.Data!.Id;

        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var otherUserResponse = await otherClient.GetAsync($"/api/v1/Projects/{projectId}");

        otherUserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, string email)
    {
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/register",
            new RegisterRequest(email, "Ahmed", "Mahmoud", "Password1!"));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LoginRequest(email, "Password1!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        loginBody.Should().NotBeNull();
        loginBody!.Data.Should().NotBeNull();
        return loginBody.Data!.Token;
    }

    private sealed class TestController : ControllerBase;

    public sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Filename=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptors = services
                    .Where(descriptor =>
                        descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                        descriptor.ServiceType == typeof(DbContextOptions) ||
                        IsDbContextOptionsConfiguration(descriptor.ServiceType))
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection.Dispose();
            }
        }

        private static bool IsDbContextOptionsConfiguration(Type serviceType) =>
            serviceType.IsGenericType &&
            serviceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1";
    }
}
