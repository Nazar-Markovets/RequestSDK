namespace RequestSDK.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ControllerHttpMethodAttribute : Attribute
{
    internal readonly Enums.HttpRequestMethod HttpRequestMethod;

    public ControllerHttpMethodAttribute(Enums.HttpRequestMethod httpRequest) => HttpRequestMethod = httpRequest;
}