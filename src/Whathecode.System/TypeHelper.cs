﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Whathecode.System.Operators;
using Whathecode.System.Reflection;


namespace Whathecode.System
{
	/// <summary>
	/// A helper class to do common <see cref="Type" /> operations.
	/// </summary>
	public static class TypeHelper
	{
		public static IEnumerable<Type> BuiltInIntegralTypes
		{
			get
			{
				yield return typeof( sbyte );
				yield return typeof( byte );
				yield return typeof( char );
				yield return typeof( short );
				yield return typeof( ushort );
				yield return typeof( int );
				yield return typeof( uint );
				yield return typeof( long );
				yield return typeof( ulong );
			}
		}

		public static IEnumerable<Type> BuiltInFloatingPointTypes
		{
			get 
			{
				yield return typeof( float );
				yield return typeof( double );
			}
		}

		/// <summary>
		/// Verify whether the given template type is an integral numeric type.
		/// TODO: Verify whether all other requirements of an integral type are met?
		/// TODO: Should this be moved to TypeInfo instead now that it relies on it?
		/// </summary>
		/// <returns>True when the template type is an integral numeric type, false otherwise.</returns>
		public static bool IsIntegralNumericType<T>()
		{
			Type type = typeof( T );
			TypeInfo info = type.GetTypeInfo();

			// Is it any of the primitive integral types?
			if ( info.IsPrimitive && BuiltInIntegralTypes.Any( t => t == type ) )
			{
				return true;
			}

			// Check whether all math operators are available and verify with integral division.
			if ( info.IsNumericType() )
			{
				// '3 / 2 == 1' when it is an integral type.
				T three = CastOperator<int, T>.Cast( 3 );
				T two = CastOperator<int, T>.Cast( 2 );
				T one = CastOperator<int, T>.Cast( 1 );
				if ( CastOperator<T, int>.Cast( three ) != 3 ||
					 CastOperator<T, int>.Cast( two ) != 2 ||
					 CastOperator<T, int>.Cast( one ) != 1 )
				{
					throw new NotSupportedException(
						String.Format( "Unable to verify for type \"{0}\" whether it is an integral numeric type.", type ) );
				}
				return Operator<T>.Divide( three, two ).Equals( one );
			}

			return false;			
		}
	}
}
