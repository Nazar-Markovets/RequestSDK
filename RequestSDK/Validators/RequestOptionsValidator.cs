using RequestSDK.Interfaces;

namespace RequestSDK.Validators;

internal class RequestOptionsValidator : IRequestOptionValidator
{
    public bool ValidateOptionsKeyValue(KeyValuePair<string, string?> valuePair)
    {
        if (string.IsNullOrWhiteSpace(valuePair.Key) || string.IsNullOrWhiteSpace(valuePair.Value)) return false;
        return true;
    }

    public bool ValidateOptionsCollection(string key, params string?[] collectionValues)
    {
        if (string.IsNullOrWhiteSpace(key) || collectionValues.Length <= 0 || collectionValues.All(v => v == null)) return false;
        return true;
    }
}
