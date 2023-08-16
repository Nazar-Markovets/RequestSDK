using RequestSDK.Interfaces;

namespace RequestSDK.Builders;

public abstract class OptionsBuilder
{
    protected readonly IRequestOptionValidator RequestOptionValidator;

    public OptionsBuilder(IRequestOptionValidator requestOptionValidator)
    {
        RequestOptionValidator = requestOptionValidator;
    }
    public abstract OptionsBuilder AddPair(KeyValuePair<string, string> option);

    public abstract OptionsBuilder AddRange(params KeyValuePair<string, string?>[] options);

    public abstract OptionsBuilder Add(string optionKey, params string?[] optionValuesCollection);

    internal abstract Dictionary<string, string> Build();
}
