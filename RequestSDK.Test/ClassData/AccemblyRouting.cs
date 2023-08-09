using RequestSDK.Attributes;

namespace RequestSDK.Test.ClassData;

public class AccemblyRouting
{

    [ControllerName("Action")]
    public static class ActionRouting
    {
        [ControllerHttpMethod(Enums.HttpRequestMethod.Delete)]
        public const string DeleteMessage = "delete";

        [ControllerHttpMethod(Enums.HttpRequestMethod.Patch)]
        public const string UpdateMessage = "message/update";

    }

    [ControllerName("Home")]
    public static class HomeRouting
    {
        [ControllerHttpMethod(Enums.HttpRequestMethod.Delete)]
        public const string DeleteMessage = "delete";

        [ControllerHttpMethod(Enums.HttpRequestMethod.Patch)]
        public const string UpdateMessage = "message/update";

    }
}
