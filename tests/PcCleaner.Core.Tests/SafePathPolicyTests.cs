using PcCleaner.Core.Security;
using Xunit;

namespace PcCleaner.Core.Tests;

public class SafePathPolicyTests
{
    [Fact]
    public void Allows_Path_Inside_Allowed_Root()
    {
        var allowedRoot = Path.Combine(Path.GetTempPath(), "pccleaner-allowed");
        var policy = SafePathPolicy.CreateDefault(new[] { allowedRoot });

        var filePath = Path.Combine(allowedRoot, "file.txt");

        Assert.True(policy.IsAllowed(filePath));
    }

    [Fact]
    public void Blocks_Path_Outside_Allowed_Root()
    {
        var allowedRoot = Path.Combine(Path.GetTempPath(), "pccleaner-allowed");
        var policy = SafePathPolicy.CreateDefault(new[] { allowedRoot });

        var otherPath = Path.Combine(Path.GetTempPath(), "pccleaner-other", "file.txt");

        Assert.False(policy.IsAllowed(otherPath));
    }
}
