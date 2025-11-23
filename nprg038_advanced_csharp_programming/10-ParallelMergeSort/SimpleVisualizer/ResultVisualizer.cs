using LibraryModel;
using LibraryQueryContracts;

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace SimpleVisualizer {
	public class ResultVisualizer : ICopyVisualizer {
		public void PrintLoanedCopy(TextWriter writer, Copy c) {
			writer.WriteLine(
				"{0} {1}: {2} loaned to {3}, {4}.",
				c.OnLoan!.DueDate.ToShortDateString(),
				c.Book.Shelf,
				c.Id,
				c.OnLoan.Client.LastName,
				System.Globalization.StringInfo.GetNextTextElement(c.OnLoan.Client.FirstName)
			);
		}
	}
}
