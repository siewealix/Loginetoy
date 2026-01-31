using PcCleaner.Core.Security;
using Xunit;

namespace PcCleaner.Core.Tests;

public class ExclusionPolicyTests
{
    [Fact]
    public void Matches_Wildcard_Patterns()
    {
        var policy = new ExclusionPolicy(new[] { "*.log", "C:\\Secret" });

        Assert.True(policy.IsExcluded("C:\\Temp\\error.log"));
        Assert.True(policy.IsExcluded("C:\\Secret\\data.txt"));
        Assert.False(policy.IsExcluded("C:\\Temp\\data.txt"));
    }
}
