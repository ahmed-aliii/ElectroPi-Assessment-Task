using FluentValidation;
using DomainTaskStatus = TMS.Domain.TaskStatus;

namespace TMS.Application
{
    public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
    {
        public UpdateTaskStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid task status.");
        }
    }
}
