namespace MyMonolithicApp.Acceptance.Tests.Contracts
{
    public class Error
    {
        public Error(string severity, string code, string detail)
        {
            Severity = severity;
            Code = code;
            Detail = detail;
        }

        public string Severity { get; init; }
        public string Code { get; init; }
        public string Detail { get; init; }
    }
}
