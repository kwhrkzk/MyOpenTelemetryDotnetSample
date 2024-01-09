
using Microsoft.EntityFrameworkCore;

public class MyDbContext(DbContextOptions options): DbContext(options) {

    public DbSet<MyModel> MyModels { get; set; }
}