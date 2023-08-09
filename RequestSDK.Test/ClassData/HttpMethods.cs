using System.Collections;

namespace RequestSDK.Test.ClassData;

public sealed class HttpMethods : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { HttpMethod.Get, "action/get" };
        yield return new object[] { HttpMethod.Get, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Get, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Get, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Get, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Get, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Get, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Get, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Get, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Get, "action/get?" };
        yield return new object[] { HttpMethod.Get, "action/get/" };

        yield return new object[] { HttpMethod.Post, "action/get" };
        yield return new object[] { HttpMethod.Post, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Post, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Post, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Post, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Post, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Post, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Post, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Post, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Post, "action/get?" };
        yield return new object[] { HttpMethod.Post, "action/get/" };

        yield return new object[] { HttpMethod.Put, "action/get" };
        yield return new object[] { HttpMethod.Put, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Put, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Put, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Put, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Put, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Put, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Put, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Put, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Put, "action/get?" };
        yield return new object[] { HttpMethod.Put, "action/get/" };

        yield return new object[] { HttpMethod.Patch, "action/get" };
        yield return new object[] { HttpMethod.Patch, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Patch, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Patch, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Patch, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Patch, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Patch, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Patch, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Patch, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Patch, "action/get?" };
        yield return new object[] { HttpMethod.Patch, "action/get/" };

        yield return new object[] { HttpMethod.Delete, "action/get" };
        yield return new object[] { HttpMethod.Delete, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Delete, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Delete, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Delete, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Delete, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Delete, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Delete, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Delete, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Delete, "action/get?" };
        yield return new object[] { HttpMethod.Delete, "action/get/" };

        yield return new object[] { HttpMethod.Head, "action/get" };
        yield return new object[] { HttpMethod.Head, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Head, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Head, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Head, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Head, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Head, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Head, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Head, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Head, "action/get?" };
        yield return new object[] { HttpMethod.Head, "action/get/" };

        yield return new object[] { HttpMethod.Options, "action/get" };
        yield return new object[] { HttpMethod.Options, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Options, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Options, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Options, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Options, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Options, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Options, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Options, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Options, "action/get?" };
        yield return new object[] { HttpMethod.Options, "action/get/" };

        yield return new object[] { HttpMethod.Trace, "action/get" };
        yield return new object[] { HttpMethod.Trace, "ACTION/GET/" };
        yield return new object[] { HttpMethod.Trace, "action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Trace, "Action/get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Trace, "action/Get?trace=1&name=test" };
        yield return new object[] { HttpMethod.Trace, "action/get?Trace=1&name=Test" };
        yield return new object[] { HttpMethod.Trace, "action/get?TRACE=1&NAME=TEST" };
        yield return new object[] { HttpMethod.Trace, "action/get?TRACE=1&" };
        yield return new object[] { HttpMethod.Trace, "action/get?TRACE=" };
        yield return new object[] { HttpMethod.Trace, "action/get?" };
        yield return new object[] { HttpMethod.Trace, "action/get/" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
