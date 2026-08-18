using System;
using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class CameraPanMathTests
    {
        [Fact]
        public void DiagonalMovementIsNormalizedToConfiguredSpeed()
        {
            var delta = CameraPanMath.CalculateDelta(1f, 1f, 12f, 0.5f);

            Assert.Equal(6f, Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y), 3);
        }

        [Fact]
        public void NoDirectionProducesNoMovement()
        {
            var delta = CameraPanMath.CalculateDelta(0f, 0f, 12f, 1f);

            Assert.Equal(0f, delta.X);
            Assert.Equal(0f, delta.Y);
        }
    }
}
