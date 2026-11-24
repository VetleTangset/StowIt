using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StowIt.Core;

public class StowItDbContextFactory : IDesignTimeDbContextFactory<StowItDbContext>
{
    public StowItDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StowItDbContext>()
            .UseSqlite("Data Source=stowit.db")
            .Options;
        return new StowItDbContext(options);
    }
}