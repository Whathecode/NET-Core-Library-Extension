using System;


namespace Whathecode.System.Reflection
{
	/// <summary>
	/// An attribute which contains an instance of a specific type, implementing desired behavior to be called during reflection.
	/// By passing a type argument and arguments for its constructor, it is possible to instantiate any desired type.
	/// This circumvents attribute limitations:
	/// "An attribute argument must be a constant expression, typeof expression or array creation expression of an attribute parameter type."
	/// TODO: Add 'Type ExpectedType' attribute to further remove duplicate code from existing extending classes.
	/// </summary>
	public abstract class AbstractBehaviorAttribute : Attribute
	{
		/// <summary>
		/// The instance of the specified type created in the constructor.
		/// </summary>
		protected readonly object Instance;


		/// <summary>
		/// Create a new attribute which contains an instance of a specified type.
		/// </summary>
		/// <param name = "type">The type to initialize.</param>
		/// <param name = "constructorArguments">The arguments to pass to the constructor of the type.</param>
		protected AbstractBehaviorAttribute( Type type, params object[] constructorArguments )
		{
			Instance = Activator.CreateInstance( type, constructorArguments );
		}
	}
}