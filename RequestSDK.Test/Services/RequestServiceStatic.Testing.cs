using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.ClassData;
using RequestSDK.Test.Integration;
using System.Text.Json;
using System.Web;
using Xunit.Abstractions;

namespace RequestSDK.Test.Services
{
    [Trait("Test", "Request Static Methods")]
    public class RequestService_Static_Testing : FixtureBase
    {
        public RequestService_Static_Testing(ITestOutputHelper consoleWriter, ServerInstanceRunner server) : base(consoleWriter, server) { }

        #region Append Path

        private const string target = "https://stackoverflow.com/questions/9110419/test-parameterization-in-xunit-net-similar-to-nunit";
        private const string targetParametrized = "https://stackoverflow.com/questions/9110419/test-parameterization-in-xunit-net-similar-to-nunit?parameter=1";
        private const string targetBase = "https://stackoverflow.com";
        private const string targetBaseParametrized = "https://stackoverflow.com?parameter=1";
        private const string combinePath = "questions/9110419/test-parameterization-in-xunit-net-similar-to-nunit";
        private const string combinePathParametrized = "questions/9110419/test-parameterization-in-xunit-net-similar-to-nunit?name=string";

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Default")]
        public void Request_AppendPath()
        {
            string actual = RequestService.AppendPathSafely(targetBase, combinePath);
            Assert.Equal(target, actual);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Encoding")]
        public void Request_AppendPath_Encoding()
        {
            string actual = RequestService.AppendPathSafely(targetBase, combinePath, autoEncode: true);
            Assert.Equal(HttpUtility.UrlEncode(target), actual);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Clear parameters")]
        public void Request_AppendPath_ClearParameters()
        {
            string actual = RequestService.AppendPathSafely(targetBaseParametrized, combinePath);
            Assert.Equal(target, actual);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Save parameters")]
        public void Request_AppendPath_SaveParameters()
        {
            string actual = RequestService.AppendPathSafely(targetBaseParametrized, combinePath, cleanExistsParameters: false);
            Assert.Equal(targetParametrized, actual);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Clear parameters & Encoding")]
        public void Request_AppendPath_ClearParameters_Encoding()
        {
            string actual = RequestService.AppendPathSafely(targetBaseParametrized, combinePath, autoEncode: true);
            Assert.Equal(HttpUtility.UrlEncode(target), actual);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Append parameters & Encoding")]
        public void Request_AppendPath_AppendParameters_Encoding()
        {
            string actual = RequestService.AppendPathSafely(targetBaseParametrized, combinePathParametrized, cleanExistsParameters: false, autoEncode: true);
            Assert.Equal(HttpUtility.UrlEncode(target + "?name=string&parameter=1"), actual);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.AppendPathSafely)}. Append parameters")]
        public void Request_AppendPath_AppendParameters()
        {
            string actual = RequestService.AppendPathSafely(targetBaseParametrized, combinePathParametrized, cleanExistsParameters: false);
            Assert.Equal(target + "?name=string&parameter=1", actual);
        }

        #endregion Append Path

        #region Encode Base64

        class TestDecodeClass
        {
            public int Id { get; set; } = new Random().Next(0, 30000);
            public Guid Identifier { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "TestName";
            public bool Male { get; set; } = true;
            
        }


        [Fact(DisplayName = $"Static. {nameof(RequestService.Base64Encode)}. Object")]
        public void Request_Base64Encode_Object()
        {
            TestDecodeClass originalObject = new();
            string encodedJson = string.Empty;
            string json = JsonSerializer.Serialize(originalObject);
            Assert.Null(Record.Exception(() => encodedJson = RequestService.Base64EncodeObject(originalObject)));
            Assert.NotEqual(encodedJson, json);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.Base64Decode)}. Object")]
        public void Request_Base64Decode_Object()
        {
            TestDecodeClass expectedObjecct = new() { Id = 21579, Identifier = new Guid("0cd7a13e-38dd-4151-81cc-9447f8c6bafa"), Male = true, Name = "TestName" };
            TestDecodeClass? decodedObject = null;
            string encodedJson = "eyJJZCI6MjE1NzksIklkZW50aWZpZXIiOiIwY2Q3YTEzZS0zOGRkLTQxNTEtODFjYy05NDQ3ZjhjNmJhZmEiLCJOYW1lIjoiVGVzdE5hbWUiLCJNYWxlIjp0cnVlfQ==";
            Assert.Null(Record.Exception(() => decodedObject = RequestService.Base64DecodeObject<TestDecodeClass>(encodedJson)));
            Assert.Equivalent(expectedObjecct, decodedObject);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.Base64Encode)}. String")]
        public void Request_Base64Encode_String()
        {
            string encodedString = string.Empty;
            Assert.Null(Record.Exception(() => encodedString = RequestService.Base64Encode("Request_Base64_String")));
            Assert.Equal("UmVxdWVzdF9CYXNlNjRfU3RyaW5n", encodedString);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.Base64Decode)}. String")]
        public void Request_Base64Decode_String()
        {
            string decodedString = string.Empty;
            Assert.Null(Record.Exception(() => decodedString = RequestService.Base64Decode("UmVxdWVzdF9CYXNlNjRfU3RyaW5n")));
            Assert.Equal("Request_Base64_String", decodedString);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.Base64Encode)}. Null")]
        public void Request_Base64Encode_Null()
        {
            string? encodedString = null;
            Assert.Null(Record.Exception(() => encodedString = RequestService.Base64Encode(null)));
            Assert.Equal(string.Empty, encodedString);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.Base64Decode)}. Null")]
        public void Request_Base64Decode_Null()
        {
            string? decodedString = null;
            Assert.Null(Record.Exception(() => decodedString = RequestService.Base64Decode(null)));
            Assert.Equal(string.Empty, decodedString);
        }

