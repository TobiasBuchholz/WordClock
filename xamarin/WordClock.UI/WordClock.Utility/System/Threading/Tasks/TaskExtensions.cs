
using Genesis.Ensure;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static void Ignore(this Task @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
        }

        public static void Ignore<T>(this Task<T> @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
        }
    }
}
