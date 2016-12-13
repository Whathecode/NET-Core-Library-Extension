using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Whathecode.System.Linq;


namespace Whathecode.System.Reflection
{
    public static partial class Extensions
    {
		/// <summary>
		/// Determines whether a conversion from one type to another is possible.
		/// This uses .NET rules. E.g., short is not implicitly convertible to int, while this is possible in C#.
		/// TODO: Support constraints, custom implicit conversion operators? Unit tests for explicit converts.
		/// TODO: The following seems the inverse of 'IsAssignableFrom'
		/// </summary>
		/// <param name = "fromType">The type to convert from.</param>
		/// <param name = "targetType">The type to convert to.</param>
		/// <param name = "castType">Specifies what types of casts should be considered.</param>
		/// <returns>true when a conversion to the target type is possible, false otherwise.</returns>
		public static bool CanConvertTo( this TypeInfo fromType, TypeInfo targetType, CastType castType = CastType.Implicit )
		{
			return CanConvertTo( fromType, targetType, castType, false );
		}

		static bool CanConvertTo( this TypeInfo fromType, TypeInfo targetType, CastType castType, bool switchVariance )
		{
			bool sameHierarchy = castType == CastType.SameHierarchy;

			Func<TypeInfo, TypeInfo, bool> covarianceCheck = sameHierarchy
				? (Func<TypeInfo, TypeInfo, bool>)IsInHierarchy
				: ( from, to ) => from == to || from.IsSubclassOf( to.AsType() );
			Func<TypeInfo, TypeInfo, bool> contravarianceCheck = sameHierarchy
				? (Func<TypeInfo, TypeInfo, bool>)IsInHierarchy
				: ( from, to ) => from == to || to.IsSubclassOf( from.AsType() );

			if ( switchVariance )
			{
				Variable.Swap( ref covarianceCheck, ref contravarianceCheck );
			}

			// Simple hierarchy check.
			if ( covarianceCheck( fromType, targetType ) )
			{
				return true;
			}

			// Interface check.
			if ( (targetType.IsInterface && fromType.ImplementsInterface( targetType ))
				|| (sameHierarchy && fromType.IsInterface && targetType.ImplementsInterface( fromType )) )
			{
				return true;
			}

			// Explicit value type conversions (including enums).
			if ( sameHierarchy && (fromType.IsValueType && targetType.IsValueType) )
			{
				return true;
			}

			// Recursively verify when it is a generic type.
			if ( targetType.IsGenericType )
			{
				TypeInfo genericDefinition = targetType.GetGenericTypeDefinition().GetTypeInfo();
				TypeInfo sourceGeneric = fromType.GetMatchingGenericType( genericDefinition );

				// Delegates never support casting in the 'opposite' direction than their varience type parameters dictate.
				CastType cast = fromType.IsDelegate() ? CastType.Implicit : castType;

				if ( sourceGeneric != null ) // Same generic types.
				{
					// Check whether parameters correspond, taking into account variance rules.
					return sourceGeneric.GetGenericArguments().Select( s => s.GetTypeInfo() ).Zip(
						targetType.GetGenericArguments().Select( t => t.GetTypeInfo() ), genericDefinition.GetGenericArguments().Select( g => g.GetTypeInfo() ),
						( from, to, generic )
							=> !(from.IsValueType || to.IsValueType)	// Variance applies only to reference types.
								? generic.GenericParameterAttributes.HasFlag( GenericParameterAttributes.Covariant )
									? CanConvertTo( from, to, cast, false )
									: generic.GenericParameterAttributes.HasFlag( GenericParameterAttributes.Contravariant )
										? CanConvertTo( from, to, cast, true )
										: false
								: false )
						.All( match => match );
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether one type is in the same inheritance hierarchy than another.
		/// TODO: This apparently does not work for interfaces which inherent from each other: https://msdn.microsoft.com/en-us/library/system.type.issubclassof(v=vs.110).aspx
		/// </summary>
		/// <param name = "source">The source for this extension method.</param>
		/// <param name = "type">The type the check whether it is in the same inheritance hierarchy.</param>
		/// <returns>true when both types are in the same inheritance hierarchy, false otherwise.</returns>
		public static bool IsInHierarchy( this TypeInfo source, TypeInfo type )
		{
			return source == type || source.IsSubclassOf( type.AsType() ) || type.IsSubclassOf( source.AsType() );
		}

		/// <summary>
		/// Verify whether the type is a numeric type.
		/// TODO: What is a 'numeric' type? This is not fully defined. Thorough unit tests need to be created for this.
		/// </summary>
		/// <param name = "source">The source of this extension method.</param>
		/// <returns>True when the given type is a numeric type, false otherwise.</returns>
		public static bool IsNumericType( this TypeInfo source )
		{
			// All primitive types except bool are numeric.
			if ( source.IsPrimitive )
			{				
				return source.AsType() != typeof( bool );
			}

			// Check whether all numeric operators are available.
			return Operator.NumericalOperators.All( source.HasOperator );
		}

		/// <summary>
		/// Verify whether the type supports a certain operator.
		/// </summary>
		/// <param name = "source">The source of this extension method.</param>
		/// <param name = "operator">The operator to check for.</param>
		/// <returns>True when the type supports the operator, false otherwise.</returns>
		public static bool HasOperator( this TypeInfo source, Operator @operator )
		{
			var defaultValue = Expression.Default( source.AsType() );

			var binaryOperator = @operator as BinaryOperator;
			if ( binaryOperator != null )
			{
				try
				{
					binaryOperator.GetExpression()( defaultValue, defaultValue ); // Throws an exception if operator is not defined.
					return true;
				}
				catch
				{
					return false;
				}								
			}

			var unaryOperator = @operator as UnaryOperator;
			if ( unaryOperator != null )
			{
				try
				{
					unaryOperator.GetExpression()( defaultValue ); // Throws an exception if operator is not defined.
					return true;
				}
				catch
				{
					return false;
				}					
			}

			throw new NotSupportedException( String.Format( "Operator \"{0}\" isn't supported.", @operator ) );
		}

		/// <summary>
		/// Return all members of a specific type in a certain type.
		/// </summary>
		/// <param name = "source">The source of this extension method.</param>
		/// <param name = "type">The type to search for.</param>
		/// <returns>A list of all object members with the specific type.</returns>
		public static IEnumerable<MemberInfo> GetMembers( this TypeInfo source, TypeInfo type )
		{
			return
				from m in source.GetMembers( ReflectionHelper.FlattenedClassMembers )
				where m is FieldInfo || m is PropertyInfo || m is EventInfo
				where m.GetMemberType().GetTypeInfo().IsOfGenericType( type )
				select m;
		}

		/// <summary>
		/// Returns all members which have a specified attribute annotated to them.
		/// </summary>
		/// <param name = "source">The source of this extension method.</param>
		/// <param name = "memberTypes">The type of members to search in.</param>
		/// <param name = "inherit">Specifies whether to search this member's inheritance chain to find the attributes.</param>
		/// <param name = "bindingFlags">
		/// A bitmask comprised of one or more <see cref="BindingFlags" /> that specify how the search is conducted.
		/// -or-
		/// Zero, to return null.
		/// </param>
		/// <typeparam name = "TAttribute">The type of the attributes to search for.</typeparam>
		/// <returns>A dictionary containing all members with their attached attributes.</returns>
		public static Dictionary<MemberInfo, TAttribute[]> GetAttributedMembers<TAttribute>(
			this TypeInfo source,
			MemberTypes memberTypes = MemberTypes.All,
			bool inherit = false,
			BindingFlags bindingFlags = ReflectionHelper.FlattenedClassMembers )
			where TAttribute : Attribute
		{
			return (
				from member in source.GetMembers( bindingFlags )
				where member.MemberType.HasFlag( memberTypes )
				let attributes = member.GetCustomAttributes<TAttribute>( inherit )
				where attributes.Any()
				select new { Member = member, Attributes = attributes.ToArray() }
				).ToDictionary( g => g.Member, g => g.Attributes );
		}

		/// <summary>
		/// Searches for all methods defined in an interface and its inherited interfaces.
		/// </summary>
		/// <param name = "source">The source of this extension method.</param>
		/// <param name = "bindingFlags">
		/// A bitmask comprised of one or more <see cref="BindingFlags" /> that specify how the search is conducted.
		/// -or-
		/// Zero, to return null.
		/// </param>
		/// <returns>A list of all found methods.</returns>
		public static IEnumerable<MethodInfo> GetFlattenedInterfaceMethods( this TypeInfo source, BindingFlags bindingFlags )
		{
			foreach ( var info in source.GetMethods( bindingFlags ) )
			{
				yield return info;
			}

			var flattened = source.GetInterfaces().SelectMany( interfaceType => GetFlattenedInterfaceMethods( interfaceType.GetTypeInfo(), bindingFlags ) );
			foreach ( var info in flattened )
			{
				yield return info;
			}
		}
    }
}
