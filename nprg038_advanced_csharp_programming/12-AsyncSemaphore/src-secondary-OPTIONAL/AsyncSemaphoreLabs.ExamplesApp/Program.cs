using System;
using System.Threading;
using System.Threading.Tasks;

using AsyncSemaphoreLabs.Lib;

namespace AsyncSemaphoreLabs.ExamplesApp {
	
	class Example_AsyncUsageWithOptionalTimeout {
		public static void Run() {
			var s1 = new AsyncSemaphore(2, 2);

			SingleRunWithTimeout(s1, Timeout.Infinite);
			Thread.Sleep(500);
			Console.WriteLine();

			Console.WriteLine("### Release(2) ###");
			s1.Release(2);
			Thread.Sleep(500);
			Console.WriteLine();

			SingleRunWithTimeout(s1, 5000);
			Thread.Sleep(500);
			Console.WriteLine();

			Console.WriteLine("### Release(2) ###");
			s1.Release(2);
			Thread.Sleep(500);
			Console.WriteLine();

			SingleRunWithTimeout(s1, 200);
			Thread.Sleep(500);
			Console.WriteLine();

			SingleRunWithTimeout(s1, 200);
		}

		private static void SingleRunWithTimeout(AsyncSemaphore s1, int timeout) {
			Console.WriteLine($"*** AsyncSemaphore.CurrentCount == {s1.CurrentCount} *** all WaitAsync with timeout == {timeout}");

			var t00 = s1.WaitAsync(timeout);
			PrintTaskInfo("Task t00", t00);
			t00.ContinueWith(task => PrintTaskInfo("Continuation of t00", task));

			var t01 = s1.WaitAsync(timeout);
			PrintTaskInfo("Task t01", t01);
			t01.ContinueWith(task => PrintTaskInfo("Continuation of t01", task));

			Thread.Sleep(50);
			Console.WriteLine("---");

			var t1 = s1.WaitAsync(timeout);
			PrintTaskInfo("Task t1", t1);
			t1.ContinueWith(task => PrintTaskInfo("Continuation of t1", task));

			var t2 = s1.WaitAsync(timeout);
			PrintTaskInfo("Task t2", t2);
			t2.ContinueWith(task => PrintTaskInfo("Continuation of t2", task));

			var t3 = s1.WaitAsync(timeout);
			PrintTaskInfo("Task t3", t3);
			t3.ContinueWith(task => PrintTaskInfo("Continuation of t3", task));

			Console.WriteLine("*** Release(2) ***");
			s1.Release(2);

			Thread.Sleep(500);

			var t4 = s1.WaitAsync(timeout);
			PrintTaskInfo("Task t4", t4);
			t4.ContinueWith(task => PrintTaskInfo("Continuation of t4", task));

			Console.WriteLine("*** Release(1) ***");
			s1.Release(1);

			Thread.Sleep(500);

			Console.WriteLine("*** Release(1) ***");
			s1.Release(1);
		}

		private static void PrintTaskInfo(string message, Task<bool> task) => Console.WriteLine($"{message}: {task.Status} -> {(task.IsCompletedSuccessfully ? task.Result.ToString() : "NONE")} AsyncState == {(task.AsyncState is TaskCompletionSource<bool> ? "TCS -> " + ((TaskCompletionSource<bool>) task.AsyncState).Task.AsyncState?.ToString() : task.AsyncState?.ToString())}");

		// The example displays output like the following:
		//      *** AsyncSemaphore.CurrentCount == 2 *** all WaitAsync with timeout == -1
		//      Task t00: RanToCompletion -> True AsyncState ==
		//      Task t01: RanToCompletion -> True AsyncState ==
		//      Continuation of t01: RanToCompletion -> True AsyncState ==
		//      Continuation of t00: RanToCompletion -> True AsyncState ==
		//      ---
		//      Task t1: WaitingForActivation -> NONE AsyncState == 1
		//      Task t2: WaitingForActivation -> NONE AsyncState == 2
		//      Task t3: WaitingForActivation -> NONE AsyncState == 3
		//      *** Release(2) ***
		//      Continuation of t1: RanToCompletion -> True AsyncState == 1
		//      Continuation of t2: RanToCompletion -> True AsyncState == 2
		//      Task t4: WaitingForActivation -> NONE AsyncState == 4
		//      *** Release(1) ***
		//      Continuation of t3: RanToCompletion -> True AsyncState == 3
		//      *** Release(1) ***
		//      Continuation of t4: RanToCompletion -> True AsyncState == 4

