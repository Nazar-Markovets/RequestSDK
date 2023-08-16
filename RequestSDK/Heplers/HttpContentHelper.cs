using System.Text.Json;
using System.Text;

namespace RequestSDK.Helpers;

public static class HttpContentHelper
{
    public static string Base64Encode(string? plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText ?? string.Empty));
    public static string Base64Decode(string? base64EncodedData) => Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData ?? string.Empty));

    public static string Base64EncodeObject<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        string base64String = Convert.ToBase64String(jsonBytes);
        return base64String;
    }

    public static T? Base64DecodeObject<T>(string base64EncodedJson)
    {
        byte[] jsonBytes = Convert.FromBase64String(base64EncodedJson);
        string json = Encoding.UTF8.GetString(jsonBytes);
        return JsonSerializer.Deserialize<T>(json);
    }

    public static StringContent? ConvertToHttpContent<T>(T? content, string contentType, JsonSerializerOptions serializerOptions = default!) =>
        content == null ? default : new StringContent(JsonSerializer.Serialize(content, serializerOptions),
                                                      Encoding.UTF8, contentType);
}
