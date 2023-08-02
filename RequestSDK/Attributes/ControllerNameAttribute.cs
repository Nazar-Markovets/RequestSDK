namespace RequestSDK.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ControllerNameAttribute : Attribute
{
    internal readonly string ControllerName;

    public ControllerNameAttribute(string controllerName) => ControllerName = controllerName;
}