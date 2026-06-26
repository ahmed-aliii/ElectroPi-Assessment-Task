using MediatR;

namespace TMS.Application.Features.Task.Commands
{
    public record DeleteTaskCommand(Guid Id) : IRequest<ServiceResult<TaskResponse>>;

    public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, ServiceResult<TaskResponse>>
    {
        private readonly IDeleteTaskUseCase _useCase;

        public DeleteTaskHandler(IDeleteTaskUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<TaskResponse>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Id);
    }
}
