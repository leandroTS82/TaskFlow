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

        var response = new CreateJobResponse(result.JobId, result.AlreadyExisted);

        if (result.AlreadyExisted)
            return Ok(response);

        return CreatedAtAction(nameof(GetJob), new { id = result.JobId }, response);
    }

    /// <summary>Gets a job by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJob([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetJobQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Cancels a job that is Pending or Running.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelJob([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelJobCommand(id), ct);
        return Ok(new CancelJobResponse(result.JobId));
    }
}

public record CreateJobRequest(
    string JobType,
    Priority Priority,
    string Payload,
    DateTime? ScheduledAt = null);

public record CreateJobResponse(Guid JobId, bool alreadyExisted = false);
public record CancelJobResponse(Guid JobId);
