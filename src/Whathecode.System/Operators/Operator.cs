﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Whathecode.System.Operators
{
	/// <summary>
	/// Allows access to standard operators (e.g., addition) when the type isn't known at compile time.
	/// Type inference is used at run time.
	/// The generic <see cref="Operator{T}" /> is more efficient when the type is known.
	/// </summary>
	/// <author>Based on work by Marc Gravell contributed to the MiscUtils library.</author>
	public static class Operator
	{
		#region Field operators.

		/// <summary>
		/// Evaluates binary addition (+) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T Add<T>( T value1, T value2 )
		{
			return Operator<T>.Add( value1, value2 );
		}

		/// <summary>
		/// Evaluates binary subtraction (-) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T Subtract<T>( T value1, T value2 )
		{
			return Operator<T>.Subtract( value1, value2 );
		}

		/// <summary>
		/// Evaluates binary multiplication (*) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T Multiply<T>( T value1, T value2 )
		{
			return Operator<T>.Multiply( value1, value2 );
		}

		/// <summary>
		/// Evaluates binary division (/) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T Divide<T>( T value1, T value2 )
		{
			return Operator<T>.Divide( value1, value2 );
		}

		#endregion // Field operators.


		#region // Bitwise operators.

		/// <summary>
		/// Evaluates bitwise and (&amp;) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T And<T>( T value1, T value2 )
		{
			return BitwiseOperator<T>.And( value1, value2 );
		}

		/// <summary>
		/// Evaluates bitwise inclusive or (|) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T Or<T>( T value1, T value2 )
		{
			return BitwiseOperator<T>.Or( value1, value2 );
		}

		/// <summary>
		/// Evaluates bitwise exclusive or (^) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T ExclusiveOr<T>( T value1, T value2 )
		{
			return BitwiseOperator<T>.ExclusiveOr( value1, value2 );
		}

		/// <summary>
		/// Evaluates bitwise not (~) for the given type.
		/// </summary>
		/// <exception cref = "InvalidOperationException">The generic type does not provide this operator.</exception>
		public static T Not<T>( T value )
		{
			return BitwiseOperator<T>.Not( value );
		}

		#endregion	// Bitwise operators.
	}

	/// <summary>
	/// Allows access to standard operators (e.g., addition) for a generic type.
	/// </summary>
	/// <remarks>
	/// Lazy backing fields are used to prevent initializing an operator which isn't supported by the type.
	/// </remarks>
	public static class Operator<T>
	{
		#region Field operators.

		static readonly Lazy<Func<T, T, T>> AddLazy;
		/// <summary>
		/// A delegate to evaluate binary addition (+) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> Add => AddLazy.Value;

		static readonly Lazy<Func<T, T, T>> SubtractLazy;
		/// <summary>
		/// A delegate to evaluate binary subtraction (-) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> Subtract => SubtractLazy.Value;

		static readonly Lazy<Func<T, T, T>> MultiplyLazy;
		/// <summary>
		/// A delegate to evaluate binary multiplication (*) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> Multiply => MultiplyLazy.Value;

		static readonly Lazy<Func<T, T, T>> DivideLazy;
		/// <summary>
		/// A delegate to evaluate binary division (/) for the given type.
		/// This delegate will throw an <see cref = "InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> Divide => DivideLazy.Value;

		#endregion // Field operators.


		#region Bitwise operators.

		static readonly Lazy<Func<T, T, T>> AndLazy;
		/// <summary>
		/// A delegate to evaluate bitwise and (&amp;) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> And => AndLazy.Value;

		static readonly Lazy<Func<T, T, T>> OrLazy;
		/// <summary>
		/// A delegate to evaluate bitwise inclusive or (|) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> Or => OrLazy.Value;

		static readonly Lazy<Func<T, T, T>> ExclusiveOrLazy;
		/// <summary>
		/// A delegate to evaluate bitwise exclusive or (^) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, T> ExclusiveOr => ExclusiveOrLazy.Value;

		static readonly Lazy<Func<T, T>> NotLazy;
		/// <summary>
		/// A delegate to evaluate bitwise not (~) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type T does not provide this operator.
		/// </summary>
		public static Func<T, T> Not => NotLazy.Value;

		#endregion // Bitwise operators.


		static Operator()
		{
			// Field operators.
			AddLazy = new Lazy<Func<T, T, T>>( () => ExpressionHelper.CompileBinaryExpression<T, T, T>( Expression.Add ) );
			SubtractLazy = new Lazy<Func<T, T, T>>( () => ExpressionHelper.CompileBinaryExpression<T, T, T>( Expression.Subtract ) );
			MultiplyLazy = new Lazy<Func<T, T, T>>( () => ExpressionHelper.CompileBinaryExpression<T, T, T>( Expression.Multiply ) );
			DivideLazy = new Lazy<Func<T, T, T>>( () => ExpressionHelper.CompileBinaryExpression<T, T, T>( Expression.Divide ) );

			// Bitwise operators.
			AndLazy = new Lazy<Func<T, T, T>>( () => BitwiseOperator<T>.And );
			OrLazy = new Lazy<Func<T, T, T>>( () => BitwiseOperator<T>.Or );
			ExclusiveOrLazy = new Lazy<Func<T, T, T>>( () => BitwiseOperator<T>.ExclusiveOr );
			NotLazy = new Lazy<Func<T, T>>( () => BitwiseOperator<T>.Not );
		}
	}


	/// <summary>
	/// Allows access to standard operators (e.g., addition) for a generic type,
	/// where the type which identifies distances between two instances is different. E.g., DateTime and TimeSpan.
	/// TODO: Could the other operator class extend from this one?
	/// </summary>
	/// <remarks>
	/// Lazy backing fields are used to prevent initializing an operator which isn't supported by the type.
	/// </remarks>
	public static class Operator<T, TSize>
	{
		#region Field operators.

		static readonly Lazy<Func<T, TSize, T>> AddSizeLazy;
		/// <summary>
		/// A delegate to evaluate binary addition (+) of a size to the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, TSize, T> AddSize => AddSizeLazy.Value;

		static readonly Lazy<Func<T, T, TSize>> SubtractLazy;
		/// <summary>
		/// A delegate to evaluate binary subtraction (-) for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, T, TSize> Subtract => SubtractLazy.Value;

		static readonly Lazy<Func<T, TSize, T>> SubtractSizeLazy;
		/// <summary>
		/// A delegate to evaluate binary subtraction (-) of a size for the given type.
		/// This delegate will throw an <see cref="InvalidOperationException" /> if the type does not provide this operator.
		/// </summary>
		public static Func<T, TSize, T> SubtractSize => SubtractSizeLazy.Value;

		#endregion // Field operators.


		// TODO: Due to a bug in .NETStandard v1.6, the correct add and subtract methods are not automatically selected based on type.
		//       Once this is fixed, the static constructor commented out below can be reused instead: http://stackoverflow.com/q/39601155/590790
		static Operator()
		{
			// Field operators.
			TypeInfo sourceType = typeof( T ).GetTypeInfo();
			if ( sourceType.IsPrimitive ) // The primitve numeric types do not have custom operators in their type definitions. 
			{
				AddSizeLazy = new Lazy<Func<T, TSize, T>>( () => ExpressionHelper.CompileBinaryExpression<T, TSize, T>( Expression.Add ) );
				SubtractLazy = new Lazy<Func<T, T, TSize>>( () => ExpressionHelper.CompileBinaryExpression<T, T, TSize>( Expression.Subtract ) );
				SubtractSizeLazy = new Lazy<Func<T, TSize, T>>( () => ExpressionHelper.CompileBinaryExpression<T, TSize, T>( Expression.Subtract ) );
			}
			else
			{
				AddSizeLazy = new Lazy<Func<T, TSize, T>>( () => CompileBinaryExpression<T, TSize, T>( Expression.Add, "op_Addition" ) );
				SubtractLazy = new Lazy<Func<T, T, TSize>>( () => CompileBinaryExpression<T, T, TSize>( Expression.Subtract, "op_Subtraction" ) );
				SubtractSizeLazy = new Lazy<Func<T, TSize, T>>( () => CompileBinaryExpression<T, TSize, T>( Expression.Subtract, "op_Subtraction" ) );
			}
		}
		static Func<TArg1, TArg2, TResult> CompileBinaryExpression<TArg1, TArg2, TResult>(
			Func<Expression, Expression, MethodInfo, BinaryExpression> operation,
			string methodName )
		{
			TypeInfo sourceType = typeof( TArg1 ).GetTypeInfo();
			MethodInfo method = (
				from m in sourceType.GetDeclaredMethods( methodName )
				let p = m.GetParameters()
				where p.Length == 2 && p[ 1 ].ParameterType == typeof( TArg2 ) && m.ReturnType == typeof( TResult )
				select m
				).FirstOrDefault();
			return ExpressionHelper.CompileBinaryExpression<TArg1, TArg2, TResult>( operation, method );
		}
		/*static Operator()
		{
			// Field operators.
			AddSizeLazy = new Lazy<Func<T, TSize, T>>( () => ExpressionHelper.CompileBinaryExpression<T, TSize, T>( Expression.Add ) );
			SubtractLazy = new Lazy<Func<T, T, TSize>>( () => ExpressionHelper.CompileBinaryExpression<T, T, TSize>( Expression.Subtract ) );
			SubtractSizeLazy = new Lazy<Func<T, TSize, T>>( () => ExpressionHelper.CompileBinaryExpression<T, TSize, T>( Expression.Subtract ) );
		}*/
	}
}
