using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TMS.Application;

namespace TMS.API
{
    public sealed class FluentValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public FluentValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null)
                    continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                var validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator is null)
                    continue;

                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var errorMessage = string.Join(
                        ", ",
                        result.Errors.Select(e => e.ErrorMessage)
                    );

                    context.Result = new BadRequestObjectResult(
                        ServiceResult<object>.BadRequest(errorMessage)
                    );

                    return;
                }
            }

            await next();
        }
    }
}
