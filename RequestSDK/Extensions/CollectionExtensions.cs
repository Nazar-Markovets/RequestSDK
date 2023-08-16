namespace RequestSDK.Extensions;

internal static class CollectionExtensions
{
    public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue>? dictionary, Action<KeyValuePair<TKey, TValue>> action) where TKey : notnull
    {
        if (dictionary == null) return;
        foreach (var kvp in dictionary)
        {
            action(kvp);
        }
    }
}
