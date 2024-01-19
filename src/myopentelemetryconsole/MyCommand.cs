using domain;

namespace myopentelemetryconsole;

public class MyCommand(IDoA doA, IDoB doB) : ConsoleAppBase
{
    private IDoA DoA { get; } = doA;
    private IDoB DoB { get; } = doB;


    public void UsecaseA([Option("s")] string something)
    {
        DoA.DoitA(something);

    }

    public void UsecaseB([Option("s1")] string something1, [Option("s2")] int something2)
    {
        DoB.DoitB(something1, something2);
    }
}