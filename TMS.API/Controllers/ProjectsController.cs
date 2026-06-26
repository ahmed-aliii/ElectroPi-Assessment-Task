using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TMS.Application;
using TMS.Application.Features.Project.Commands;
using TMS.Application.Features.Project.Queries;

namespace TMS.API.Controllers
{
    [Authorize]
    [SwaggerTag("Project management")]
    public class ProjectsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [SwaggerOperation(Summary = "Get paginated projects for the current user")]
        public async Task<IActionResult> GetPaged([FromQuery] GetProjectsPagedRequest request)
            => ApiResponse.FromResult(this, await _mediator.Send(new GetProjectsPagedQuery(request)));

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get project by id")]
        public async Task<IActionResult> GetById(Guid id)
            => ApiResponse.FromResult(this, await _mediator.Send(new GetProjectByIdQuery(id)));

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new project")]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
            => ApiResponse.FromResult(this, await _mediator.Send(new CreateProjectCommand(request)));

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update a project")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
            => ApiResponse.FromResult(this, await _mediator.Send(new UpdateProjectCommand(id, request)));

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Soft-delete a project")]
        public async Task<IActionResult> Delete(Guid id)
            => ApiResponse.FromResult(this, await _mediator.Send(new DeleteProjectCommand(id)));
    }
}
