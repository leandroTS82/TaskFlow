using Mediator;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Commands.CancelJob;
using TaskFlow.Application.Commands.CreateJob;
using TaskFlow.Application.Queries.GetAllJobs;
using TaskFlow.Application.Queries.GetJob;
using TaskFlow.Domain.Enums;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns all jobs.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllJobs(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllJobsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Creates a new job.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateJobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateJob(
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        [FromBody] CreateJobRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new { error = "The 'Idempotency-Key' header is required." });

        var command = new CreateJobCommand(
            request.JobType,
            request.Priority,
            request.Payload,
            idempotencyKey,
            request.ScheduledAt);

        var result = await _mediator.Send(command, ct);

        var response = new CreateJobResponse(result.JobId);

        if (result.AlreadyExisted)
            return Ok(response);

        return Ok(response);
    }
}

public record CreateJobRequest(
    string JobType,
    Priority Priority,
    string Payload,
    DateTime? ScheduledAt = null);

public record CreateJobResponse(Guid JobId);
