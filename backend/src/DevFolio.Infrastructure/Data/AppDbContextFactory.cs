using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DevFolio.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // This points to the SQLite database file. 
        // The migration tool will use this to create the schema.
        optionsBuilder.UseSqlite("Data Source=devfolio.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}