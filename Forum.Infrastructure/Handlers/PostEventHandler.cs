using ForumBL.Core.Abstractions.Repositories;
using Forum.Domain.Entities;
using Forum.Domain.Events;

namespace Forum.Infrastructure.Handlers;

public class PostEventHandler
{
    private readonly IPostEventRepository _postEventRepository;
    private readonly IForumUnitOfWork _unitOfWork;

    public PostEventHandler(IPostEventRepository postEventRepository, IForumUnitOfWork unitOfWork)
    {
        _postEventRepository = postEventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.Payload is not PostDomainEventPayload payload)
        {
            return;
        }

        var postEvent = new PostEvent
        {
            PostId = payload.PostId,
            UserId = payload.UserId,
            EventType = payload.EventType,
            Message = payload.Message,
            CreatedAt = domainEvent.CreatedAt
        };

        await _postEventRepository.AddAsync(postEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
