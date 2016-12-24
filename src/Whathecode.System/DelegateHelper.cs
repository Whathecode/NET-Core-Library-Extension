﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Whathecode.System.Reflection;


namespace Whathecode.System
{
	/// <summary>
	/// A helper class to do common <see cref="Delegate" /> operations.
	/// TODO: Add extra contracts to reenforce correct usage.
	/// TODO: Since Microsoft moved .CreateDelegate to MethodInfo, the helper methods here should be moved there as well.
	/// </summary>
	public static class DelegateHelper
	{
		/// <summary>
		/// Options which specify what type of delegate should be created.
		/// </summary>
		public enum CreateOptions
		{
			None,
			/// <summary>
			/// Downcasts of delegate parameter types to the correct types required for the method are done where necessary.
			/// Of course only valid casts will work.
			/// </summary>
			Downcasting
		}


		/// <summary>
		/// Holds the expressions for the parameters when creating delegates.
		/// </summary>
		struct ParameterConversionExpressions
		{
			public IEnumerable<ParameterExpression> OriginalParameters;
			public IEnumerable<Expression> ConvertedParameters;
		}


		/// <summary>
		/// The name of the Invoke method of a Delegate.
		/// </summary>
		const string InvokeMethod = "Invoke";


		/// <summary>
		/// Get method info for a specified delegate type.
		/// </summary>
		/// <param name = "delegateType">The delegate type to get info for.</param>
		/// <returns>The method info for the given delegate type.</returns>
		public static MethodInfo MethodInfoFromDelegateType( Type delegateType )
		{
			TypeInfo info = delegateType.GetTypeInfo();
			if ( !info.IsSubclassOf( typeof( MulticastDelegate ) ) )
			{
				throw new ArgumentException( "Given type should be a delegate.", nameof( delegateType ) );
			}

			return info.GetMethod( InvokeMethod );
		}

		/// <summary>
		/// Creates a delegate of a specified type which wraps another similar delegate, doing downcasts where necessary.
		/// The created delegate will only work in case the casts are valid.
		/// </summary>
		/// <typeparam name = "TDelegate">The type for the delegate to create.</typeparam>
		/// <param name = "toWrap">The delegate which needs to be wrapped by another delegate.</param>
		/// <returns>A new delegate which wraps the passed delegate, doing downcasts where necessary.</returns>
		public static TDelegate WrapDelegate<TDelegate>( Delegate toWrap )
			where TDelegate : class
		{
			if ( !typeof( TDelegate ).GetTypeInfo().IsSubclassOf( typeof( MulticastDelegate ) ) )
			{
				throw new ArgumentException( "Specified type should be a delegate." );
			}

			MethodInfo toCreateInfo = MethodInfoFromDelegateType( typeof( TDelegate ) );
			MethodInfo toWrapInfo = toWrap.GetMethodInfo();

			// Create delegate original and converted parameters.
			// TODO: In the unlikely event that someone would create a delegate with a Closure argument, the following logic will fail. Add precondition to check this?
			IEnumerable<Type> toCreateArguments = toCreateInfo.GetParameters().Select( d => d.ParameterType );
			ParameterInfo[] test = toWrapInfo.GetParameters();
			IEnumerable<Type> toWrapArguments = toWrapInfo.GetParameters()
				// Closure argument isn't an actual argument, but added by the compiler for dynamically generated methods (expression trees).
				.SkipWhile( p => p.ParameterType.FullName == "System.Runtime.CompilerServices.Closure" )	
				.Select( p => p.ParameterType );
			ParameterConversionExpressions parameterExpressions = CreateParameterConversionExpressions( toCreateArguments, toWrapArguments );

			// Create call to wrapped delegate.
			Expression delegateCall = Expression.Invoke(
				Expression.Constant( toWrap ),
				parameterExpressions.ConvertedParameters );

			return Expression.Lambda<TDelegate>(
				ConvertOrWrapDelegate( delegateCall, toCreateInfo.ReturnType ),
				parameterExpressions.OriginalParameters
				).Compile();
		}


