using static System.Net.WebRequestMethods;
using System.Net.NetworkInformation;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using System.IO;

namespace RequestSDK.Schemes;

/// <summary>
/// <see href="https://www.iana.org/assignments/http-authschemes/http-authschemes.xhtml"/>
/// </summary>
public sealed class AuthenticationSchemes
{
    public readonly string Basic = "Basic";
    public readonly string Bearer = "Bearer";
    public readonly string Digest = "Digest";
    public readonly string DPoP = "DPoP";
    public readonly string HOBA = "HOBA";
    public readonly string Mutual = "Mutual";
    public readonly string Negotiate = "Negotiate";
    public readonly string OAuth = "OAuth";
    public readonly string SCRAM_SHA_1 = "SCRAM-SHA-1";
    public readonly string SCRAM_SHA_256 = "SCRAM-SHA-256";
    public readonly string vapid = "vapid";
}
