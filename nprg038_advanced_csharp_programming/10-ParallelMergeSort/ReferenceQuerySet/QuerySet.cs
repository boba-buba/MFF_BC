using LibraryModel;

// IMPORTANT NOTE: DO NOT LOOK INTO THIS FILE - CLOSE IT NOW !!!
//				   We will explain this solution at the end of the semester!

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace ReferenceQuerySet {
	public class QuerySet {
		public static List<Copy> SingleThreadedQuery(Library library) {
			var q = from c in library.Copies
					where c.State == CopyState.OnLoan && c.Book.Shelf[2] <= 'Q'
					let client = c.OnLoan!.Client
					orderby c.OnLoan!.DueDate, client.LastName, client.FirstName, c.Book.Shelf, c.Id
					select c;

			return q.ToList();
		}

		public static List<Copy> ParallelQuery(Library library) {
			var q = from c in library.Copies.AsParallel()
					where c.State == CopyState.OnLoan && c.Book.Shelf[2] <= 'Q'
					let client = c.OnLoan!.Client
					orderby c.OnLoan!.DueDate, client.LastName, client.FirstName, c.Book.Shelf, c.Id
					select c;

			return q.ToList();
		}
	}
}
