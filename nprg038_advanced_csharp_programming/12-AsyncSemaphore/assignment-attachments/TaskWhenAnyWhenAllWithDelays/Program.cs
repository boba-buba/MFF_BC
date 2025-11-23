using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TaskWhenAnyWhenAllWithDelays {
	class Program {
		static void Main(string[] args) {
			{
				var sw = Stopwatch.StartNew();

				Task.WaitAll(Task.Delay(200), Task.Delay(500), Task.Delay(1000));
				
				sw.Stop();
				Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: with Task.WaitAll elapsed {sw.Elapsed} ms");
			}

			{
				var sw = Stopwatch.StartNew();

				int taskIndex = Task.WaitAny(Task.Delay(200), Task.Delay(500), Task.Delay(1000));

				sw.Stop();
				Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: with Task.WaitAny elapsed {sw.Elapsed} ms");
			}

			{
				var sw = Stopwatch.StartNew();

				Task aggregationTask = Task.WhenAll(Task.Delay(200), Task.Delay(500), Task.Delay(1000));
				aggregationTask.Wait();

				sw.Stop();
				Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: with Task.WhenAll elapsed {sw.Elapsed} ms");
			}

			{
				var sw = Stopwatch.StartNew();

				Task<Task> aggregationTask = Task.WhenAny(Task.Delay(200), Task.Delay(500), Task.Delay(1000));
				aggregationTask.Wait();

				sw.Stop();
				Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: with Task.WhenAny elapsed {sw.Elapsed} ms");
			}

			{
				var sw = Stopwatch.StartNew();

				var aggregationTask = Task.WhenAll(Task.Delay(200), Task.Delay(500), Task.Delay(1000));
				aggregationTask.ContinueWith(completedTask => {
					sw.Stop();
					Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: with Task.WhenAll and ContinueWith elapsed {sw.Elapsed} ms");
					Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:      + completedTask is {completedTask}");
				});
			}

			{
				var sw = Stopwatch.StartNew();

				var aggregationTask = Task.WhenAny(Task.Delay(200), Task.Delay(500), Task.Delay(1000));
				aggregationTask.ContinueWith(completedTask => {
					sw.Stop();
					Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: with Task.WhenAny and ContinueWith elapsed {sw.Elapsed} ms");
					Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:      + completedTask is {completedTask}");
				});
			}

			Thread.Sleep(5000);
		}
	}
}
