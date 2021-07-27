using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TF47_Prism_Sharp.Helper
{
    public class ThreadSafeQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly SemaphoreSlim _lock;

        public ThreadSafeQueue()
        {
            _queue = new Queue<T>();
            _lock = new SemaphoreSlim(1);
        }

        public async Task EnqueueAsync(T element, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            _queue.Enqueue(element);
            _lock.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            var result = _queue.Dequeue();
            _lock.Release();
            return result;
        }

        public int Count => _queue.Count;
        public bool IsEmpty => _queue.Count == 0;
    }
}