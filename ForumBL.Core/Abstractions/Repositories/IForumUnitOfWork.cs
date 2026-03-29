namespace ForumBL.Core.Abstractions.Repositories;

public interface IForumUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
