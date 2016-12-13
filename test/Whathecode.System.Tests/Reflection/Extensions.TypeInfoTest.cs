using System;
using System.Reflection;
using Whathecode.System.Collections.Generic;
using Whathecode.System.Reflection;
using Xunit;


namespace Whathecode.System.Tests.Reflection
{
    public partial class Extensions
    {
		public class TypeInfoTest
		{
			class Simple { }

			interface ICovariantOne<out T> { }
			interface IContravariantOne<in T> { }
			class CovariantOne<T> : ICovariantOne<T> { }
			class ContravariantOne<T> : IContravariantOne<T> { }

			/// <summary>
			/// List which groups the type from which to which to convert,
			/// together with the expected outcome for an implicit and explicit conversion. (success/failure)
			/// </summary>
			class CanConvert : TupleList<TypeInfo, TypeInfo, bool, bool>
			{
				public void Add( TypeInfo from, TypeInfo to, bool expectedResult )
				{
					// By default, assume explicit conversions within the same hierarchy are always possible.
					Add( from, to, expectedResult, true );
				}

				public void Test()
				{
					foreach ( var test in this )
					{
						// Implicit test.
						Assert.Equal( test.Item3, test.Item1.CanConvertTo( test.Item2 ) );

						// Test whether explicit casts to to type in same hierarchy is possible.
						Assert.Equal( test.Item4, test.Item1.CanConvertTo( test.Item2, CastType.SameHierarchy ) );

						// TODO: 'CanConvertTo' seems the inverse of 'IsAssignableFrom'. Furthermore, this implementation contains errors (indicated below).
						//       The only benefit over 'IsAssignableFrom' is most likely the CastType.SameHierarchy option checking for explicit conversions.
						//bool isAssignable = test.Item2.IsAssignableFrom( test.Item1 );
						//Assert.Equal( test.Item3, isAssignable );
					}
				}
			}


			// Types.
			readonly TypeInfo _int = typeof( int ).GetTypeInfo();
			readonly TypeInfo _short = typeof( short ).GetTypeInfo();
			readonly TypeInfo _string = typeof( string ).GetTypeInfo();
			readonly TypeInfo _object = typeof( object ).GetTypeInfo();
			readonly TypeInfo _simple = typeof( Simple ).GetTypeInfo();
			readonly TypeInfo _comparable = typeof( IComparable ).GetTypeInfo();


			[Fact]
			public void CanConvertToTest()
			{
				new CanConvert
				{
					// Non generic.
					{ _int, _int, true },		// No change.
					{ _string, _string, true },
					{ _object, _object, true },
					{ _simple, _simple, true },
					{ _int, _object, true },	// object <-> value
					{ _object, _int, false },
					{ _string, _object, true },	// string <-> object
					{ _object, _string, false },
					{ _simple, _object, true },	// object <-> object
					{ _object, _simple, false },
					{ _int, _short, false },	// value <-> value (by .NET rules, not C#)
					{ _short, _int, false }, // TODO: Fails as inverse of 'IsAssignableFrom'.

					// Interface.
					{ _comparable, _comparable, true },	// No change.
					{ _int, _comparable, true },		// value <-> interface
					{ _comparable, _int, false },
					{ _comparable, _object, true },		// object <-> interface
					{ _object, _comparable, false }
				}.Test();

				// Interface variant type parameters.
				Func<TypeInfo, TypeInfo, TypeInfo> makeGeneric = ( g, t ) => g.MakeGenericType( t.AsType() ).GetTypeInfo();
				VarianceCheck( typeof( ICovariantOne<> ), makeGeneric, true );
				VarianceCheck( typeof( IContravariantOne<> ), makeGeneric, false );

				// Delegate variant type parameter.
				VarianceCheck( typeof( Func<> ), makeGeneric, true );
				VarianceCheck( typeof( Action<> ), makeGeneric, false );

				// Multiple variant type parameters.
				TypeInfo simpleObject = typeof( Func<Simple, object> ).GetTypeInfo();
				TypeInfo objectSimple = typeof( Func<object, Simple> ).GetTypeInfo();
				TypeInfo simpleSimple = typeof( Func<Simple, Simple> ).GetTypeInfo();
				TypeInfo objectObject = typeof( Func<object, object> ).GetTypeInfo();
				Assert.True( simpleObject.CanConvertTo( simpleObject ) );
				Assert.False( simpleObject.CanConvertTo( objectSimple ) );
				Assert.True( objectSimple.CanConvertTo( simpleObject ) );
				Assert.False( simpleObject.CanConvertTo( simpleSimple ) );
				Assert.True( objectSimple.CanConvertTo( objectObject ) );

				// TODO: Multiple inheritance for interfaces.

				// Recursive variant type parameters.
				Func<TypeInfo, TypeInfo, TypeInfo> makeInnerGeneric = ( g, t )
					=> g.GetGenericTypeDefinition( g.GetGenericArguments()[ 0 ].GetGenericTypeDefinition().MakeGenericType( t.AsType() ) );
				VarianceCheck( typeof( Func<Func<object>> ), makeInnerGeneric, true );
				VarianceCheck( typeof( Action<Action<object>> ), makeInnerGeneric, false ); // TODO: Fails as inverse of 'IsAssignableFrom'.
				VarianceCheck( typeof( ICovariantOne<ICovariantOne<object>> ), makeInnerGeneric, true );
				VarianceCheck( typeof( IContravariantOne<IContravariantOne<object>> ), makeInnerGeneric, false ); // TODO: Fails as inverse of 'IsAssignableFrom'.

				// Mixed recursive covariant/contravariant type parameters.
				VarianceCheck( typeof( Func<Action<object>> ), makeInnerGeneric, false );
				VarianceCheck( typeof( Action<Func<object>> ), makeInnerGeneric, true ); // TODO: Fails as inverse of 'IsAssignableFrom'.
			}

