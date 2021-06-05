using Xunit;

namespace MyMonolithicApp.Acceptance.Tests.Server
{
    [CollectionDefinition(nameof(TestApplicationServer))]
    public class TestApplicationServerCollection : ICollectionFixture<TestApplicationServer>
    {
    }
}
