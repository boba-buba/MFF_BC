using System;
using System.Threading.Tasks;

using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using AsyncSemaphoreLabs.Lib;

namespace AsyncSemaphoreLabs.Benchmark {

	public class LockingOverheadTests {
		[Params(2_000_000)]
		public int Files = 2_000_000;   // Pre-assigned so that we can test the class outside of the BenchmarkDotNet infrastructure (can call VerifyCorrectness method).

		public int MaxLength = 128;

		private FakeString[] filenames;
		private int[] counts;

		[GlobalSetup]
		public void PrepareEnvironment() {
			Random r = new Random();

			filenames = new FakeString[Files];
			for (int i = 0; i < filenames.Length; i++) {
				filenames[i] = new FakeString('X', r.Next(MaxLength - 1) + 1);
			}

			counts = new int[MaxLength];

			globalLock = new object();

			globalSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

			globalAsyncSemaphore = new AsyncSemaphore(initialCount: 1, maxCount: 1);

			itemLocks = new object[MaxLength];
			for (int i = 0; i < itemLocks.Length; i++) {
				itemLocks[i] = new object();
			}

			itemSemaphores = new SemaphoreSlim[MaxLength];
			for (int i = 0; i < itemSemaphores.Length; i++) {
				itemSemaphores[i] = new SemaphoreSlim(initialCount: 1, maxCount: 1);
			}

			itemAsyncSemaphores = new AsyncSemaphore[MaxLength];
			for (int i = 0; i < itemSemaphores.Length; i++) {
				itemAsyncSemaphores[i] = new AsyncSemaphore(initialCount: 1, maxCount: 1);
			}
		}

		public void SingleThreaded() {
			Array.Clear(counts, 0, counts.Length);

			foreach (var name in filenames) {
				counts[name.Length]++;
			}
		}

		private object globalLock;

		[Benchmark]
		public void MultiThreadedGlobalLock() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					lock (globalLock) {
						counts[length]++;
					}
				}
			);
		}

		private SemaphoreSlim globalSemaphore;

		[Benchmark]
		public void MultiThreadedGlobalSemaphoreSlim() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					globalSemaphore.Wait();
					counts[length]++;
					globalSemaphore.Release();
				}
			);
		}

		[Benchmark]
		public void MultiThreadedGlobalSemaphoreSlimWaitAsync() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					globalSemaphore.WaitAsync().Wait();
					counts[length]++;
					globalSemaphore.Release();
				}
			);
		}

		private AsyncSemaphore globalAsyncSemaphore;

		[Benchmark]
		public void MultiThreadedGlobalAsyncSemaphore_ASSIGNMENT() {
			Array.Clear(counts, 0, counts.Length);

			var baseTicks = DateTime.Now.Ticks;

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					globalAsyncSemaphore.WaitAsync(Timeout.Infinite).Wait();
					counts[length]++;
					globalAsyncSemaphore.Release(1);
				}
			);
		}

		private object[] itemLocks;

		[Benchmark(Baseline = true)]
		public void MultiThreadedOneLockPerItem() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					lock (itemLocks[length]) {
						counts[length]++;
					}
				}
			);
		}

		private SemaphoreSlim[] itemSemaphores;

		[Benchmark]
		public void MultiThreadedOneSemaphoreSlimPerItem() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					itemSemaphores[length].Wait();
					counts[length]++;
					itemSemaphores[length].Release();
				}
			);
		}

		[Benchmark]
		public void MultiThreadedOneSemaphoreSlimPerItemWaitAsync() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					itemSemaphores[length].WaitAsync().Wait();
					counts[length]++;
					itemSemaphores[length].Release();
				}
			);
		}

		private AsyncSemaphore[] itemAsyncSemaphores;

		[Benchmark]
		public void MultiThreadedOneAsyncSemaphorePerItem_ASSIGNMENT() {
			Array.Clear(counts, 0, counts.Length);

			Parallel.For(0, filenames.Length,
				(int i) => {
					int length = filenames[i].Length;
					itemAsyncSemaphores[length].WaitAsync(Timeout.Infinite).Wait();
					counts[length]++;
					itemAsyncSemaphores[length].Release(1);
				}
			);
		}

		public void VerifyCorrectness() {
			PrepareEnvironment();
			var referenceCounts = counts;
			SingleThreaded();
			counts = new int[referenceCounts.Length];

			MultiThreadedGlobalLock();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedGlobalLock));

			MultiThreadedOneLockPerItem();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedOneLockPerItem));

			MultiThreadedGlobalSemaphoreSlim();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedGlobalSemaphoreSlim));

			MultiThreadedGlobalSemaphoreSlimWaitAsync();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedGlobalSemaphoreSlimWaitAsync));

			MultiThreadedOneSemaphoreSlimPerItem();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedOneSemaphoreSlimPerItem));

			MultiThreadedOneSemaphoreSlimPerItemWaitAsync();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedOneSemaphoreSlimPerItemWaitAsync));

			MultiThreadedGlobalAsyncSemaphore_ASSIGNMENT();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedGlobalAsyncSemaphore_ASSIGNMENT));

			MultiThreadedOneAsyncSemaphorePerItem_ASSIGNMENT();
			CheckIfCountsCorrect(referenceCounts, nameof(MultiThreadedOneAsyncSemaphorePerItem_ASSIGNMENT));
		}

		private void CheckIfCountsCorrect(int[] referenceCounts, string testName) {
			Console.WriteLine(testName);
			for (int i = 0; i < counts.Length; i++) {
				if (counts[i] != referenceCounts[i]) {
					Console.WriteLine($"[{i}] == {counts[i]} should be {referenceCounts[i]}");
					break;
				}
			}
			Console.WriteLine("DONE.");
		}

		// We are using fake strings with just .Length API to save memory so the benchmark can run on a reasonable machine with less than 4 GiB of RAM.
		class FakeString {
			public FakeString(char c, int count) {
				Length = count;
			}

			public int Length { get; }
		}
	}

	class BenchmarkProgram {
		static void Main(string[] args) {
			// Uncomment the following code to test correctness of the implemented algorithms.
			// var tests = new LockingOverheadTests();
			// tests.VerifyCorrectness();

			BenchmarkRunner.Run<LockingOverheadTests>();
		}
	}
}