		//      ### Release(2) ###

		//      *** AsyncSemaphore.CurrentCount == 2 *** all WaitAsync with timeout == 5000
		//      Task t00: RanToCompletion -> True AsyncState ==
		//      Task t01: RanToCompletion -> True AsyncState ==
		//      Continuation of t01: RanToCompletion -> True AsyncState ==
		//      Continuation of t00: RanToCompletion -> True AsyncState ==
		//      ---
		//      Task t1: WaitingForActivation -> NONE AsyncState == TCS -> 5
		//      Task t2: WaitingForActivation -> NONE AsyncState == TCS -> 6
		//      Task t3: WaitingForActivation -> NONE AsyncState == TCS -> 7
		//      *** Release(2) ***
		//      Continuation of t2: RanToCompletion -> True AsyncState == TCS -> 6
		//      Continuation of t1: RanToCompletion -> True AsyncState == TCS -> 5
		//      Task t4: WaitingForActivation -> NONE AsyncState == TCS -> 8
		//      *** Release(1) ***
		//      Continuation of t3: RanToCompletion -> True AsyncState == TCS -> 7
		//      *** Release(1) ***
		//      Continuation of t4: RanToCompletion -> True AsyncState == TCS -> 8

		//      ### Release(2) ###

		//      *** AsyncSemaphore.CurrentCount == 2 *** all WaitAsync with timeout == 200
		//      Task t00: RanToCompletion -> True AsyncState ==
		//      Task t01: RanToCompletion -> True AsyncState ==
		//      Continuation of t01: RanToCompletion -> True AsyncState ==
		//      Continuation of t00: RanToCompletion -> True AsyncState ==
		//      ---
		//      Task t1: WaitingForActivation -> NONE AsyncState == TCS -> 9
		//      Task t2: WaitingForActivation -> NONE AsyncState == TCS -> 10
		//      Task t3: WaitingForActivation -> NONE AsyncState == TCS -> 11
		//      *** Release(2) ***
		//      Continuation of t1: RanToCompletion -> True AsyncState == TCS -> 9
		//      Continuation of t2: RanToCompletion -> True AsyncState == TCS -> 10
		//      Continuation of t3: RanToCompletion -> False AsyncState == TCS -> 11
		//      Task t4: WaitingForActivation -> NONE AsyncState == TCS -> 12
		//      *** Release(1) ***
		//      Continuation of t4: RanToCompletion -> True AsyncState == TCS -> 12
		//      *** Release(1) ***

		//      *** AsyncSemaphore.CurrentCount == 1 *** all WaitAsync with timeout == 200
		//      Task t00: RanToCompletion -> True AsyncState ==
		//      Task t01: WaitingForActivation -> NONE AsyncState == TCS -> 13
		//      Continuation of t00: RanToCompletion -> True AsyncState ==
		//      ---
		//      Task t1: WaitingForActivation -> NONE AsyncState == TCS -> 14
		//      Task t2: WaitingForActivation -> NONE AsyncState == TCS -> 15
		//      Task t3: WaitingForActivation -> NONE AsyncState == TCS -> 16
		//      *** Release(2) ***
		//      Continuation of t01: RanToCompletion -> True AsyncState == TCS -> 13
		//      Continuation of t1: RanToCompletion -> True AsyncState == TCS -> 14
		//      Continuation of t2: RanToCompletion -> False AsyncState == TCS -> 15
		//      Continuation of t3: RanToCompletion -> False AsyncState == TCS -> 16
		//      Task t4: WaitingForActivation -> NONE AsyncState == TCS -> 17
		//      *** Release(1) ***
		//      Continuation of t4: RanToCompletion -> True AsyncState == TCS -> 17
		//      *** Release(1) ***
	}

	class Example_SyncUsageWithInfiniteTimeout_AsInSemaphoreSlimDocumentation {
		//
		// Example take from SemaphoreSlim documentation:
		// https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=net-5.0
		// and updated to use our AsyncSemaphore async API synchronously.
		//

		private static AsyncSemaphore s_semaphore;