			/// <summary>
			/// Checks the variance rules for generic types.
			/// For interfaces, only considers single implementing interface.
			/// </summary>
			/// <param name = "genericType">The generic type to check.</param>
			/// <param name = "makeGeneric">Function which can convert generic type into a specific type.</param>
			/// <param name = "covariant">true when to check for covariance, false for contravariance.</param>
			void VarianceCheck( Type genericType, Func<TypeInfo, TypeInfo, TypeInfo> makeGeneric, bool covariant )
			{
				TypeInfo info = genericType.GetTypeInfo();
				TypeInfo genericSimple = makeGeneric( info, _simple );
				TypeInfo genericObject = makeGeneric( info, _object );
				TypeInfo genericValue = makeGeneric( info, _int );

				bool isDelegate = info.IsDelegate();

				new CanConvert
				{
					// No change.
					{ genericObject, genericObject, true },		
		
					// generic type <-> object
					{ genericObject, _object, true },
					{ _object, genericObject, false },

					// No variance for value type parameters.
					// Converting from a generic type with a value parameter to one with a reference type parameters is only possible
					// when it is an interface type, and a certain type implements both interfaces. (e.g. ICovariance<int> -> ICovariance<object>)
					{ genericValue, genericObject, false, false },
					{ genericObject, genericValue, false, false },

					// Covariance/contraviariance between reference type parameters.
					// Only generic interface types can explicitly convert in the 'opposite' direction of their variance. Delegates can't!				
					{ genericSimple, genericObject,
						covariant,
						covariant ? true : !isDelegate },
					{ genericObject, genericSimple,
						!covariant,
						!covariant ? true : !isDelegate }
				}.Test();
			}

			/// <summary>
			/// TODO: This needs to be better defined, and subsequently thoroughly tested!
			///       Byte, e.g., does not seem to be a numeric type, although primitive and not bool (it does not support simple addition).
			/// </summary>
			/// <param name="expected"></param>
			/// <param name="type"></param>
			[Theory]
			[InlineData( false, typeof( bool ) )]
			[InlineData( true, typeof( byte ) )]
			[InlineData( true, typeof( char ) )]
			[InlineData( true, typeof( decimal ) )]
			[InlineData( true, typeof( double ) )]
			[InlineData( false, typeof( Enum ) )]
			[InlineData( true, typeof( float ) )]
			[InlineData( true, typeof( int ) )]
			[InlineData( true, typeof( long ) )]
			[InlineData( true, typeof( sbyte ) )]
			[InlineData( true, typeof( short ) )]
			[InlineData( true, typeof( uint ) )]
			[InlineData( true, typeof( ulong ) )]
			[InlineData( true, typeof( ushort ) )]
			public void IsNumericTypeTest( bool expected, Type type )
			{
				Assert.Equal( expected, type.GetTypeInfo().IsNumericType() );
			}
		}
    }
}
