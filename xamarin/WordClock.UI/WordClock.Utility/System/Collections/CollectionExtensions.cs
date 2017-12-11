using System.Collections.Generic;

namespace System.Collections
{
	public static class CollectionExtensions
	{
		public static T[] ToSingleArray<T>(this T @this)
		{
			var array = new T[1];
			array[0] = @this;
			return array;
		}

        public static void AddRange<T>(this IList<T> @this, IList<T> itemsToAdd)
        {
            foreach(var t in itemsToAdd) {
                @this.Add(t);
            }
        }
	}
}
