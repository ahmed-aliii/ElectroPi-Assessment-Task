using MediatR;

namespace TMS.Application.Features.Task.Commands
{
    public record UpdateTaskStatusCommand(Guid Id, UpdateTaskStatusRequest Request) : IRequest<ServiceResult<TaskResponse>>;

    public class UpdateTaskStatusHandler : IRequestHandler<UpdateTaskStatusCommand, ServiceResult<TaskResponse>>
    {
        private readonly IUpdateTaskStatusUseCase _useCase;

        public UpdateTaskStatusHandler(IUpdateTaskStatusUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<TaskResponse>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Id, request.Request);
    }
}
