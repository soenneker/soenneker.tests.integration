using Soenneker.TestHosts.Unit;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Tests.Integration.Tests;

[ClassDataSource<UnitTestHost>(Shared = SharedType.PerTestSession)]
public sealed class IntegrationTestTests(UnitTestHost host) : HostedUnitTest(host)
{
    [Test]
    public void Default()
    {
    }
}