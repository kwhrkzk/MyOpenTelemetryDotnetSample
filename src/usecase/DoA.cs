using domain;
using Microsoft.Extensions.Logging;

namespace usecase;

[GenerateDecorator]
public record DoA(ILogger<DoA> Logger, IOperateDB OperateDB) : IDoA
{
    public void DoitA(string something)
    {
        Logger.LogInformation($"{something} Did it!");
        OperateDB.DoIt();
    }
}
