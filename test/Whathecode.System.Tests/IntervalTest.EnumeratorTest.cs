﻿using System;
using System.Linq;
using Whathecode.System;
using Xunit;


namespace Whathecode.Tests.System
{
	public partial class IntervalTest
	{
		/// <summary>
		/// Unit tests for <see cref="Interval.Enumerator{TMath, TSize}" />.
		/// TODO: Now that xUnit is used, perhaps a cleaner way of testing generics can be set up by implementing a custom Theory data provider.
		///       Marc Seemann seems to have toyed around with this: http://blog.ploeh.dk/2011/05/09/GenericunittestingwithxUnit.net/
		/// </summary>
		public class EnumeratorTest
		{
			public EnumeratorTest()
			{
				Interval<DateTime, TimeSpan>.ConvertDoubleToSize = d => new TimeSpan( (long)Math.Round( d ) );
				Interval<DateTime, TimeSpan>.ConvertSizeToDouble = s => s.Ticks;
			}


			[Fact]
			public void StepTest()
			{
				// Different types.
				StepTestHelper( 0, true, 5, true, 1, new [] { 0, 1, 2, 3, 4, 5 } );
				StepTestHelper(
					new DateTime( 2015, 2, 1 ), true, new DateTime( 2015, 2, 1, 1, 0, 0 ), true,
					TimeSpan.FromMinutes( 20 ),
					new []
					{
						new DateTime( 2015, 2, 1, 0, 0, 0 ),
						new DateTime( 2015, 2, 1, 0, 20, 0 ),
						new DateTime( 2015, 2, 1, 0, 40, 0 ),
						new DateTime( 2015, 2, 1, 1, 0, 0 )
					} );

				// Excluded intervals.
				StepTestHelper( 0, true, 5, false, 1, new [] { 0, 1, 2, 3, 4 } );
				StepTestHelper( 0, false, 5, true, 1, new [] { 1, 2, 3, 4, 5 } );

				// Single and empty interval.
				StepTestHelper( 0, true, 0, true, 1, new [] { 0 } );
				StepTestHelper( 0, false, 0, false, 1, new int[] {} );

				// Step size of 0.
				StepTestHelper( 0, true, 5, true, 0, new [] { 0 } );
			}

			static void StepTestHelper<T, TSize>( T start, bool startIncluded, T end, bool endIncluded, TSize stepSize, T[] expected )
				where T : IComparable<T>
				where TSize : IComparable<TSize>
			{
				var enumerator = new Interval<T, TSize>.Enumerator(
					new Interval<T, TSize>( start, startIncluded, end, endIncluded ),
					stepSize );
				Assert.True( expected.SequenceEqual( enumerator ) );
			}

			[Fact]
			public void StepAnchorTest()
			{
				// Positive values.
				StepAnchorTestHelper( 0, 5, 2, 1, new[] { 1, 3, 5 } );
				StepAnchorTestHelper( 0, 5, 2, -11, new[] { 1, 3, 5 } );
				StepAnchorTestHelper( 0, 10, 5, 12, new[] { 2, 7 } );
				StepAnchorTestHelper( 0, 10, 5, -12, new[] { 3, 8 } );
				StepAnchorTestHelper(
					new DateTime( 2015, 1, 1, 13, 0, 0 ), new DateTime( 2015, 1, 2, 0, 0, 0 ),
					TimeSpan.FromHours( 10 ), new DateTime( 2015, 1, 1 ), new [] { new DateTime( 2015, 1, 1, 20, 0, 0 ) } );

				// Negative values.
				StepAnchorTestHelper( -10, 10, 4, 1, new [] { -7, -3, 1, 5, 9 } );
			}

			static void StepAnchorTestHelper<T, TSize>( T start, T end, TSize stepSize, T anchor, T[] expected )
				where T : IComparable<T>
				where TSize : IComparable<TSize>
			{
				var enumerator = new Interval<T, TSize>.Enumerator( new Interval<T, TSize>( start, end ), stepSize, anchor );
				Assert.True( expected.SequenceEqual( enumerator ) );
			}
		}
	}
}
