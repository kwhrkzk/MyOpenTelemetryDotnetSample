using domain;
using Microsoft.Extensions.Logging;

namespace usecase;

[GenerateDecorator]
public record DoB(ILogger<DoB> Logger) : IDoB
{
    public void DoitB(string something1, int something2)
    {
        Logger.LogInformation($"{something1} Did it!");
    }
}
