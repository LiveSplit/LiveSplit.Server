using System;

namespace LiveSplit
{
    public interface IScript
    {
        dynamic this[String name] { get; set; }
        dynamic Run();
    }
}
