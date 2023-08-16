namespace RequestSDK.Interfaces;

public interface IRequestOptionValidator
{
    public bool ValidateOptionsKeyValue(KeyValuePair<string, string?> pair);

    public bool ValidateOptionsCollection(string key, params string?[] valuesCollection);
}
