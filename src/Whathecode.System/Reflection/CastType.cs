namespace Whathecode.System.Reflection
{
	public enum CastType
	{
		/// <summary>
		/// Only consider implicit conversions.
		/// </summary>
		Implicit,
		/// <summary>
		/// Consider all possible conversions within the same hierarchy which are possible using an explicit cast.
		/// </summary>
		SameHierarchy
	}
}
