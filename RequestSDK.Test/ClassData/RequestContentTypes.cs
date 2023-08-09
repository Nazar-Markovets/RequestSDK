using System.Collections;

namespace RequestSDK.Test.ClassData;

public sealed class RequestContentTypes : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, false };
        yield return new object[] { 1.0, false };
        yield return new object[] { true, false };
        yield return new object[] { "string", false };
        yield return new object[] { new List<string> { "string" }, true };
        yield return new object[] { new { Name = "Nazar", LastName = "Markovets" }, true };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
