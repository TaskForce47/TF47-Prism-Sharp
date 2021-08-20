using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TF47_Prism_Sharp_Dependencies.Services
{
    public class ArmaCallbackService
    {
        private readonly unsafe delegate*unmanaged<string, string, string, int> _extensionCallback;
        private readonly ConcurrentQueue<(string, string)> _queue;
        private CancellationTokenSource _cancellationTokenSource;
        
        public unsafe ArmaCallbackService(delegate*unmanaged<string, string, string, int> extensionCallback)
        {
            _extensionCallback = extensionCallback;
            _queue = new();
        }
        

        public void EnqueueEngineCallback(string function, string data)
        {
            _queue.Enqueue((function, data));
        }

        public ArmaCallbackService StartEngineCallback()
        {
            _cancellationTokenSource = new();
            var thread = new Thread(async () =>
            {
                while (! _cancellationTokenSource.IsCancellationRequested)
                {
                    while (!_queue.IsEmpty)
                    {
                        if (!_queue.TryDequeue(out (string function, string data) item)) continue;

                        var result = CallEngine(ref item.function, ref item.data);
                        
                        while (result == -1)
                        {
                            Debug.WriteLine($"Callback to engine is full! Waiting...");
                            await Task.Delay(10, _cancellationTokenSource.Token);
                            result = CallEngine(ref item.function, ref item.data);
                        }
                        Debug.WriteLine($"Data send back to engine: {item.Item1} {item.Item2}");
                    }
                }
            });
            thread.Start();
            return this;
        }

        private int CallEngine(ref string function, ref string data)
        {
            unsafe
            {
                return _extensionCallback("TF47PrismSharp", function, data);
            }
        }

        public void StopEngineCallback()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}