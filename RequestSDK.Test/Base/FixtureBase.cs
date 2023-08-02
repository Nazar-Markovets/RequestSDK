using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RequestSDK.Test.Base;

public class FixtureBase
{
    protected readonly ITestOutputHelper Console;

    public FixtureBase(ITestOutputHelper consoleWriter)
    {
        Console = consoleWriter;
    }
}
