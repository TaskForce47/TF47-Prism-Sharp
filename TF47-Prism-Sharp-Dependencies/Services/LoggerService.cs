using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TF47_Prism_Sharp_Dependencies.Services
{
    public class LoggerService
    {
        private readonly bool _logToConsole;
        private readonly string _logFile;
        private readonly ConcurrentQueue<string> _logsToWrite = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        public LoggerService(string module, bool logToConsole)
        {
            _logToConsole = logToConsole;
            var logFolder = Path.Combine(Environment.CurrentDirectory, "tf47prism-logs", module);
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            _logFile = Path.Combine(logFolder, $"{DateTime.Now:yyyy-M-d hh:mm:ss}.txt");
            if (!File.Exists(_logFile))
                File.Create(_logFile);

            Task.Run(async () =>
            {
                await LogWriter(_cancellationTokenSource.Token);
            }).ConfigureAwait(false);
        }

        ~LoggerService()
        {
            _cancellationTokenSource.Cancel();
        }

        public void LogInformation(string message)
        {
            message = $"{DateTime.Now:hh:mm:ss} Information: {message}";
            _logsToWrite.Enqueue(message);
            if (_logToConsole) Console.WriteLine(message);
        }
        
        public void LogWarning(string message)
        {
            message = $"{DateTime.Now:hh:mm:ss} Warning: {message}";
            _logsToWrite.Enqueue(message);
            if (_logToConsole) Console.WriteLine(message);
        }
        
        public void LogError(string message)
        {
            message = $"{DateTime.Now:hh:mm:ss} Error: {message}";
            _logsToWrite.Enqueue(message);
            if (_logToConsole) Console.WriteLine(message);
        }

        private async Task LogWriter(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_logsToWrite.IsEmpty)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                List<string> temp = new();
                while (!_logsToWrite.IsEmpty)
                {
                    if (_logsToWrite.TryDequeue(out string message))
                        temp.Add(message);
                    else
                        break;
                }

                await File.AppendAllLinesAsync(_logFile, temp, Encoding.UTF8, cancellationToken);
            }
        }
    }
}