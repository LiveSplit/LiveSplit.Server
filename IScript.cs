using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit
{
    public interface IScript
    {
        dynamic this[String name] { get; set; }
        dynamic Run();
    }
}