		private static int s_additionalDelay; // A padding interval to make the output more orderly.

		public static void Run() {
			// Create the semaphore.
			s_semaphore = new AsyncSemaphore(0, 3);
			Console.WriteLine("{0} tasks can enter the semaphore.", s_semaphore.CurrentCount);
			Task[] tasks = new Task[5];

			// Create and start five numbered tasks.
			for (int i = 0; i <= 4; i++) {
				tasks[i] = Task.Run(() => {
					// Each task begins by requesting the semaphore.
					Console.WriteLine("Task {0} begins and waits for the semaphore.", Task.CurrentId);

					int semaphoreCount;
					s_semaphore.WaitAsync(Timeout.Infinite).Wait();
					int currentCount = s_semaphore.CurrentCount;
					try {
						int thisTaskAdditionalDelay = Interlocked.Add(ref s_additionalDelay, 250);

						Console.WriteLine("Task {0} enters the semaphore (unreliable information: CurrentCount == {1}).", Task.CurrentId, currentCount);

						// The task just sleeps for 1+ seconds.
						Thread.Sleep(1000 + thisTaskAdditionalDelay);
					} finally {
						semaphoreCount = s_semaphore.Release(1);
					}
					Console.WriteLine("Task {0} releases the semaphore; previous count: {1}.", Task.CurrentId, semaphoreCount);
				});
			}

			// Wait for half a second, to allow all the tasks to start and block.
			Thread.Sleep(500);

			// Restore the semaphore count to its maximum value.
			Console.Write("Main thread calls Release(3) --> ");
			s_semaphore.Release(3);
			Console.WriteLine("{0} tasks can enter the semaphore.", s_semaphore.CurrentCount);
			// Main thread waits for the tasks to complete.
			Task.WaitAll(tasks);

			Console.WriteLine($"Main thread finished running {nameof(Example_SyncUsageWithInfiniteTimeout_AsInSemaphoreSlimDocumentation)}.");
		}

		// The example displays output like the following:
		//      0 tasks can enter the semaphore.
		//      Task 1 begins and waits for the semaphore.
		//      Task 3 begins and waits for the semaphore.
		//      Task 2 begins and waits for the semaphore.
		//      Task 4 begins and waits for the semaphore.
		//      Task 5 begins and waits for the semaphore.
		//      Main thread calls Release(3) --> 0 tasks can enter the semaphore.
		//      Task 2 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 3 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 1 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 1 releases the semaphore; previous count: 0.
		//      Task 4 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 3 releases the semaphore; previous count: 0.
		//      Task 5 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 2 releases the semaphore; previous count: 0.
		//      Task 4 releases the semaphore; previous count: 1.
		//      Task 5 releases the semaphore; previous count: 2.
		//      Main thread finished running Example_SyncUsageWithInfiniteTimeout_AsInSemaphoreSlimDocumentation.
		//
		//  - OR -
		//
		//      0 tasks can enter the semaphore.
		//      Task 1 begins and waits for the semaphore.
		//      Task 2 begins and waits for the semaphore.
		//      Task 4 begins and waits for the semaphore.
		//      Task 3 begins and waits for the semaphore.
		//      Task 5 begins and waits for the semaphore.
		//      Main thread calls Release(3) --> 0 tasks can enter the semaphore.
		//      Task 1 enters the semaphore (unreliable information: CurrentCount == 1).
		//      Task 2 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 4 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 1 releases the semaphore; previous count: 0.
		//      Task 3 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 2 releases the semaphore; previous count: 0.
		//      Task 5 enters the semaphore (unreliable information: CurrentCount == 0).
		//      Task 4 releases the semaphore; previous count: 0.
		//      Task 3 releases the semaphore; previous count: 1.
		//      Task 5 releases the semaphore; previous count: 2.
		//      Main thread finished running Example_SyncUsageWithInfiniteTimeout_AsInSemaphoreSlimDocumentation.
	}

	class Program {
		static void Main(string[] args) {
			Example_AsyncUsageWithOptionalTimeout.Run();

			Console.WriteLine();
			Console.WriteLine("----------------------------------------------------------");
			Console.WriteLine();   

			Example_SyncUsageWithInfiniteTimeout_AsInSemaphoreSlimDocumentation.Run();
		}
	}
}
