using System;
using Whathecode.System.Operators;


namespace Whathecode.System
{
	/// <summary>
	/// Class specifying an interval from a value, to a value. Borders may be included or excluded. This type is immutable.
	/// TODO: The 'ToString' and 'Parse' methods were removed since they relied on TypeConverter, which is not present in the .NET Core Library. How to support this?
	/// </summary>
	/// <remarks>
	/// This is a wrapper class which simply redirect calls to a more generic base type.
	/// </remarks>
	/// <typeparam name = "T">The type used to specify the interval, and used for the calculations.</typeparam>
	public class Interval<T> : AbstractInterval<T>
		where T : IComparable<T>
	{
		readonly static bool _isIntegralType;

		// TODO: Is there any benefit moving these converter functions to a factory which injects them through constructor injection instead?
		/// <summary>
		/// A function which can convert <typeparamref name="T" /> to a double representation.
		/// The function is used at runtime by any instance of this type to perform double calculations.
		/// </summary>
		public static Func<T, double> ConvertValueToDouble { get; set; }
		/// <summary>
		/// A function which can convert a double to <typeparamref name="T" />.
		/// This function is used at runtime by any instance of this type to perform double calculations.
		/// </summary>
		public static Func<double, T> ConvertDoubleToValue { get; set; }


		static Interval()
		{
			_isIntegralType = TypeHelper.IsIntegralNumericType<T>();

			// Verify whether default convertion operators are available to and from double.
			try
			{
				ConvertValueToDouble = CastOperator<T, double>.Cast;
				ConvertDoubleToValue = CastOperator<double, T>.Cast;
			}
			catch (TypeInitializationException)
			{
				ConvertValueToDouble = null;
				ConvertDoubleToValue = null;
			}
		}

		/// <summary>
		/// Create a new interval with a specified start and end, both included in the interval.
		/// </summary>
		/// <param name = "start">The start of the interval, included in the interval.</param>
		/// <param name = "end">The end of the interval, included in the interval.</param>
		public Interval( T start, T end )
			: this( start, true, end, true ) {}

		/// <summary>
		/// Create a new interval with a specified start and end.
		/// </summary>
		/// <param name = "start">The start of the interval.</param>
		/// <param name = "isStartIncluded">Is the value at the start of the interval included in the interval.</param>
		/// <param name = "end">The end of the interval.</param>
		/// <param name = "isEndIncluded">Is the value at the end of the interval included in the interval.</param>
		public Interval( T start, bool isStartIncluded, T end, bool isEndIncluded )
			: base( start, isStartIncluded, end, isEndIncluded ) {}

		/// <summary>
		/// Create a less generic interval from a more generic base type.
		/// </summary>
		/// <param name = "interval">The more generic base type.</param>
		public Interval( IInterval<T, T> interval )
			: base( interval.Start, interval.IsStartIncluded, interval.End, interval.IsEndIncluded ) {}


		protected override bool IsIntegralType
		{
			get { return _isIntegralType; }
		}

		protected override IInterval<T, T> CreateInstance( T start, bool isStartIncluded, T end, bool isEndIncluded )
		{
			return new Interval<T>( start, isStartIncluded, end, isEndIncluded );
		}

		protected override IInterval<T> ReduceGenerics( IInterval<T, T> interval )
		{
			return new Interval<T>( interval.Start, interval.IsStartIncluded, interval.End, interval.IsEndIncluded );
		}

		protected override double Convert( T value )
		{
			CheckForInvalidImplementation();
			return ConvertValueToDouble( value );
		}

		protected override T Convert( double value )
		{
			CheckForInvalidImplementation();
			return ConvertDoubleToValue( value );
		}

		void CheckForInvalidImplementation()
		{
			if (ConvertValueToDouble == null || ConvertDoubleToValue == null)
			{
				Type type = typeof( Interval<T> );
				const string message =
					"In order to use {0} you need to set 'ConvertSizeToDouble' and 'ConvertDoubleToSize'. " +
					"These converters could not be generated automatically for the specified type parameters.";
				throw new InvalidImplementationException( String.Format( message, type ) );
			}
		}

		protected override T Subtract( T value1, T value2 )
		{
			return Operator<T>.Subtract( value1, value2 );
		}

		protected override T AddSize( T value, T size )
		{
			return Operator<T>.Add( value, size );
		}
	}


