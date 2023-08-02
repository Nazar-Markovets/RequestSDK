namespace RequestSDK.Services;

public partial class RequestService
{
    public sealed class HttpClientSettings
    {
        public byte HttpClientId { get; set; }
        public string HttpClientName { get; set; } = default!;
        public Uri BaseAddress { get; set; } = default!;
        public byte RetryCount { get; set; } = 0;
        public int MilliSecondsSleep { get; set; } = 300;
    }
}