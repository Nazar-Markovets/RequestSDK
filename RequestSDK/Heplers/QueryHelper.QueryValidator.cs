namespace RequestSDK.Helpers;

public static partial class QueryHelper
{
    public static class QueryValidator
    {
        public static void ThrowIfNotAbsolute(string absolutePath) => _ = 
            Uri.IsWellFormedUriString(absolutePath, UriKind.Absolute) ? true 
            : throw new UriFormatException($"Can't append base path of the current format: '{absolutePath}'");

        public static void ThrowIfNotAbsolute(Uri absoluteUri) => _ =
            Uri.IsWellFormedUriString(absoluteUri.ToString(), UriKind.Absolute) ? true
            : throw new UriFormatException($"Can't append base path of the current format: '{absoluteUri}'");
    }
}


