using System.Collections;

namespace RequestSDK.Test.ClassData
{
    public class InvalidRoutes : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "htps://stackoverflow.com" };
            yield return new object[] { "https//stackoverflow.com" };
            yield return new object[] { "https:/stackoverflow.com" };
            yield return new object[] { "https!//stackoverflow.com" };
            yield return new object[] { "https://" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
