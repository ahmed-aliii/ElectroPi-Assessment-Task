using FluentValidation;
using DomainTaskStatus = TMS.Domain.TaskStatus;
using TaskPriority = TMS.Domain.TaskPriority;

namespace TMS.Application
{
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("Project id is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(200).WithMessage("Task title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Task description cannot exceed 2000 characters.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid task status.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid task priority.");
        }
    }
}
