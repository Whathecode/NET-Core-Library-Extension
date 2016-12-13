using Whathecode.System.Algorithm.Interpolation;
using Xunit;


namespace Whathecode.Tests.System.Algorithm.Interpolation
{
    public class LinearInterpolationTest
    {
		[Fact]
		public void InterpolateTest()
		{
			var keyPoints = new AbsoluteKeyPointCollection<int, int>( new ValueTypeInterpolationProvider<int>(), 0 );
			keyPoints.Add( 0 );
			keyPoints.Add( 10 );
			var interpolate = new LinearInterpolation<int, int>( keyPoints );

			Assert.Equal( 5, interpolate.Interpolate( 0.5 ) );
		}
    }
}
