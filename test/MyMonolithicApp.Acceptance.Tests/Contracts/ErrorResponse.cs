using System.Collections.Generic;

namespace MyMonolithicApp.Acceptance.Tests.Contracts
{
    public class ErrorResponse
    {
        public IEnumerable<Error> Errors { get; init; }

        public ErrorResponse(IEnumerable<Error> errors)
        {
            Errors = errors;
        }
    }
}