		/// <summary>
		/// Creates a delegate of a specified type that represents the specified static or instance method,
		/// with the specified first argument.
		/// </summary>
		/// <typeparam name = "TDelegate">The type for the delegate.</typeparam>
		/// <param name = "method">The MethodInfo describing the static or instance method the delegate is to represent.</param>
		/// <param name = "instance">When method is an instance method, the instance to call this method on. Null for static methods.</param>
		/// <param name = "options">Options which specify what type of delegate should be created.</param>
		public static TDelegate CreateDelegate<TDelegate>(
			MethodInfo method,
			object instance = null,
			CreateOptions options = CreateOptions.None )
			where TDelegate : class
		{
			switch ( options )
			{
				case CreateOptions.None:
					// Ordinary delegate creation, maintaining variance safety.
					return method.CreateDelegate( typeof( TDelegate ), instance ) as TDelegate;

				case CreateOptions.Downcasting:
					{
						MethodInfo delegateInfo = MethodInfoFromDelegateType( typeof( TDelegate ) );

						// Create delegate original and converted arguments.
						IEnumerable<Type> delegateTypes = delegateInfo.GetParameters().Select( d => d.ParameterType );
						IEnumerable<Type> methodTypes = method.GetParameters().Select( p => p.ParameterType );
						ParameterConversionExpressions delegateParameterExpressions = CreateParameterConversionExpressions( delegateTypes, methodTypes );

						// Create method call.
						Expression methodCall = Expression.Call(
							instance == null ? null : Expression.Constant( instance ),
							method,
							delegateParameterExpressions.ConvertedParameters );

						return Expression.Lambda<TDelegate>(
							ConvertOrWrapDelegate( methodCall, delegateInfo.ReturnType ), // Convert return type when necessary.
							delegateParameterExpressions.OriginalParameters
							).Compile();
					}

				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Creates a delegate of a specified type that represents a method which can be executed on an instance passed as parameter.
		/// </summary>
		/// <typeparam name = "TDelegate">
		/// The type for the delegate. This delegate needs at least one (first) type parameter denoting the type of the instance
		/// which will be passed.
		/// E.g., Action&lt;ExampleObject, object&gt;,
		/// where ExampleObject denotes the instance type and object denotes the desired type of the first parameter of the method.
		/// </typeparam>
		/// <param name = "method">The MethodInfo describing the method of the instance type.</param>
		/// <param name = "options">Options which specify what type of delegate should be created.</param>
		public static TDelegate CreateOpenInstanceDelegate<TDelegate>(
			MethodInfo method,
			CreateOptions options = CreateOptions.None )
			where TDelegate : class
		{
			if ( method.IsStatic )
			{
				throw new ArgumentException( "Since the delegate expects an instance, the method cannot be static.", nameof( method ) );
			}

			switch ( options )
			{
				case CreateOptions.None:
					// Ordinary delegate creation, maintaining variance safety.
					return method.CreateDelegate( typeof( TDelegate ) ) as TDelegate;

				case CreateOptions.Downcasting:
					{
						MethodInfo delegateInfo = MethodInfoFromDelegateType( typeof( TDelegate ) );
						ParameterInfo[] delegateParameters = delegateInfo.GetParameters();

						// Convert instance type when necessary.
						Type delegateInstanceType = delegateParameters.Select( p => p.ParameterType ).First();
						Type methodInstanceType = method.DeclaringType;
						ParameterExpression instance = Expression.Parameter( delegateInstanceType );
						Expression convertedInstance = ConvertOrWrapDelegate( instance, methodInstanceType );

						// Create delegate original and converted arguments.
						IEnumerable<Type> delegateTypes = delegateParameters.Select( d => d.ParameterType ).Skip( 1 );
						IEnumerable<Type> methodTypes = method.GetParameters().Select( m => m.ParameterType );
						ParameterConversionExpressions delegateParameterExpressions = CreateParameterConversionExpressions( delegateTypes, methodTypes );

						// Create method call.
						Expression methodCall = Expression.Call(
							convertedInstance,
							method,
							delegateParameterExpressions.ConvertedParameters );					

						return Expression.Lambda<TDelegate>(
							ConvertOrWrapDelegate( methodCall, delegateInfo.ReturnType ), // Convert return type when necessary.
							new[] { instance }.Concat( delegateParameterExpressions.OriginalParameters )
							).Compile();
					}

				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Creates the expressions for delegate parameters and their conversions
		/// to the corresponding required types where necessary.
		/// </summary>
		/// <param name = "toCreateTypes">The types of the parameters of the delegate to create.</param>
		/// <param name = "toWrapTypes">The types of the parameters of the call to wrap.</param>
		/// <returns>An object containing the delegate expressions.</returns>
		static ParameterConversionExpressions CreateParameterConversionExpressions(
			IEnumerable<Type> toCreateTypes,
			IEnumerable<Type> toWrapTypes )
		{
			ParameterExpression[] originalParameters = toCreateTypes.Select( Expression.Parameter ).ToArray(); // ToArray prevents deferred execution.   
			
			return new ParameterConversionExpressions
			{
				OriginalParameters = originalParameters,

				// Convert the parameters from the delegate parameter type to the required type when necessary.
				ConvertedParameters = originalParameters.Zip( toWrapTypes, ConvertOrWrapDelegate )
			};
		}

		/// <summary>
		/// Converts the result of the given expression to the desired type,
		/// or when it is a delegate, tries to wrap it with a delegate which attempts to do downcasts where necessary.
		/// </summary>
		/// <param name = "expression">The expression of which the result needs to be converted.</param>
		/// <param name = "toType">The type to which the result needs to be converted.</param>
		/// <returns>An expression which converts the given expression to the desired type.</returns>
		static Expression ConvertOrWrapDelegate( Expression expression, Type toType )
		{
			Expression convertedExpression;
			TypeInfo fromTypeInfo = expression.Type.GetTypeInfo();
			TypeInfo toTypeInfo = toType.GetTypeInfo();

			if ( toTypeInfo == fromTypeInfo )
			{
				convertedExpression = expression;	// No conversion of the return type needed.
			}
			else
			{
				// TODO: CanConvertTo is incomplete. For the current purpose it returns the correct result, but might not in all cases.
				if ( fromTypeInfo.CanConvertTo( toTypeInfo, CastType.SameHierarchy ) )
				{
					convertedExpression = Expression.Convert( expression, toType );
				}
				else
				{
					// When the return type is a delegate, attempt recursively wrapping it, adding extra conversions where needed. E.g. Func<T>
					if ( fromTypeInfo.IsDelegate() && fromTypeInfo.IsGenericType )
					{
						Func<Delegate, object> wrapDelegateDelegate = WrapDelegate<object>;
						MethodInfo wrapDelegateMethod = wrapDelegateDelegate.GetMethodInfo().GetGenericMethodDefinition( toType );
						MethodCallExpression wrapDelegate = Expression.Call( wrapDelegateMethod, expression );
						convertedExpression = wrapDelegate;
					}
					else
					{
						throw new InvalidOperationException( "Can't downcast the return type to its desired type." );
					}
				}
			}

			return convertedExpression;
		}
	}
}