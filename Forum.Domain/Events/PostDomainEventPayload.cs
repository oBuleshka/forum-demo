using Forum.Domain.Enums;

namespace Forum.Domain.Events;

public record PostDomainEventPayload(
    Guid PostId,
    Guid UserId,
    PostEventType EventType,
    string Message);