        #endregion Encode Base64

        #region Request Parameter

        [Fact(DisplayName = $"Static. {nameof(RequestService.RequestParameter)}. Empty")]
        public void Request_RequestParameter_Empty()
        {
            KeyValuePair<string, string?> parameter = default;
            Assert.Null(Record.Exception(() => parameter = RequestService.RequestParameter(string.Empty, string.Empty)));
            Assert.Equivalent(new KeyValuePair<string, string>(string.Empty, string.Empty), parameter);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.RequestParameter)}. Both Null")]
        public void Request_RequestParameter_Null()
        {
            KeyValuePair<string, string?> parameter = default;
            Assert.Null(Record.Exception(() => parameter = RequestService.RequestParameter(null, null)));
            Assert.Equivalent(new KeyValuePair<string, string>(string.Empty, string.Empty), parameter);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.RequestParameter)}. Value Null")]
        public void Request_RequestParameter_ValueNull()
        {
            KeyValuePair<string, string?> parameter = default;
            Assert.Null(Record.Exception(() => parameter = RequestService.RequestParameter("Key", null)));
            Assert.Equivalent(new KeyValuePair<string, string>("Key", string.Empty), parameter);
        }
        #endregion Request Parameter

        #region BasePath

        [Fact(DisplayName = $"Static. {nameof(RequestService.GetBasePath)}. NULL String Value")]
        public void Request_Base_NullString()
        {
            string? path = string.Empty;
            Assert.Null(Record.Exception(() => path = RequestService.GetBasePath(path: default(string)!)));
            Assert.Null(path);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.GetBasePath)}. NULL Uri Value")]
        public void Request_Base_NullUri()
        {
            string? path = string.Empty;
            Assert.Null(Record.Exception(() => path = RequestService.GetBasePath(path: default(Uri)!)));
            Assert.Null(path);
        }

        [Fact(DisplayName = $"Static. {nameof(RequestService.GetBasePath)}. Empty String Value")]
        public void Request_Base_EmptyString()
        {
            Assert.Throws<InvalidCastException>(() => RequestService.GetBasePath(string.Empty));
        }

        [Theory(DisplayName = $"Static. {nameof(RequestService.GetBasePath)}. Invalid String Value")]
        [ClassData(typeof(InvalidRoutes))]
        public void Request_Base_InvalidString(string invalidRoute)
        {
            ThrowsWithMessage<InvalidCastException>(() => RequestService.GetBasePath(invalidRoute), $"Invalid route is allowed. Route : {invalidRoute}");
        }

        [Theory(DisplayName = $"Static. {nameof(RequestService.GetBasePath)}. Invalid Uri Value")]
        [ClassData(typeof(InvalidRoutes))]
        public void Request_Base_InvalidUri(string invalidRoute)
        {
            ThrowsWithMessage<UriFormatException>(() => RequestService.GetBasePath(new Uri(invalidRoute)), $"Invalid route is allowed. Route : {invalidRoute}");
        }


        # endregion BasePath
    }
}
