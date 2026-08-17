using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class QueenInputUpdateScopeTests
    {
        [Fact]
        public void EnterMakesScopeActiveUntilLeaseIsDisposed()
        {
            var scope = new QueenInputUpdateScope();

            Assert.False(scope.IsActive);

            using (scope.Enter())
            {
                Assert.True(scope.IsActive);
            }

            Assert.False(scope.IsActive);
        }

        [Fact]
        public void NestedLeasesKeepScopeActiveUntilBothAreDisposed()
        {
            var scope = new QueenInputUpdateScope();
            var outerLease = scope.Enter();
            var innerLease = scope.Enter();

            innerLease.Dispose();
            Assert.True(scope.IsActive);

            outerLease.Dispose();
            Assert.False(scope.IsActive);
        }

        [Fact]
        public void DisposingTheSameLeaseTwiceIsSafe()
        {
            var scope = new QueenInputUpdateScope();
            var lease = scope.Enter();

            lease.Dispose();
            lease.Dispose();

            Assert.False(scope.IsActive);
        }
    }
}
