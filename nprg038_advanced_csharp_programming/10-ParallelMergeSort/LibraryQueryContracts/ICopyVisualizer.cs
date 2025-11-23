using LibraryModel;

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace LibraryQueryContracts {
	public interface ICopyVisualizer {
		public void PrintLoanedCopy(TextWriter writer, Copy copy);
	}
}
