using FluentAssertions;
using FluentValidation.TestHelper;
using TMS.Domain;
using DomainTaskStatus = TMS.Domain.TaskStatus;

namespace TMS.Application.Tests;

public class ValidatorTests
{
    [Fact]
    public void LoginRequestValidator_WhenRequestIsValid_HasNoErrors()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest("user@example.com", "Password1!");

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginRequestValidator_WhenEmailAndPasswordAreInvalid_HasExpectedErrors()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest("not-an-email", "");

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format.");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void RegisterRequestValidator_WhenRequestIsValid_HasNoErrors()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("user@example.com", "Ahmed", "Mahmoud", "Password1!");

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterRequestValidator_WhenPasswordMissesRules_HasExpectedErrors()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("bad-email", "", new string('x', 51), "short");

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 50 characters.");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters.");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number.");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one special character.");
    }

    [Fact]
    public void RefreshTokenRequestValidator_WhenTokensAreMissing_HasExpectedErrors()
    {
        var validator = new RefreshTokenRequestValidator();
        var request = new RefreshTokenRequest("", "");

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.AccessToken)
            .WithErrorMessage("Access token is required.");
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateProjectRequestValidator_WhenNameIsMissing_HasExpectedError(string name)
    {
        var validator = new CreateProjectRequestValidator();
        var request = new CreateProjectRequest(name, "description");

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Project name is required.");
    }

    [Fact]
    public void CreateProjectRequestValidator_WhenNameIsTooLong_HasExpectedError()
    {
        var validator = new CreateProjectRequestValidator();
        var request = new CreateProjectRequest(new string('x', 201), null);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Project name cannot exceed 200 characters.");
    }

    [Fact]
    public void UpdateProjectRequestValidator_WhenRequestIsValid_HasNoErrors()
    {
        var validator = new UpdateProjectRequestValidator();
        var request = new UpdateProjectRequest("Project", "description");

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0, 10, "PageNumber", "Page number must be at least 1.")]
    [InlineData(1, 0, "PageSize", "Page size must be between 1 and 100.")]
    [InlineData(1, 101, "PageSize", "Page size must be between 1 and 100.")]
    public void GetProjectsPagedRequestValidator_WhenPagingIsInvalid_HasExpectedError(
        int pageNumber,
        int pageSize,
        string propertyName,
        string message)
    {
        var validator = new GetProjectsPagedRequestValidator();
        var request = new GetProjectsPagedRequest(pageNumber, pageSize);

        var result = validator.TestValidate(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == propertyName &&
            error.ErrorMessage == message);
    }

    [Fact]
    public void CreateTaskRequestValidator_WhenRequestIsValid_HasNoErrors()
    {
        var validator = new CreateTaskRequestValidator();
        var request = new CreateTaskRequest(Guid.NewGuid(), "Task", "description", DomainTaskStatus.Todo, DateTime.UtcNow, TaskPriority.Medium);

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTaskRequestValidator_WhenRequestIsInvalid_HasExpectedErrors()
    {
        var validator = new CreateTaskRequestValidator();
        var request = new CreateTaskRequest(
            Guid.Empty,
            "",
            new string('x', 2001),
            (DomainTaskStatus)999,
            null,
            (TaskPriority)999);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ProjectId)
            .WithErrorMessage("Project id is required.");
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Task title is required.");
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Task description cannot exceed 2000 characters.");
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid task status.");
        result.ShouldHaveValidationErrorFor(x => x.Priority)
            .WithErrorMessage("Invalid task priority.");
    }

    [Fact]
    public void UpdateTaskStatusRequestValidator_WhenStatusIsInvalid_HasExpectedError()
    {
        var validator = new UpdateTaskStatusRequestValidator();
        var request = new UpdateTaskStatusRequest((DomainTaskStatus)999);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid task status.");
    }
}
