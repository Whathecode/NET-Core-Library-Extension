using System;
using Whathecode.System.Operators;


namespace Whathecode.System
{
	/// <summary>
	/// A helper class to do common <see cref="Math" /> operations.
	/// </summary>
	/// <typeparam name = "T">The type used for calculations.</typeparam>
	public class MathHelper<T>
    {
		/// <summary>
		/// Round a value to the nearest multiple of another value.
		/// </summary>
		/// <param name = "value">The value to round to the nearest multiple.</param>
		/// <param name = "roundToMultiple">The passed value will be rounded to the nearest multiple of this value.</param>
		/// <returns>A multiple of roundToMultiple, nearest to the passed value.</returns>
		public static T RoundToNearest( T value, T roundToMultiple )
		{
			double factor = CastOperator<T, double>.Cast( roundToMultiple );
			double result = Math.Round( CastOperator<T, double>.Cast( value ) / factor ) * factor;
			return CastOperator<double, T>.Cast( result );
		}
    }
}
