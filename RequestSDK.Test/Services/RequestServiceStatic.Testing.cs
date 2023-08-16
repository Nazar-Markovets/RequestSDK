using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

using RequestSDK.Helpers;
using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.ClassData;
using RequestSDK.Test.Integration;

using System;
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

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Default")]
        public void Request_AppendPath()
        {
            string actual = QueryHelper.AppendPathSafely(targetBase, combinePath);
            Assert.Equal(target, actual);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Encoding")]
        public void Request_AppendPath_Encoding()
        {
            string actual = QueryHelper.AppendPathSafely(targetBase, combinePath, autoEncode: true);
            Assert.Equal(HttpUtility.UrlEncode(target), actual);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Clear parameters")]
        public void Request_AppendPath_ClearParameters()
        {
            string actual = QueryHelper.AppendPathSafely(targetBaseParametrized, combinePath);
            Assert.Equal(target, actual);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Save parameters")]
        public void Request_AppendPath_SaveParameters()
        {
            string actual = QueryHelper.AppendPathSafely(targetBaseParametrized, combinePath, cleanExistsParameters: false);
            Assert.Equal(targetParametrized, actual);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Clear parameters & Encoding")]
        public void Request_AppendPath_ClearParameters_Encoding()
        {
            string actual = QueryHelper.AppendPathSafely(targetBaseParametrized, combinePath, autoEncode: true);
            Assert.Equal(HttpUtility.UrlEncode(target), actual);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Append parameters & Encoding")]
        public void Request_AppendPath_AppendParameters_Encoding()
        {
            string actual = QueryHelper.AppendPathSafely(targetBaseParametrized, combinePathParametrized, cleanExistsParameters: false, autoEncode: true);
            Assert.Equal(HttpUtility.UrlEncode(target + "?name=string&parameter=1"), actual);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.AppendPathSafely)}. Append parameters")]
        public void Request_AppendPath_AppendParameters()
        {
            string actual = QueryHelper.AppendPathSafely(targetBaseParametrized, combinePathParametrized, cleanExistsParameters: false);
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


        [Fact(DisplayName = $"Static. {nameof(HttpContentHelper.Base64EncodeObject)}. Object")]
        public void Request_Base64Encode_Object()
        {
            TestDecodeClass originalObject = new();
            string encodedJson = string.Empty;
            string json = JsonSerializer.Serialize(originalObject);
            Assert.Null(Record.Exception(() => encodedJson = HttpContentHelper.Base64EncodeObject(originalObject)));
            Assert.NotEqual(encodedJson, json);
        }

        [Fact(DisplayName = $"Static. {nameof(HttpContentHelper.Base64DecodeObject)}. Object")]
        public void Request_Base64Decode_Object()
        {
            TestDecodeClass expectedObjecct = new() { Id = 21579, Identifier = new Guid("0cd7a13e-38dd-4151-81cc-9447f8c6bafa"), Male = true, Name = "TestName" };
            TestDecodeClass? decodedObject = null;
            string encodedJson = "eyJJZCI6MjE1NzksIklkZW50aWZpZXIiOiIwY2Q3YTEzZS0zOGRkLTQxNTEtODFjYy05NDQ3ZjhjNmJhZmEiLCJOYW1lIjoiVGVzdE5hbWUiLCJNYWxlIjp0cnVlfQ==";
            Assert.Null(Record.Exception(() => decodedObject = HttpContentHelper.Base64DecodeObject<TestDecodeClass>(encodedJson)));
            Assert.Equivalent(expectedObjecct, decodedObject);
        }

        [Fact(DisplayName = $"Static. {nameof(HttpContentHelper.Base64Encode)}. String")]
        public void Request_Base64Encode_String()
        {
            string encodedString = string.Empty;
            Assert.Null(Record.Exception(() => encodedString = HttpContentHelper.Base64Encode("Request_Base64_String")));
            Assert.Equal("UmVxdWVzdF9CYXNlNjRfU3RyaW5n", encodedString);
        }

        [Fact(DisplayName = $"Static. {nameof(HttpContentHelper.Base64Decode)}. String")]
        public void Request_Base64Decode_String()
        {
            string decodedString = string.Empty;
            Assert.Null(Record.Exception(() => decodedString = HttpContentHelper.Base64Decode("UmVxdWVzdF9CYXNlNjRfU3RyaW5n")));
            Assert.Equal("Request_Base64_String", decodedString);
        }

        [Fact(DisplayName = $"Static. {nameof(HttpContentHelper.Base64Encode)}. Null")]
        public void Request_Base64Encode_Null()
        {
            string? encodedString = null;
            Assert.Null(Record.Exception(() => encodedString = HttpContentHelper.Base64Encode(null)));
            Assert.Equal(string.Empty, encodedString);
        }

        [Fact(DisplayName = $"Static. {nameof(HttpContentHelper.Base64Decode)}. Null")]
        public void Request_Base64Decode_Null()
        {
            string? decodedString = null;
            Assert.Null(Record.Exception(() => decodedString = HttpContentHelper.Base64Decode(null)));
            Assert.Equal(string.Empty, decodedString);
        }

        #endregion Encode Base64

        #region Request Parameter

        //[Fact(DisplayName = $"Static. {nameof(RequestService.QueryParameter)}. Empty")]
        //public void Request_RequestParameter_Empty()
        //{
        //    KeyValuePair<string, string> parameter = default;
        //    Assert.Null(Record.Exception(() => parameter = RequestService.QueryParameter(string.Empty, string.Empty)));
        //    Assert.Equivalent(new KeyValuePair<string, string>(string.Empty, string.Empty), parameter);
        //}

        //[Fact(DisplayName = $"Static. {nameof(RequestService.QueryParameter)}. Both Null")]
        //public void Request_RequestParameter_Null()
        //{
        //    KeyValuePair<string, string> parameter = default;
        //    Assert.Null(Record.Exception(() => parameter = RequestService.QueryParameter(null!, null)));
        //    Assert.Equivalent(new KeyValuePair<string, string>(string.Empty, string.Empty), parameter);
        //}

        //[Fact(DisplayName = $"Static. {nameof(RequestService.QueryParameter)}. Value Null")]
        //public void Request_RequestParameter_ValueNull()
        //{
        //    KeyValuePair<string, string> parameter = default;
        //    Assert.Null(Record.Exception(() => parameter = RequestService.QueryParameter("Key", null)));
        //    Assert.Equivalent(new KeyValuePair<string, string>("Key", string.Empty), parameter);
        //}
        #endregion Request Parameter

        #region BasePath

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.TryGetBasePath)}. NULL String Value")]
        public void Request_Base_NullString()
        {
            string? path = string.Empty;
            Assert.Null(Record.Exception(() => QueryHelper.TryGetBasePath(absolutePath: default(string), out path)));
            Assert.Null(path);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.TryGetBasePath)}. NULL Uri Value")]
        public void Request_Base_NullUri()
        {
            string? path = string.Empty;
            Assert.Null(Record.Exception(() => QueryHelper.TryGetBasePath(absolutePath: default(Uri), out path)));
            Assert.Null(path);
        }

        [Fact(DisplayName = $"Static. {nameof(QueryHelper.GetBasePath)}. Empty String Value")]
        public void Request_Base_EmptyString()
        {
            Assert.Throws<InvalidCastException>(() => QueryHelper.GetBasePath(string.Empty));
        }

        [Theory(DisplayName = $"Static. {nameof(QueryHelper.TryGetBasePath)}. Invalid String Value")]
        [ClassData(typeof(InvalidRoutes))]
        public void Request_Base_InvalidString(string invalidRoute)
        {
            ThrowsWithMessage<InvalidCastException>(() => QueryHelper.GetBasePath(invalidRoute), $"Invalid route is allowed. Route : {invalidRoute}");
        }

        [Theory(DisplayName = $"Static. {nameof(QueryHelper.TryGetBasePath)}. Invalid Uri Value")]
        [ClassData(typeof(InvalidRoutes))]
        public void Request_Base_InvalidUri(string invalidRoute)
        {
            bool isValidated = false;
            try
            {
                QueryHelper.GetBasePath(new Uri(invalidRoute));
            }
            catch (InvalidCastException)
            {
                isValidated = true;
            }
            catch(UriFormatException)
            {
                isValidated = true;
            }

            Assert.True(isValidated, $"Invalid route is allowed. Route : {invalidRoute}");
        }


        #endregion BasePath

        #region Combine Query Parameters

        private const string EmptyValue = "";
        private const string IgnoreWord = "ignore_parameters";

        [Theory(DisplayName = $"Static. {nameof(QueryHelper.CombineQueryParameters)}. Ignore Empty")]
        [InlineData(true, IgnoreWord, EmptyValue)]
        [InlineData(true, EmptyValue, "value")]
        [InlineData(true, EmptyValue, EmptyValue)]
        [InlineData(true, "with_value", "value")]
        [InlineData(true, "with_empty_value", EmptyValue)]

        [InlineData(false, IgnoreWord, EmptyValue)]
        [InlineData(false, EmptyValue, "value1")]
        [InlineData(false, EmptyValue, EmptyValue)]
        [InlineData(false, "with_value", "value")]
        [InlineData(false, "with_empty_value", EmptyValue)]
        public void Request_CombineQueryParameters_IgnoreEmpty(bool ignoreEmptyParameters, string requestParameterKey, string requestParameterValue)
        {
            string? queryParameters = null;
            KeyValuePair<string, string>[] array =
            {
                QueryHelper.CreateQueryParameter(requestParameterKey, requestParameterValue),
                QueryHelper.CreateQueryParameter("name", "john")
            };
            Assert.Null(Record.Exception(() =>
            {
                return queryParameters = requestParameterKey.Equals(IgnoreWord)
                                         ? QueryHelper.CombineQueryParameters(ignoreEmptyParameters)
                                         : QueryHelper.CombineQueryParameters(ignoreEmptyParameters, array!);
            }));

            TestContole.WriteLine(string.IsNullOrEmpty(queryParameters) ? "Result : Empty String" : $"Result : {queryParameters}");

            switch (requestParameterKey)
            {
                case IgnoreWord: Assert.Equal(string.Empty, queryParameters); break;
                case "with_value": Assert.Equal("with_value=value&name=john", queryParameters); break;
                case "with_empty_value" when ignoreEmptyParameters is false: Assert.Equal("with_empty_value=&name=john", queryParameters); break;
                default: Assert.Equal("name=john", queryParameters ?? "NULL"); break;
            }
        }

        [Theory(DisplayName = $"Static. {nameof(QueryHelper.CombineQueryParameters)}. Same Duplicates")]
        [InlineData(true, EmptyValue, "value")]
        [InlineData(true, EmptyValue, EmptyValue)]
        [InlineData(true, "with_value", "value")]
        [InlineData(true, "with_empty_value", EmptyValue)]
        
        [InlineData(false, EmptyValue, "value")]
        [InlineData(false, EmptyValue, EmptyValue)]
        [InlineData(false, "with_value", "value")]
        [InlineData(false, "with_empty_value", EmptyValue)]
        public void Request_CombineQueryParameters_Duplicates(bool ignoreEmptyParameters, string requestParameterKey, string requestParameterValue)
        {
            string? queryParameters = default;
            KeyValuePair<string, string>[] array =
            {
                QueryHelper.CreateQueryParameter(requestParameterKey, requestParameterValue),
                QueryHelper.CreateQueryParameter(requestParameterKey, requestParameterValue)
            };
            Assert.Null(Record.Exception(() => queryParameters = QueryHelper.CombineQueryParameters(ignoreEmptyParameters, array!)));
            TestContole.WriteLine(string.IsNullOrEmpty(queryParameters) ? "Result : Empty String" : $"Result : {queryParameters}");

            switch (requestParameterKey)
            {
                case "with_value": Assert.Equal("with_value=value", queryParameters); break;
                case "with_empty_value" when ignoreEmptyParameters is false: Assert.Equal("with_empty_value=", queryParameters); break;
                default: Assert.Equal(string.Empty, queryParameters ?? "NULL"); break;
            }
        }

        [Theory(DisplayName = $"Static. {nameof(QueryHelper.CombineQueryParameters)}. Pair Duplicates")]
        [InlineData(true, EmptyValue, "value")]
        [InlineData(true, EmptyValue, EmptyValue)]
        [InlineData(true, "with_value", "value")]
        [InlineData(true, "with_empty_value", EmptyValue)]

        [InlineData(false, EmptyValue, "value")]
        [InlineData(false, EmptyValue, EmptyValue)]
        [InlineData(false, "with_value", "value")]
        [InlineData(false, "with_empty_value", EmptyValue)]
        public void Request_CombineQueryParameters_Pair_Duplicates(bool ignoreEmptyParameters, string requestParameterKey, string requestParameterValue)
        {
            string? queryParameters = default;
            KeyValuePair<string, string>[] array =
            {
                QueryHelper.CreateQueryParameter(requestParameterKey, requestParameterValue),
                QueryHelper.CreateQueryParameter(requestParameterKey, requestParameterValue),
                QueryHelper.CreateQueryParameter("name", "john"),
                QueryHelper.CreateQueryParameter("name", "john"),
            };
            Assert.Null(Record.Exception(() => queryParameters = QueryHelper.CombineQueryParameters(ignoreEmptyParameters, array!)));
            TestContole.WriteLine(string.IsNullOrEmpty(queryParameters) ? "Result : Empty String" : $"Result : {queryParameters}");

            switch (requestParameterKey)
            {
                case "with_value": Assert.Equal("with_value=value&name=john", queryParameters); break;
                case "with_empty_value" when ignoreEmptyParameters is false: Assert.Equal("with_empty_value=&name=john", queryParameters); break;
                default: Assert.Equal("name=john", queryParameters ?? "NULL"); break;
            }
        }

        [Theory(DisplayName = $"Static. {nameof(QueryHelper.CombineQueryParameters)}. Different Duplicates")]
        [InlineData(true, EmptyValue, "value")]
        [InlineData(true, EmptyValue, EmptyValue)]
        [InlineData(true, "with_value", "value")]
        [InlineData(true, "with_empty_value", EmptyValue)]

        [InlineData(false, EmptyValue, "value")]
        [InlineData(false, EmptyValue, EmptyValue)]
        [InlineData(false, "with_value", "value")]
        [InlineData(false, "with_empty_value", EmptyValue)]
        public void Request_CombineQueryParameters_Different_Duplicates(bool ignoreEmptyParameters, string requestParameterKey, string requestParameterValue)
        {
            string? queryParameters = default;
            KeyValuePair<string, string>[] array =
            {
                QueryHelper.CreateQueryParameter(requestParameterKey, requestParameterValue),
                QueryHelper.CreateQueryParameter(requestParameterKey, "duplicateKeyValue"),
                QueryHelper.CreateQueryParameter("name", "john"),
                QueryHelper.CreateQueryParameter("name", "maria"),
            };
            Assert.Null(Record.Exception(() => queryParameters = QueryHelper.CombineQueryParameters(ignoreEmptyParameters, array!)));
            TestContole.WriteLine(string.IsNullOrEmpty(queryParameters) ? "Result : Empty String" : $"Result : {queryParameters}");

            switch (requestParameterKey)
            {
                case "with_value": Assert.Equal("with_value=value,duplicateKeyValue&name=john,maria", queryParameters); break;
                case "with_empty_value": Assert.Equal("with_empty_value=duplicateKeyValue&name=john,maria", queryParameters); break;
                case null: Assert.Equal("name=john,maria", queryParameters ?? "NULL"); break;
                case EmptyValue: Assert.Equal("name=john,maria", queryParameters ?? "NULL"); break;
            }
        }

        #endregion Combine Query Parameters
    }
}
