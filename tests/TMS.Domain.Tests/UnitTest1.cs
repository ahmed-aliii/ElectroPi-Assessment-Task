using FluentAssertions;
using DomainTask = TMS.Domain.Task;

namespace TMS.Domain.Tests;

public class DomainEntityTests
{
    [Fact]
    public void ProjectCreate_WhenNameIsEmpty_ThrowsArgumentException()
    {
        var act = () => Project.Create(" ", "description", "owner-id");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Project name is required.");
    }

    [Fact]
    public void ProjectCreate_WhenOwnerIdIsEmpty_ThrowsArgumentException()
    {
        var act = () => Project.Create("Project", "description", " ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Owner id is required.");
    }

    [Fact]
    public void ProjectCreate_WhenInputIsValid_TrimsValuesAndSetsAuditFields()
    {
        var createdBy = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var project = Project.Create("  Project Alpha  ", "  Important work  ", "owner-id", createdBy);

        project.Id.Should().NotBeEmpty();
        project.OwnerId.Should().Be("owner-id");
        project.Name.Should().Be("Project Alpha");
        project.Description.Should().Be("Important work");
        project.CreatedBy.Should().Be(createdBy);
        project.CreatedAt.Should().BeOnOrAfter(before);
        project.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ProjectUpdate_WhenNameIsEmpty_ThrowsArgumentException()
    {
        var project = Project.Create("Project", null, "owner-id");

        var act = () => project.Update(" ", null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Project name is required.");
    }

    [Fact]
    public void ProjectUpdate_WhenInputIsValid_TrimsValuesAndSetsAuditFields()
    {
        var project = Project.Create("Project", null, "owner-id");
        var updatedBy = Guid.NewGuid();
        var before = DateTime.UtcNow;

        project.Update("  New Name  ", "  New Description  ", updatedBy);

        project.Name.Should().Be("New Name");
        project.Description.Should().Be("New Description");
        project.UpdatedBy.Should().Be(updatedBy);
        project.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void ProjectAddTask_WhenInputIsValid_AddsTaskForProject()
    {
        var project = Project.Create("Project", null, "owner-id");

        var task = project.AddTask(
            "  First task  ",
            "  Task details  ",
            TaskStatus.Todo,
            DateTime.UtcNow.AddDays(1),
            TaskPriority.High);

        task.ProjectId.Should().Be(project.Id);
        task.Title.Should().Be("First task");
        task.Description.Should().Be("Task details");
        project.Tasks.Should().ContainSingle().Which.Should().BeSameAs(task);
    }

    [Fact]
    public void ProjectSoftDelete_WhenCalled_SetsDeleteAuditFields()
    {
        var project = Project.Create("Project", null, "owner-id");
        var deletedBy = Guid.NewGuid();
        var before = DateTime.UtcNow;

        project.SoftDelete(deletedBy);

        project.IsDeleted.Should().BeTrue();
        project.DeletedBy.Should().Be(deletedBy);
        project.DeletedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void TaskCreate_WhenProjectIdIsEmpty_ThrowsArgumentException()
    {
        var act = () => DomainTask.Create(Guid.Empty, "Task", null, TaskStatus.Todo, null, TaskPriority.Medium);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Project id is required.");
    }

    [Fact]
    public void TaskCreate_WhenTitleIsEmpty_ThrowsArgumentException()
    {
        var act = () => DomainTask.Create(Guid.NewGuid(), " ", null, TaskStatus.Todo, null, TaskPriority.Medium);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Task title is required.");
    }

    [Fact]
    public void TaskCreate_WhenInputIsValid_TrimsValuesAndSetsFields()
    {
        var projectId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(3);
        var before = DateTime.UtcNow;

        var task = DomainTask.Create(projectId, "  Ship tests  ", "  Add coverage  ", TaskStatus.InProgress, dueDate, TaskPriority.Critical, createdBy);

        task.Id.Should().NotBeEmpty();
        task.ProjectId.Should().Be(projectId);
        task.Title.Should().Be("Ship tests");
        task.Description.Should().Be("Add coverage");
        task.Status.Should().Be(TaskStatus.InProgress);
        task.DueDate.Should().Be(dueDate);
        task.Priority.Should().Be(TaskPriority.Critical);
        task.CreatedBy.Should().Be(createdBy);
        task.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void TaskUpdateMethods_WhenCalled_UpdateExpectedFieldsAndAudit()
    {
        var task = DomainTask.Create(Guid.NewGuid(), "Task", null, TaskStatus.Todo, null, TaskPriority.Low);
        var updatedBy = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(5);

        task.UpdateDetails("  Updated task  ", "  Updated details  ", updatedBy);
        task.ChangeStatus(TaskStatus.Done, updatedBy);
        task.UpdateDueDate(dueDate, updatedBy);
        task.ChangePriority(TaskPriority.High, updatedBy);

        task.Title.Should().Be("Updated task");
        task.Description.Should().Be("Updated details");
        task.Status.Should().Be(TaskStatus.Done);
        task.DueDate.Should().Be(dueDate);
        task.Priority.Should().Be(TaskPriority.High);
        task.UpdatedBy.Should().Be(updatedBy);
        task.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void TaskSoftDelete_WhenCalled_SetsDeleteAuditFields()
    {
        var task = DomainTask.Create(Guid.NewGuid(), "Task", null, TaskStatus.Todo, null, TaskPriority.Medium);
        var deletedBy = Guid.NewGuid();
        var before = DateTime.UtcNow;

        task.SoftDelete(deletedBy);

        task.IsDeleted.Should().BeTrue();
        task.DeletedBy.Should().Be(deletedBy);
        task.DeletedAt.Should().BeOnOrAfter(before);
    }

    [Theory]
    [InlineData("", "Last", "First name is required.")]
    [InlineData("First", "", "Last name is required.")]
    public void ApplicationUserRegister_WhenRequiredNameIsMissing_ThrowsArgumentException(
        string firstName,
        string lastName,
        string expectedMessage)
    {
        var act = () => ApplicationUser.Register("user@example.com", firstName, lastName);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void ApplicationUserRegister_WhenInputIsValid_SetsIdentityAndAuditFields()
    {
        var before = DateTime.UtcNow;

        var user = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");

        user.Email.Should().Be("user@example.com");
        user.UserName.Should().Be("user@example.com");
        user.FirstName.Should().Be("Ahmed");
        user.LastName.Should().Be("Mahmoud");
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ApplicationUserRefreshToken_WhenTokenMatchesAndIsNotExpired_ReturnsTrue()
    {
        var user = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");
        user.UpdateRefreshToken("refresh-token", DateTime.UtcNow.AddMinutes(5));

        user.IsRefreshTokenValid("refresh-token").Should().BeTrue();
    }

    [Theory]
    [InlineData("wrong-token", 5)]
    [InlineData("refresh-token", -5)]
    public void ApplicationUserRefreshToken_WhenTokenDoesNotMatchOrIsExpired_ReturnsFalse(
        string tokenToCheck,
        int expiryOffsetMinutes)
    {
        var user = ApplicationUser.Register("user@example.com", "Ahmed", "Mahmoud");
        user.UpdateRefreshToken("refresh-token", DateTime.UtcNow.AddMinutes(expiryOffsetMinutes));

        user.IsRefreshTokenValid(tokenToCheck).Should().BeFalse();
    }
}
