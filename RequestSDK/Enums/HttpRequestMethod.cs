namespace RequestSDK.Enums;

public enum HttpRequestMethod : byte
{
    None = byte.MinValue,
    Get,
    Put,
    Delete,
    Post,
    Head,
    Trace,
    Patch,
    Connect,
    Options,
    Custom
}