using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TMS.Application;
using TMS.Application.Features.Task.Commands;
using TMS.Application.Features.Task.Queries;

namespace TMS.API.Controllers
{
    [Authorize]
    [SwaggerTag("Task management")]
    public class TasksController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new task")]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
            => ApiResponse.FromResult(this, await _mediator.Send(new CreateTaskCommand(request)));

        [HttpGet("project/{projectId:guid}")]
        [SwaggerOperation(Summary = "Get tasks by project")]
        public async Task<IActionResult> GetByProject(Guid projectId)
            => ApiResponse.FromResult(this, await _mediator.Send(new GetTasksByProjectQuery(projectId)));

        [HttpPatch("{id:guid}/status")]
        [SwaggerOperation(Summary = "Update task status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
            => ApiResponse.FromResult(this, await _mediator.Send(new UpdateTaskStatusCommand(id, request)));

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Soft-delete a task")]
        public async Task<IActionResult> Delete(Guid id)
            => ApiResponse.FromResult(this, await _mediator.Send(new DeleteTaskCommand(id)));
    }
}
