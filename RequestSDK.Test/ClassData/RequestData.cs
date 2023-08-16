using System.Collections;
using System.Net.Mime;

using RequestSDK.Helpers;
using RequestSDK.Services;

namespace RequestSDK.Test.ClassData;

public sealed class RequestData : IEnumerable<object[]>
{
    public class TypedRequestData : IEquatable<TypedRequestData>
    {
        public enum WorkerType : byte { ProjectManager, Developer, TeamLead }
        public int Age { get; set; }
        public double Salary { get; set; }
        public float TaxPersent { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsNewWorker { get; set; }
        public WorkerType JobType { get; set; }

        public bool Equals(TypedRequestData? other)
        {
            return other != null
                   && Age == other.Age
                   && Salary == other.Salary
                   && TaxPersent == other.TaxPersent
                   && Name == other.Name
                   && Description == other.Description
                   && IsNewWorker == other.IsNewWorker
                   && JobType == other.JobType;
        }
    }

    private readonly string StringHttpContent = "Mocked HTTP response content";
    private readonly Func<object> GenerateAnonimousObjectHttpContent = () => new { RequestContentName = "Test Name", RequestMessage = "Test Message", RequestId = 1 };
    private readonly TypedRequestData TypedObjectRequestContent = new TypedRequestData()
    {
        Age = 40,
        TaxPersent = 15.5f,
        Salary = 599.555d,
        Description = "Worker",
        IsNewWorker = true,
        JobType = TypedRequestData.WorkerType.TeamLead,
        Name = "John"
    };

    private readonly string ClientBasePath = "https://example.com/rest";
    private readonly string ClientEndPoint = "controller/action";
    private readonly string[] AcceptTypes = new string[] { MediaTypeNames.Application.Json, MediaTypeNames.Text.Html };

    private readonly KeyValuePair<string, string>[] RequestHeaders = new[]
    {
        new KeyValuePair<string, string>("X-KEY", "X-VALUE-1, X-VALUE-2")
    };

    private readonly KeyValuePair<string, string>[] RequestParameters = new[]
    {
        new KeyValuePair<string, string>("first-name", "john"),
        new KeyValuePair<string, string>("last-name", "doe"),
        new KeyValuePair<string, string>("age", "21")
    };

    public IEnumerator<object[]> GetEnumerator()
    {
        //Check Response [string], Request [string]
        yield return new object[]
        {
            StringHttpContent,
            ClientBasePath,
            ClientEndPoint,
            StringHttpContent,
            AcceptTypes,
            RequestHeaders,
            RequestParameters
        };

        //Check Response [string], Request [anonimous type]
        yield return new object[]
        {
            StringHttpContent,
            ClientBasePath,
            ClientEndPoint,
            GenerateAnonimousObjectHttpContent(),
            AcceptTypes,
            RequestHeaders,
            RequestParameters
        };

        //Check Response [string], Request [typed complex object]
        yield return new object[]
        {
            StringHttpContent,
            ClientBasePath,
            ClientEndPoint,
            TypedObjectRequestContent,
            AcceptTypes,
            RequestHeaders,
            RequestParameters
        };

        //Check Response [anonimous type], Request [string]
        yield return new object[]
        {
            GenerateAnonimousObjectHttpContent(),
            ClientBasePath,
            ClientEndPoint,
            StringHttpContent,
            AcceptTypes,
            RequestHeaders,
            RequestParameters
        };

        //Check Response [typed complex object], Request [string]
        yield return new object[]
        {
            TypedObjectRequestContent,
            ClientBasePath,
            ClientEndPoint,
            StringHttpContent,
            AcceptTypes,
            RequestHeaders,
            RequestParameters
        };

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
