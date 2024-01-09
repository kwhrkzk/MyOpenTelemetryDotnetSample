using System.Diagnostics;
using Microsoft.Extensions.Logging;

public class MyCommand(ILogger<MyCommand> logger, MyDbContext db, ActivitySource activitySource) : ConsoleAppBase
{
    private ILogger<MyCommand> Logger { get; } = logger;
    private MyDbContext Db { get; } = db;
    private ActivitySource MyActivitySource { get; } = activitySource;

    [RootCommand]
    public void Exec()
    {
        Logger.LogInformation("hello world");

        Activity? activity = MyActivitySource.StartActivity("mycommand");
        if (activity is null)
            return;

        try
        {
            Db.Database.EnsureDeleted();
            Db.Database.EnsureCreated();

            Db.MyModels.Add(new MyModel() { Id = 1, Name = "test" });
            Db.SaveChanges();

            foreach (var item in Db.MyModels.ToList())
                activity.SetTag(item.Name, item.Id);

            activity.SetStatus(ActivityStatusCode.Ok);

            Db.Database.EnsureDeleted();
        }
        catch (Exception ex)
        {
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        finally
        {
            activity.Dispose();
        }
    }
}