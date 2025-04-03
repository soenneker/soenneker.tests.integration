using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Tests.Integration.Tests;

[Collection("Collection")]
public class IntegrationTestTests : FixturedUnitTest
{
    public IntegrationTestTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public void Default()
    {

    }
}
