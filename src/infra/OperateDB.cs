using domain;
using infra.context;

namespace infra;

public record OperateDB(MyDbContext Db) : IOperateDB
{
    public void DoIt()
    {
        Db.Database.EnsureDeleted();
        Db.Database.EnsureCreated();

        Db.MyModels.Add(new MyModel() { Id = 1, Name = "test" });
        Db.SaveChanges();

        Db.Database.EnsureDeleted();
    }
}
