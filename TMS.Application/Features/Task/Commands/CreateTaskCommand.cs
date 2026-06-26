using MediatR;

namespace TMS.Application.Features.Task.Commands
{
    public record CreateTaskCommand(CreateTaskRequest Request) : IRequest<ServiceResult<TaskResponse>>;

    public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, ServiceResult<TaskResponse>>
    {
        private readonly ICreateTaskUseCase _useCase;

        public CreateTaskHandler(ICreateTaskUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Request);
    }
}
