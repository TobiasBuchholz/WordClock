using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace System.Reactive.Concurrency
{
    public class SingleThreadScheduler : IScheduler
    {
        private readonly TaskFactory _taskFactory;

        public SingleThreadScheduler(TaskScheduler staTaskScheduler)
        {
            _taskFactory = new TaskFactory(staTaskScheduler);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var innerDisp = new SingleAssignmentDisposable();
            _taskFactory.StartNew(() => {
                if (!innerDisp.IsDisposed) innerDisp.Disposable = action(this, state);
            });
            return innerDisp;
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
