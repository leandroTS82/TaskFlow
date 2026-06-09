using Mediator;

namespace TaskFlow.Application.Commands.CancelJob;

public record CancelJobCommand(Guid JobId) : IRequest<CancelJobResult>;

public record CancelJobResult(Guid JobId);
