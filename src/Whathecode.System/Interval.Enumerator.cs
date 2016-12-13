using System;
using Whathecode.System.Collections.Generic;
using Whathecode.System.Operators;


namespace Whathecode.System
{
	public partial class Interval<T, TSize>
	{
		/// <summary>
		/// Enumerator which allows you to walk across values inside an interval.
		/// </summary>
		public class Enumerator : AbstractEnumerator<T>
		{
			readonly Interval<T, TSize> _interval;
			readonly TSize _step;

			readonly bool _isAnchorSet;
			readonly T _anchor;


			/// <summary>
			/// Create a new enumerator which traverses across an interval in specified steps.
			/// </summary>
			/// <param name = "interval">The interval which to traverse.</param>
			/// <param name = "step">The steps to step forward each time.</param>
			public Enumerator( Interval<T, TSize> interval, TSize step )
			{
				_interval = interval;
				_step = step;
			}

			public Enumerator( Interval<T, TSize> interval, TSize step, T anchorAt )
				: this( interval, step )
			{
				_isAnchorSet = true;
				_anchor = anchorAt;
			}


			protected override T GetFirst()
			{
				Interval<T, TSize> interval = _interval;

				// When anchor is set, start the interval at the next anchor position.
				if ( _isAnchorSet )
				{
					TSize anchorDiff = Operator<T, TSize>.Subtract( _interval.Start, _anchor );
					double stepSize = ConvertSizeToDouble( _step );
					double diff = Math.Abs( ConvertSizeToDouble( anchorDiff ) ) % stepSize;
					if ( diff > 0 )
					{
						if ( _anchor.CompareTo( _interval.Start ) < 0 )
						{
							diff = stepSize - diff;
						}
						TSize addition = ConvertDoubleToSize( diff );
						interval = new Interval<T, TSize>(
							Operator<T, TSize>.AddSize( _interval.Start, addition ), true,
							_interval.End, _interval.IsEndIncluded );
					}
				}

				// When first value doesn't lie in interval, immediately step.
				return interval.IsStartIncluded ? interval.Start : Operator<T, TSize>.AddSize( interval.Start, _step );
			}

			protected override T GetNext( int enumeratedAlready, T previous )
			{
				return Operator<T, TSize>.AddSize( previous, _step );
			}

			protected override bool HasElements()
			{
				bool nextInInterval = _interval.LiesInInterval( Operator<T, TSize>.AddSize( _interval.Start, _step ) );
				return _interval.IsStartIncluded || nextInInterval;
			}

			protected override bool HasMoreElements( int enumeratedAlready, T previous )
			{
				if ( Interval<T, TSize>.ConvertSizeToDouble( _step ) == 0 && enumeratedAlready == 1 )
				{
					return false;
				}

				return _interval.LiesInInterval( Operator<T, TSize>.AddSize( previous, _step ) );
			}

			public override void Dispose()
			{
				// TODO: Nothing to do?
			}
		}
	}
}