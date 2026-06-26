using System.Net;
using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using Moq;
using TMS.Domain;
using DomainTaskStatus = TMS.Domain.TaskStatus;
using DomainTask = TMS.Domain.Task;
using Task = System.Threading.Tasks.Task;

namespace TMS.Application.Tests;

public class UseCaseTests
{
    private const string OwnerId = "8f5bb6ec-fd05-47f1-96e2-26335d168b25";

    [Fact]
    public async Task LoginUseCase_WhenUserDoesNotExist_ReturnsUnauthorized()
    {
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        userService
            .Setup(service => service.GetByEmailAsync("missing@example.com"))
            .ReturnsAsync(ServiceResult<ApplicationUser>.NotFound("Not found"));
        var useCase = new LoginUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new LoginRequest("missing@example.com", "Password1!"));

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Messages.Should().Be("Invalid credentials.");
        authService.Verify(service => service.BuildAuthResponseAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginUseCase_WhenPasswordIsInvalid_ReturnsUnauthorized()
    {
        var user = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        userService
            .Setup(service => service.GetByEmailAsync(user.Email!))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(user));
        userService
            .Setup(service => service.CheckPasswordAsync(user, "wrong"))
            .ReturnsAsync(ServiceResult<bool>.Ok(false));
        var useCase = new LoginUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new LoginRequest(user.Email!, "wrong"));

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        authService.Verify(service => service.BuildAuthResponseAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginUseCase_WhenCredentialsAreValid_ReturnsAuthResponse()
    {
        var user = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");
        var authResponse = CreateAuthResponse(user);
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        userService
            .Setup(service => service.GetByEmailAsync(user.Email!))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(user));
        userService
            .Setup(service => service.CheckPasswordAsync(user, "Password1!"))
            .ReturnsAsync(ServiceResult<bool>.Ok(true));
        authService
            .Setup(service => service.BuildAuthResponseAsync(user))
            .ReturnsAsync(authResponse);
        var useCase = new LoginUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new LoginRequest(user.Email!, "Password1!"));

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Be(authResponse);
    }

    [Fact]
    public async Task RegisterUserUseCase_WhenUserAlreadyExists_ReturnsConflict()
    {
        var existingUser = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        userService
            .Setup(service => service.GetByEmailAsync(existingUser.Email!))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(existingUser));
        var useCase = new RegisterUserUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new RegisterRequest(existingUser.Email!, "Ahmed", "Mahmoud", "Password1!"));

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        userService.Verify(service => service.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserUseCase_WhenRegistrationSucceeds_ReturnsAuthResponse()
    {
        var user = ApplicationUser.Register("new@example.com", "Ahmed", "Mahmoud");
        var authResponse = CreateAuthResponse(user);
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        userService
            .Setup(service => service.GetByEmailAsync(user.Email!))
            .ReturnsAsync(ServiceResult<ApplicationUser>.NotFound("Not found"));
        userService
            .Setup(service => service.RegisterAsync(user.Email!, "Ahmed", "Mahmoud", "Password1!"))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(user));
        authService
            .Setup(service => service.BuildAuthResponseAsync(user))
            .ReturnsAsync(authResponse);
        var useCase = new RegisterUserUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new RegisterRequest(user.Email!, "Ahmed", "Mahmoud", "Password1!"));

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Be(authResponse);
    }

    [Fact]
    public async Task RefreshTokenUseCase_WhenAccessTokenHasNoEmail_ReturnsUnauthorized()
    {
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        authService
            .Setup(service => service.GetPrincipalFromExpiredToken("access-token"))
            .Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        var useCase = new RefreshTokenUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new RefreshTokenRequest("access-token", "refresh-token"));

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Messages.Should().Be("Invalid access token.");
        userService.Verify(service => service.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenUseCase_WhenRefreshTokenIsValid_ReturnsNewAuthResponse()
    {
        var user = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");
        user.UpdateRefreshToken("refresh-token", DateTime.UtcNow.AddMinutes(10));
        var authResponse = CreateAuthResponse(user);
        var userService = new Mock<IUserService>();
        var authService = new Mock<IAuthService>();
        authService
            .Setup(service => service.GetPrincipalFromExpiredToken("access-token"))
            .Returns(CreatePrincipal(user.Email!));
        userService
            .Setup(service => service.GetByEmailAsync(user.Email!))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(user));
        authService
            .Setup(service => service.BuildAuthResponseAsync(user))
            .ReturnsAsync(authResponse);
        var useCase = new RefreshTokenUseCase(userService.Object, authService.Object);

        var result = await useCase.ExecuteAsync(new RefreshTokenRequest("access-token", "refresh-token"));

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Be(authResponse);
    }

    [Fact]
    public async Task CreateProjectUseCase_WhenUserIsUnauthenticated_ReturnsUnauthorized()
    {
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns((string?)null);
        var useCase = new CreateProjectUseCase(projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(new CreateProjectRequest("Project", null));

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        projectService.Verify(service => service.CreateAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task CreateProjectUseCase_WhenServiceCreatesProject_ReturnsCreatedResponse()
    {
        var projectResponse = new ProjectResponse(Guid.NewGuid(), "Project", null, DateTime.UtcNow, null, 0);
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.CreateAsync(It.IsAny<Project>()))
            .ReturnsAsync((Project project) => ServiceResult<Project>.Created(project));
        mapper
            .Setup(service => service.Map<ProjectResponse>(It.IsAny<Project>()))
            .Returns(projectResponse);
        var useCase = new CreateProjectUseCase(projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(new CreateProjectRequest("Project", null));

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().Be(projectResponse);
        projectService.Verify(service => service.CreateAsync(It.Is<Project>(project => project.OwnerId == OwnerId)), Times.Once);
    }

    [Fact]
    public async Task GetProjectByIdUseCase_WhenProjectDoesNotBelongToUser_ReturnsNotFound()
    {
        var projectId = Guid.NewGuid();
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.GetByIdAndOwnerAsync(projectId, OwnerId))
            .ReturnsAsync(ServiceResult<Project>.NotFound("Project not found."));
        var useCase = new GetProjectByIdUseCase(projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(projectId);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        mapper.Verify(service => service.Map<ProjectResponse>(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectUseCase_WhenProjectExists_SoftDeletesAndUpdatesProject()
    {
        var project = Project.Create("Project", null, OwnerId);
        var projectResponse = new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAt, project.UpdatedAt, 0);
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.GetTrackedByIdAndOwnerAsync(project.Id, OwnerId))
            .ReturnsAsync(ServiceResult<Project>.Ok(project));
        projectService
            .Setup(service => service.UpdateAsync(project))
            .ReturnsAsync(ServiceResult<Project>.Ok(project));
        mapper
            .Setup(service => service.Map<ProjectResponse>(project))
            .Returns(projectResponse);
        var useCase = new DeleteProjectUseCase(projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(project.Id);

        result.Success.Should().BeTrue();
        project.IsDeleted.Should().BeTrue();
        project.DeletedBy.Should().Be(Guid.Parse(OwnerId));
        projectService.Verify(service => service.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task GetProjectsPagedUseCase_WhenUserIsAuthenticated_ReturnsOwnerScopedPage()
    {
        var project = Project.Create("Project", null, OwnerId);
        var pagedProjects = new PagedResult<Project>
        {
            Items = [project],
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 1
        };
        var mappedPageItem = new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAt, project.UpdatedAt, 0);
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.GetAllPagedAsync(
                1,
                10,
                It.IsAny<Expression<Func<Project, bool>>>(),
                It.IsAny<Expression<Func<Project, object>>[]>()))
            .ReturnsAsync(ServiceResult<PagedResult<Project>>.Ok(pagedProjects));
        mapper
            .Setup(service => service.Map<IEnumerable<ProjectResponse>>(pagedProjects.Items))
            .Returns([mappedPageItem]);
        var useCase = new GetProjectsPagedUseCase(projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(new GetProjectsPagedRequest(1, 10));

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().ContainSingle().Which.Should().Be(mappedPageItem);
        result.Data.TotalRecords.Should().Be(1);
    }

    [Fact]
    public async Task UpdateProjectUseCase_WhenProjectExists_UpdatesAndReturnsProject()
    {
        var project = Project.Create("Old", null, OwnerId);
        var response = new ProjectResponse(project.Id, "New", "Updated", project.CreatedAt, DateTime.UtcNow, 0);
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.GetTrackedByIdAndOwnerAsync(project.Id, OwnerId))
            .ReturnsAsync(ServiceResult<Project>.Ok(project));
        projectService
            .Setup(service => service.UpdateAsync(project))
            .ReturnsAsync(ServiceResult<Project>.Ok(project));
        mapper
            .Setup(service => service.Map<ProjectResponse>(project))
            .Returns(response);
        var useCase = new UpdateProjectUseCase(projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(project.Id, new UpdateProjectRequest("New", "Updated"));

        result.Success.Should().BeTrue();
        project.Name.Should().Be("New");
        project.Description.Should().Be("Updated");
        project.UpdatedBy.Should().Be(Guid.Parse(OwnerId));
        result.Data.Should().Be(response);
    }

    [Fact]
    public async Task CreateTaskUseCase_WhenProjectIsNotOwnedByUser_ReturnsNotFound()
    {
        var request = new CreateTaskRequest(Guid.NewGuid(), "Task", null);
        var taskService = new Mock<ITaskService>();
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.GetByIdAndOwnerAsync(request.ProjectId, OwnerId))
            .ReturnsAsync(ServiceResult<Project>.NotFound("Project not found."));
        var useCase = new CreateTaskUseCase(taskService.Object, projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        taskService.Verify(service => service.CreateAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task CreateTaskUseCase_WhenProjectIsOwnedByUser_ReturnsCreatedTask()
    {
        var project = Project.Create("Project", null, OwnerId);
        var taskResponse = new TaskResponse(Guid.NewGuid(), project.Id, "Task", null, DomainTaskStatus.Todo, null, TaskPriority.Medium, DateTime.UtcNow, null);
        var taskService = new Mock<ITaskService>();
        var projectService = new Mock<IProjectService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        projectService
            .Setup(service => service.GetByIdAndOwnerAsync(project.Id, OwnerId))
            .ReturnsAsync(ServiceResult<Project>.Ok(project));
        taskService
            .Setup(service => service.CreateAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync((DomainTask task) => ServiceResult<DomainTask>.Created(task));
        mapper
            .Setup(service => service.Map<TaskResponse>(It.IsAny<DomainTask>()))
            .Returns(taskResponse);
        var useCase = new CreateTaskUseCase(taskService.Object, projectService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(new CreateTaskRequest(project.Id, "Task", null));

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().Be(taskResponse);
        taskService.Verify(service => service.CreateAsync(It.Is<DomainTask>(task => task.ProjectId == project.Id)), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskStatusUseCase_WhenTaskExists_UpdatesStatusAndReturnsOk()
    {
        var task = DomainTask.Create(Guid.NewGuid(), "Task", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        var taskResponse = new TaskResponse(task.Id, task.ProjectId, task.Title, task.Description, DomainTaskStatus.Done, task.DueDate, task.Priority, task.CreatedAt, DateTime.UtcNow);
        var taskService = new Mock<ITaskService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        taskService
            .Setup(service => service.GetTrackedByIdAndOwnerAsync(task.Id, OwnerId))
            .ReturnsAsync(ServiceResult<DomainTask>.Ok(task));
        taskService
            .Setup(service => service.UpdateAsync(task))
            .ReturnsAsync(ServiceResult<DomainTask>.Ok(task));
        mapper
            .Setup(service => service.Map<TaskResponse>(task))
            .Returns(taskResponse);
        var useCase = new UpdateTaskStatusUseCase(taskService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(task.Id, new UpdateTaskStatusRequest(DomainTaskStatus.Done));

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        task.Status.Should().Be(DomainTaskStatus.Done);
        task.UpdatedBy.Should().Be(Guid.Parse(OwnerId));
    }

    [Fact]
    public async Task GetTasksByProjectUseCase_WhenTasksExist_ReturnsMappedTasks()
    {
        var projectId = Guid.NewGuid();
        var task = DomainTask.Create(projectId, "Task", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        var response = new TaskResponse(task.Id, projectId, task.Title, task.Description, task.Status, task.DueDate, task.Priority, task.CreatedAt, task.UpdatedAt);
        var taskService = new Mock<ITaskService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        taskService
            .Setup(service => service.GetByProjectIdAndOwnerAsync(projectId, OwnerId))
            .ReturnsAsync(ServiceResult<IReadOnlyList<DomainTask>>.Ok([task]));
        mapper
            .Setup(service => service.Map<IReadOnlyList<TaskResponse>>(It.Is<IReadOnlyList<DomainTask>>(tasks => tasks.Single() == task)))
            .Returns([response]);
        var useCase = new GetTasksByProjectUseCase(taskService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(projectId);

        result.Success.Should().BeTrue();
        result.Data.Should().ContainSingle().Which.Should().Be(response);
    }

    [Fact]
    public async Task DeleteTaskUseCase_WhenTaskExists_SoftDeletesAndUpdatesTask()
    {
        var task = DomainTask.Create(Guid.NewGuid(), "Task", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        var response = new TaskResponse(task.Id, task.ProjectId, task.Title, task.Description, task.Status, task.DueDate, task.Priority, task.CreatedAt, task.UpdatedAt);
        var taskService = new Mock<ITaskService>();
        var currentUser = new Mock<ICurrentUserService>();
        var mapper = new Mock<IMapper>();
        currentUser.Setup(service => service.UserId).Returns(OwnerId);
        taskService
            .Setup(service => service.GetTrackedByIdAndOwnerAsync(task.Id, OwnerId))
            .ReturnsAsync(ServiceResult<DomainTask>.Ok(task));
        taskService
            .Setup(service => service.UpdateAsync(task))
            .ReturnsAsync(ServiceResult<DomainTask>.Ok(task));
        mapper
            .Setup(service => service.Map<TaskResponse>(task))
            .Returns(response);
        var useCase = new DeleteTaskUseCase(taskService.Object, currentUser.Object, mapper.Object);

        var result = await useCase.ExecuteAsync(task.Id);

        result.Success.Should().BeTrue();
        task.IsDeleted.Should().BeTrue();
        task.DeletedBy.Should().Be(Guid.Parse(OwnerId));
        taskService.Verify(service => service.UpdateAsync(task), Times.Once);
    }

    private static AuthResponse CreateAuthResponse(ApplicationUser user) =>
        new(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7),
            AuthenticatedUserDto.FromUser(user, ["User"]));

    private static ClaimsPrincipal CreatePrincipal(string email) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Email, email)]));
}
