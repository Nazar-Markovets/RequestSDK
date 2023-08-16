using RequestSDK.Interfaces;

namespace RequestSDK.Builders;

internal class RequestOptionsBuilder : OptionsBuilder
{
    private readonly Dictionary<string, HashSet<string>> _optionPairs;

    internal RequestOptionsBuilder(IRequestOptionValidator requestOptionValidator) : base(requestOptionValidator)
    {
        _optionPairs = new Dictionary<string, HashSet<string>>();
    }

    private static HashSet<string> FillKeyCollection(ref HashSet<string>? definedValues, params string?[] valuesCollection)
    {
        definedValues ??= new HashSet<string>();
        foreach (string? value in valuesCollection.Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            definedValues.Add(value!);
        }
        return definedValues;
    }

    private void AddCore(string key, params string?[] valuesCollection)
    {
        if (_optionPairs.TryGetValue(key, out HashSet<string>? definedValues))
        {
            FillKeyCollection(ref definedValues, valuesCollection);
        }
        else
        {
            _optionPairs.Add(key, FillKeyCollection(ref definedValues, valuesCollection));
        }
    }

    public override RequestOptionsBuilder AddPair(KeyValuePair<string, string> keyValuePair)
    {
        if (RequestOptionValidator.ValidateOptionsKeyValue(keyValuePair!)) AddCore(keyValuePair.Key, keyValuePair.Value);

        return this;
    }

    public override RequestOptionsBuilder Add(string key, params string?[] valuesCollection)
    {
        if (RequestOptionValidator.ValidateOptionsCollection(key, valuesCollection) == false) AddCore(key, valuesCollection);

        return this;
    }

    public override OptionsBuilder AddRange(params KeyValuePair<string, string?>[] pairs)
    {
        IEnumerable<KeyValuePair<string, string?>> sourtedCollection = pairs.Where(kv => RequestOptionValidator.ValidateOptionsKeyValue(kv));
        foreach (KeyValuePair<string, string?> option in pairs)
        {
            AddCore(option.Key, option.Value);
        }
        return this;
    }

    public static string GetJoinedValues(params string?[] values) => string.Join(',', values.Where(v => !string.IsNullOrWhiteSpace(v)));

    internal override Dictionary<string, string> Build() => _optionPairs.ToDictionary(kv => kv.Key, kv => GetJoinedValues(kv.Value.ToArray()));

}