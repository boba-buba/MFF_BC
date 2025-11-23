using System;
using System.Threading.Tasks;

namespace TaskOfTaskContinueWith {
	class Program {
		static void Main(string[] args) {

			Console.WriteLine($"Main method running in task ID {Task.CurrentId?.ToString() ?? "NONE"} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
	
			Task<double> t2 = Task.FromResult(3.14);
			Task<Task> t1 = Task.FromResult<Task>(t2);

			Console.WriteLine($"t1 > task ID {t1.Id} is {t1}");
			Console.WriteLine($"t2 > task ID {t2.Id} is {t2}");

			t1.ContinueWith(
				completedTask => {
					Console.WriteLine($"t1 continuation > in task ID {Task.CurrentId} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: completedTask is {completedTask}");
					Console.WriteLine($"	> Did we get t2 Task? {completedTask == t2}");
				}
			);

			Task.Delay(200).Wait();

			t1.ContinueWith(
				(Task completedTask) => {
					Console.WriteLine($"t1 continuation > in task ID {Task.CurrentId} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: completedTask is {completedTask}");
					Console.WriteLine($"	> Did we get t2 Task? {completedTask == t2}");
				}
			);

			Task.Delay(200).Wait();

			t1.ContinueWith(
				(Task<Task> completedTask) => {
					Console.WriteLine($"t1 continuation > in task ID {Task.CurrentId} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: completedTask is {completedTask}");
					// Error: Operator '==' cannot be applied to operands of type 'Task<Task>' and 'Task<double>'
					// Console.WriteLine($"	> Did we get t2 Task? {completedTask == t2}");
				}
			);

			// Error: Parameter 1 is declared as type 'System.Threading.Tasks.Task<System.Threading.Tasks.Task>' but should be 'System.Threading.Tasks.Task'
			//((Task) t1).ContinueWith(
			//	(Task<Task> completedTask) => {
			//		Console.WriteLine($"t1 continuation > in task ID {Task.CurrentId} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: completedTask is {completedTask}");
			//		Console.WriteLine($"	> Did we get t2 Task? {completedTask == t2}");
			//	}
			//);

			Task.Delay(200).Wait();

			t1.ContinueWith(
				(Task<Task> completedTask) => {
					Console.WriteLine($"t1 continuation > in task ID {Task.CurrentId} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: completedTask is {completedTask}");
					Console.WriteLine($"	> Did we get t2 Task? {completedTask.Result == t2}");
				}
			);

			Task.Delay(200).Wait();

			t1.ContinueWith(
				completedTask => {
					Console.WriteLine($"t1 continuation > in task ID {Task.CurrentId} @ thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: completedTask is {completedTask}");
					Console.WriteLine($"	> Did we get t2 Task? {completedTask.Result == t2}");
				}
			);
		
		}

		// Possible example output:
		//		Main method running in task ID NONE @ thread 1
		//		t1 > task ID 1 is System.Threading.Tasks.Task`1[System.Threading.Tasks.Task]
		//		t2 > task ID 2 is System.Threading.Tasks.Task`1[System.Double]
		//		t1 continuation > in task ID 3 @ thread 4: completedTask is System.Threading.Tasks.Task`1[System.Threading.Tasks.Task]
		//		        > Did we get t2 Task? False
		//		t1 continuation > in task ID 4 @ thread 4: completedTask is System.Threading.Tasks.Task`1[System.Threading.Tasks.Task]
		//		        > Did we get t2 Task? False
		//		t1 continuation > in task ID 5 @ thread 6: completedTask is System.Threading.Tasks.Task`1[System.Threading.Tasks.Task]
		//		t1 continuation > in task ID 6 @ thread 6: completedTask is System.Threading.Tasks.Task`1[System.Threading.Tasks.Task]
		//		        > Did we get t2 Task? True
		//		t1 continuation > in task ID 7 @ thread 6: completedTask is System.Threading.Tasks.Task`1[System.Threading.Tasks.Task]
		//		        > Did we get t2 Task? True
	}
}
