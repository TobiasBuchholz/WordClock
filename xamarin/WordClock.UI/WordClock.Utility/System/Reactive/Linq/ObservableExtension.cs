using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Genesis.Ensure;
using Genesis.Logging;
using ReactiveUI;

namespace System.Reactive.Linq
{
    public static class ObservableExtension
    {
        /// <summary>
        /// Skips elements from the <paramref name="source" /> sequence as soon as an element of the
        /// <paramref name="throttler" /> sequence arrives, for the specified duration.
        /// </summary>
        public static IObservable<T> ThrottleWhenIncoming<T, TDontCare>(this IObservable<T> source, IObservable<TDontCare> throttler, TimeSpan throttleDuration, IScheduler scheduler)
        {
            bool acceptElements = true;

            throttler.Do(_ => acceptElements = false)
                     .Throttle(throttleDuration, scheduler)
                     .Subscribe(_ => acceptElements = true);

            return source.Where(_ => acceptElements);
        }

        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, Task<bool>> predicate)
        {
            return source.SelectMany(async item => new
            {
                ShouldInclude = await predicate(item),
                Item = item
            })
                         .Where(x => x.ShouldInclude)
                         .Select(x => x.Item);
        }

        public static IObservable<T> DoOnError<T>(this IObservable<T> source, Action<Exception> action)
        {
            return source.Do(_ => { }, action);
        }

        public static IObservable<T> DoOnCompleted<T>(this IObservable<T> source, Action action)
        {
            return source.Do(_ => { }, action);
        }

        public static IObservable<TSource> RetryAfterDelay<TSource, TException>(this IObservable<TSource> source,
                                                                                TimeSpan retryDelay,
                                                                                int retryCount,
                                                                                IScheduler scheduler) where TException : Exception
        {
            return source.Catch<TSource, TException>(ex =>
            {
                if (retryCount <= 0)
                {
                    return Observable.Throw<TSource>(ex);
                }
                return source.DelaySubscription(retryDelay, scheduler)
                             .RetryAfterDelay<TSource, TException>(retryDelay, --retryCount, scheduler);
            });
        }

        public static IDisposable ExecuteNow<TParam, TResult>(this ReactiveCommandBase<TParam, TResult> source, CompositeDisposable disposable)
        {
            return source.Execute().SubscribeSafe().DisposeWith(disposable);
        }

        public static IDisposable ExecuteNow<TParam, TResult>(this ReactiveCommandBase<TParam, TResult> source, TParam param, CompositeDisposable disposable)
        {
            return source.Execute(param).SubscribeSafe().DisposeWith(disposable);
        }

        public static ReactiveCommandBase<TParam, TResult> LogThrownExceptions<TParam, TResult>(this ReactiveCommandBase<TParam, TResult> command, CompositeDisposable disposables)
        {
            command
                .ThrownExceptions
                .SubscribeSafe(LogException)
                .DisposeWith(disposables);
            return command;
        }

        public static void LogThrownExceptions<T>(this ObservableAsPropertyHelper<T> propertyHelper, CompositeDisposable disposable)
        {
            propertyHelper
                .ThrownExceptions
                .SubscribeSafe(LogException)
                .DisposeWith(disposable);
        }

        private static void LogException(Exception e)
        {
            var logger = LoggerService.GetLogger(typeof(ObservableExtension));
            logger.Error(e, "An exception was caught:\n");
        }

        public static IObservable<T> CatchAndLogException<T>(this IObservable<T> source, IObservable<T> second = null)
        {
            return source.Catch<T, Exception>(e => LogCatchedException(e, second));
        }

        private static IObservable<T> LogCatchedException<T>(Exception e, IObservable<T> second = null)
        {
            LogException(e);
            return second ?? Observable.Empty<T>();
        }

        public static IDisposable DisposableEvents(this object target, string eventName, EventHandler<EventArgs> handler)
        {
            return DisposableEvents<EventArgs>(target, eventName, handler);
        }

        public static IDisposable DisposableEvents<TEventArgs>(this object target, string eventName, EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            return Observable.FromEventPattern<TEventArgs>(target, eventName)
                             .Subscribe(pattern => handler(pattern.Sender, pattern.EventArgs));
        }

        public static IObservable<Unit> ToSignal<T>(this IObservable<T> @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this
                .Select(_ => Unit.Default);
        }

        public static IObservable<bool> ToTrue<T>(this IObservable<T> @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this
                .Select(_ => true);
        }

        public static IObservable<bool> ToFalse<T>(this IObservable<T> @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this
                .Select(_ => false);
        }

