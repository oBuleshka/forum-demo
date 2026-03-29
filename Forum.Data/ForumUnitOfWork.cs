using ForumBL.Core.Abstractions.Repositories;
using Forum.Data.Context;

namespace Forum.Data;

public class ForumUnitOfWork : IForumUnitOfWork
{
    private readonly ForumDbContext _dbContext;

    public ForumUnitOfWork(ForumDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
