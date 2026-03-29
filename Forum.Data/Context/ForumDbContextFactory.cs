using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Forum.Data.Context;

public class ForumDbContextFactory : IDesignTimeDbContextFactory<ForumDbContext>
{
    public ForumDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ForumDbContext>();
        optionsBuilder.UseSqlServer("Data Source=\"localhost, 1433\";Initial Catalog=ForumDB;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=Orinda1433;Trust Server Certificate=True");

        return new ForumDbContext(optionsBuilder.Options);
    }
}
