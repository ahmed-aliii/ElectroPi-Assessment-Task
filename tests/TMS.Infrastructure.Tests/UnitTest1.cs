using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TMS.Application;
using TMS.Domain;
using TMS.Infrastructure;
using DomainTaskStatus = TMS.Domain.TaskStatus;
using DomainTask = TMS.Domain.Task;
using Task = System.Threading.Tasks.Task;

namespace TMS.Infrastructure.Tests;

public class RepositoryTests
{
    [Fact]
    public async Task ProjectRepositoryGetByIdAndOwnerAsync_WhenProjectBelongsToOwner_ReturnsProjectWithTasks()
    {
        await using var database = await CreateDatabaseAsync();
        var user = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        var project = Project.Create("Project", null, user.Id);
        project.AddTask("Task", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        database.Context.Users.Add(user);
        database.Context.Projects.Add(project);
        await database.Context.SaveChangesAsync();
        var repository = new ProjectRepository(database.Context);

        var result = await repository.GetByIdAndOwnerAsync(project.Id, user.Id);

        result.Should().NotBeNull();
        result!.OwnerId.Should().Be(user.Id);
        result.Tasks.Should().ContainSingle();
    }

    [Fact]
    public async Task ProjectRepositoryGetByIdAndOwnerAsync_WhenOwnerDoesNotMatch_ReturnsNull()
    {
        await using var database = await CreateDatabaseAsync();
        var owner = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        var otherUser = ApplicationUser.Register("other@example.com", "Other", "User");
        var project = Project.Create("Project", null, owner.Id);
        database.Context.Users.AddRange(owner, otherUser);
        database.Context.Projects.Add(project);
        await database.Context.SaveChangesAsync();
        var repository = new ProjectRepository(database.Context);

        var result = await repository.GetByIdAndOwnerAsync(project.Id, otherUser.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ProjectRepositoryGetByIdAndOwnerAsync_WhenProjectIsSoftDeleted_ReturnsNull()
    {
        await using var database = await CreateDatabaseAsync();
        var owner = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        var project = Project.Create("Project", null, owner.Id);
        project.SoftDelete(Guid.NewGuid());
        database.Context.Users.Add(owner);
        database.Context.Projects.Add(project);
        await database.Context.SaveChangesAsync();
        var repository = new ProjectRepository(database.Context);

        var result = await repository.GetByIdAndOwnerAsync(project.Id, owner.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TaskRepositoryGetByProjectIdAndOwnerAsync_ReturnsOnlyActiveTasksForOwnedActiveProject()
    {
        await using var database = await CreateDatabaseAsync();
        var owner = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        var otherUser = ApplicationUser.Register("other@example.com", "Other", "User");
        var ownedProject = Project.Create("Owned", null, owner.Id);
        var otherProject = Project.Create("Other", null, otherUser.Id);
        var visibleTask = ownedProject.AddTask("Visible", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        var deletedTask = ownedProject.AddTask("Deleted", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        deletedTask.SoftDelete(Guid.NewGuid());
        otherProject.AddTask("Other", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        database.Context.Users.AddRange(owner, otherUser);
        database.Context.Projects.AddRange(ownedProject, otherProject);
        await database.Context.SaveChangesAsync();
        var repository = new TaskRepository(database.Context);

        var result = await repository.GetByProjectIdAndOwnerAsync(ownedProject.Id, owner.Id);

        result.Should().ContainSingle();
        result.Single().Id.Should().Be(visibleTask.Id);
    }

    [Fact]
    public async Task TaskRepositoryGetByIdAndOwnerAsync_WhenProjectIsSoftDeleted_ReturnsNull()
    {
        await using var database = await CreateDatabaseAsync();
        var owner = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        var project = Project.Create("Project", null, owner.Id);
        var task = project.AddTask("Task", null, DomainTaskStatus.Todo, null, TaskPriority.Medium);
        project.SoftDelete(Guid.NewGuid());
        database.Context.Users.Add(owner);
        database.Context.Projects.Add(project);
        await database.Context.SaveChangesAsync();
        var repository = new TaskRepository(database.Context);

        var result = await repository.GetByIdAndOwnerAsync(task.Id, owner.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UserRepositoryGetByEmailAsync_WhenUserIsSoftDeleted_ReturnsNull()
    {
        await using var database = await CreateDatabaseAsync();
        var user = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        user.SoftDelete(Guid.NewGuid());
        database.Context.Users.Add(user);
        await database.Context.SaveChangesAsync();
        var repository = new UserRepository(database.Context);

        var result = await repository.GetByEmailAsync(user.Email!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GenericRepositoryGetAllPagedAsync_WhenFilterIsProvided_ReturnsMatchingPage()
    {
        await using var database = await CreateDatabaseAsync();
        var owner = ApplicationUser.Register("owner@example.com", "Ahmed", "Mahmoud");
        var otherUser = ApplicationUser.Register("other@example.com", "Other", "User");
        database.Context.Users.AddRange(owner, otherUser);
        database.Context.Projects.AddRange(
            Project.Create("One", null, owner.Id),
            Project.Create("Two", null, owner.Id),
            Project.Create("Other", null, otherUser.Id));
        await database.Context.SaveChangesAsync();
        var repository = new GenericRepository<Project>(database.Context);

        var result = await repository.GetAllPagedAsync(1, 10, project => project.OwnerId == owner.Id);

        result.TotalRecords.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    private static async Task<TestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(connection, context);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDatabase(SqliteConnection connection, ApplicationDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
