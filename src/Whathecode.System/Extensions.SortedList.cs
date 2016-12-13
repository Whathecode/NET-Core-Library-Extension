using System;
using System.Collections.Generic;


namespace Whathecode.System
{
    public static class Extensions
    {
		/// <summary>
		/// Get the interval in between which all keys lie.
		/// </summary>
		/// <param name = "source">The source for this extension method.</param>
		/// <returns>The interval in between which all keys lie.</returns>
		public static IInterval<TKey> GetKeysInterval<TKey, TValue>( this SortedList<TKey, TValue> source )
			where TKey : IComparable<TKey>
		{
			return new Interval<TKey>( source.Keys[ 0 ], source.Keys[ source.Count - 1 ] );
		}
    }
}
