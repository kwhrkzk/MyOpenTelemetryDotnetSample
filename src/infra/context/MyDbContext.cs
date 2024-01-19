
using Microsoft.EntityFrameworkCore;

namespace infra.context;
public class MyDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<MyModel> MyModels { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("Data Source=mysqlite.db");
    }
}