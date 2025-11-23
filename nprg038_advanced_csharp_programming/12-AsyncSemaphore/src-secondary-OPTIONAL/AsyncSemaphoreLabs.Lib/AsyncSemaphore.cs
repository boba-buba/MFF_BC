using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSemaphoreLabs.Lib {
	public class AsyncSemaphore {

		public AsyncSemaphore(int initialCount, int maxCount) {
			throw new NotImplementedException();
		}

		public int CurrentCount => throw new NotImplementedException();

		public Task<bool> WaitAsync(int millisecondsTimeout) {
			throw new NotImplementedException();
		}

		public int Release(int releaseCount) {
			throw new NotImplementedException();
		}

	}
}