	/// <summary>
	/// Class specifying an interval from a value, to a value. Borders may be included or excluded. This type is immutable.
	/// </summary>
	/// <typeparam name = "T">The type used to specify the interval, and used for the calculations.</typeparam>
	/// <typeparam name = "TSize">The type used to specify distances in between two values of <typeparamref name="T" />.</typeparam>
	public partial class Interval<T, TSize> : AbstractInterval<T, TSize>
		where T : IComparable<T>
		where TSize : IComparable<TSize>
	{
		readonly static bool _isIntegralType;

		// TODO: Is there any benefit moving these converter functions to a factory which injects them through constructor injection instead?
		/// <summary>
		/// A function which can convert <typeparamref name="TSize" /> to a double representation.
		/// The function is used at runtime by any instance of this type to perform double calculations.
		/// </summary>
		public static Func<TSize, double> ConvertSizeToDouble { get; set; }
		/// <summary>
		/// A function which can convert a double to <typeparamref name="TSize" />.
		/// This function is used at runtime by any instance of this type to perform double calculations.
		/// </summary>
		public static Func<double, TSize> ConvertDoubleToSize { get; set; }


		static Interval()
		{
			_isIntegralType = TypeHelper.IsIntegralNumericType<TSize>();

			// Verify whether default convertion operators are available to and from double.
			try
			{
				ConvertSizeToDouble = CastOperator<TSize, double>.Cast;
				ConvertDoubleToSize = CastOperator<double, TSize>.Cast;
			}
			catch ( TypeInitializationException )
			{
				ConvertSizeToDouble = null;
				ConvertDoubleToSize = null;
			}
		}

		/// <summary>
		/// Create a new interval with a specified start and end, both included in the interval.
		/// </summary>
		/// <param name = "start">The start of the interval, included in the interval.</param>
		/// <param name = "end">The end of the interval, included in the interval.</param>
		public Interval( T start, T end )
			: this( start, true, end, true ) { }

		/// <summary>
		/// Create a new interval with a specified start and end.
		/// </summary>
		/// <param name = "start">The start of the interval.</param>
		/// <param name = "isStartIncluded">Is the value at the start of the interval included in the interval.</param>
		/// <param name = "end">The end of the interval.</param>
		/// <param name = "isEndIncluded">Is the value at the end of the interval included in the interval.</param>
		public Interval( T start, bool isStartIncluded, T end, bool isEndIncluded )
			: base( start, isStartIncluded, end, isEndIncluded ) { }


		protected override bool IsIntegralType
		{
			get { return _isIntegralType; }
		}

		protected override IInterval<T, TSize> CreateInstance( T start, bool isStartIncluded, T end, bool isEndIncluded )
		{
			return new Interval<T, TSize>( start, isStartIncluded, end, isEndIncluded );
		}

		protected override double Convert( TSize size )
		{
			CheckForInvalidImplementation();
			return ConvertSizeToDouble( size );
		}

		protected override TSize Convert( double size )
		{
			CheckForInvalidImplementation();
			return ConvertDoubleToSize( size );
		}

		void CheckForInvalidImplementation()
		{
			if ( ConvertSizeToDouble == null || ConvertDoubleToSize == null )
			{
				Type type = typeof( Interval<T, TSize> );
				const string message =
					"In order to use {0} you need to set 'ConvertSizeToDouble' and 'ConvertDoubleToSize'. " +
					"These converters could not be generated automatically for the specified type parameters.";
				throw new InvalidImplementationException( String.Format( message, type ) );
			}
		}

		protected override TSize Subtract( T value1, T value2 )
		{
			return Operator<T, TSize>.Subtract( value1, value2 );
		}

		protected override T SubtractSize( T value, TSize size )
		{
			return Operator<T, TSize>.SubtractSize( value, size );
		}

		protected override TSize SubtractSizes( TSize value1, TSize value2 )
		{
			return Operator<TSize>.Subtract( value1, value2 );
		}

		protected override T AddSize( T value, TSize size )
		{
			return Operator<T, TSize>.AddSize( value, size );
		}
	}
}