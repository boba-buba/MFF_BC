// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace LibraryModel {

	public class Book {
		public required string Isbn { get; set; }
		public required string Title { get; set; }
		public required string Author { get; set; }
		public required string Shelf { get; set; }
		public required DateTime DatePublished { get; set; }
		public List<Copy> Copies { get; private set; }

		public Book() {
			Copies = new List<Copy>();
		}
	}

	public enum CopyState {
		InShelf, OnLoan, Lost
	};

	public class Copy {
		public required string Id { get; set; }
		public required Book Book { get; set; }
		public required CopyState State { get; set; }
		public Loan? OnLoan { get; set; } = null;
	}

	public class Loan {
		public required Copy Copy { get; set; }
		public required Client Client { get; set; }
		public required DateTime DueDate { get; set; }
	}

	public class Client {
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
	}

	public class Library {
		public List<Book> Books { get; private set; }
		public List<Copy> Copies { get; private set; }
		public List<Loan> Loans { get; private set; }
		public List<Client> Clients { get; private set; }

		public Library() {
			Books = new List<Book>();
			Copies = new List<Copy>();
			Loans = new List<Loan>();
			Clients = new List<Client>();
		}
	}

}
