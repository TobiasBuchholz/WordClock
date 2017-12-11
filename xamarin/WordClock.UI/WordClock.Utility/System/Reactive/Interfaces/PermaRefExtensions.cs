using System.Reactive.Subjects;

namespace System.Reactive.Interfaces
{
	public static class PermaRefExtensions
	{
		public static IObservable<T> PermaRef<T>(this IConnectableObservable<T> observable)
		{
			observable.Connect();
			return observable;
		}
	}
}
