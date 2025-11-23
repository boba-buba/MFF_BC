using LibraryModel;

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace LibraryQueryContracts {
	public interface IParallelQuery {
		public Library Library { set; } 
		public int ThreadCount { set; }

		public List<Copy> ExecuteQuery();
	}
}