        public static IObservable<IList<T>> ToListAsync<T>(this IObservable<T> @this, TimeSpan? timeout = null)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this.Timeout(timeout.GetValueOrDefault(TimeSpan.FromSeconds(3)))
                        .Buffer(int.MaxValue)
                        .FirstAsync();
        }

        public static IObservable<T> DoWhere<T>(this IObservable<T> source, Func<T, bool> predicate, Action<T> action)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            Ensure.ArgumentNotNull(predicate, nameof(predicate));

            return source.Do(t =>
            {
                if (predicate(t))
                {
                    action(t);
                }
            });
        }

        public static IObservable<bool> WhereIsTrue(this IObservable<bool> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x);
        }

        public static IObservable<bool> WhereIsFalse(this IObservable<bool> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => !x);
        }

        public static IObservable<T> Iterate<T>(this IObservable<IEnumerable<T>> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.SelectMany(item => item);
        }

        public static IObservable<bool> Negate(this IObservable<bool> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Select(x => !x);
        }

        public static IObservable<T> Debug<T>(this IObservable<T> source, Func<T, string> debugMessage)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Do(t => Diagnostics.Debug.WriteLine(debugMessage(t)));
        }

        public static IObservable<T> DebugWriteLine<T>(this IObservable<T> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Do(t => Diagnostics.Debug.WriteLine(t));
        }

        public static IObservable<IEnumerable<T>> WhereIsEmpty<T>(this IObservable<IEnumerable<T>> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => !x.Any());
        }

        public static IObservable<IList<T>> WhereIsEmpty<T>(this IObservable<IList<T>> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => !x.Any());
        }

        public static IObservable<IEnumerable<T>> WhereHasItems<T>(this IObservable<IEnumerable<T>> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source
                .WhereNotNull()
                .Where(x => x.Any());
        }

        public static IObservable<IList<T>> WhereHasItems<T>(this IObservable<IList<T>> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source
                .WhereNotNull()
                .Where(x => x.Any());
        }

        public static IObservable<T> WhereNotNull<T>(this IObservable<T> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x != null);
        }

        public static IObservable<int> WhereIsZero(this IObservable<int> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x == 0);
        }

        public static IObservable<int> WhereGreaterZero(this IObservable<int> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x > 0);
        }

        public static IObservable<long> WhereGreaterZero(this IObservable<long> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x > 0);
        }

        public static IObservable<int> WhereGreaterEqualZero(this IObservable<int> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x >= 0);
        }

        public static IObservable<long> WhereIsZero(this IObservable<long> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x == 0);
        }

        public static IObservable<long> WhereGreaterEqualZero(this IObservable<long> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x >= 0);
        }

        public static IObservable<int> WhereLessZero(this IObservable<int> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x < 0);
        }

        public static IObservable<long> WhereLessZero(this IObservable<long> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x < 0);
        }

        public static IObservable<int> WhereLessEqualZero(this IObservable<int> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x <= 0);
        }

        public static IObservable<long> WhereLessEqualZero(this IObservable<long> source)
        {
            Ensure.ArgumentNotNull(source, nameof(source));
            return source.Where(x => x <= 0);
        }

        public static IObservable<string> FromPropertyChanged<T>(T item) where T : INotifyPropertyChanged
        {
            return Observable
                .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => item.PropertyChanged += h, h => item.PropertyChanged -= h)
                .Select(x => x.EventArgs.PropertyName);
        }

        public static IObservable<TProperty> FromPropertyChanged<TItem, TProperty>(TItem item, string propertyName, Func<TProperty> selector) where TItem : INotifyPropertyChanged
        {
            return FromPropertyChanged(item, propertyName)
                .Select(_ => selector());
        }

        public static IObservable<Unit> FromPropertyChanged<T>(T item, string propertyName) where T : INotifyPropertyChanged
        {
            return Observable
                .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => item.PropertyChanged += h, h => item.PropertyChanged -= h)
                .Select(x => x.EventArgs.PropertyName)
                .Where(x => x.Equals(propertyName))
                .ToSignal();
        }

        public static IObservable<bool> IsActivated(this ICanActivate @this) 
        {
            return @this
                .Activated
                .ToTrue()
                .Merge(@this.Deactivated.ToFalse());
        }

        public static IObservable<T> SkipWhere<T>(this IObservable<T> source, int count, Func<T, bool> predicate)
        {
            return source
                .FirstAsync()
                .Select(x => predicate(x))
                .SelectMany(x => x ? source.Skip(count) : source);
        }

        public static IObservable<Unit> CombineLatestToSignal<T, S>(this IObservable<T> first, IObservable<S> second)
        {
            return first.CombineLatest(second, (_, __) => Unit.Default);
        }
    }
}
