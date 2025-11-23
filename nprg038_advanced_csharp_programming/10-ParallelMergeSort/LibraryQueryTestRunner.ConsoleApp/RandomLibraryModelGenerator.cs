using System.Text;

using LibraryModel;

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace LibraryQueryTestRunner.ConsoleApp {
	public class RandomLibraryModelGenerator {

		public int BookCount { get; set; }
		public int MaxCopyCount { get; set; }
		public int LoanCount { get; set; }
		public int ClientCount { get; set; }
		public int RandomSeed { get; set; }

		private Random? randomInstance = null;

		private Random _random {
			get {
				if (randomInstance == null) {
					randomInstance = new Random(RandomSeed);
				}
				return randomInstance;
			}
		}

		private string GetRandomNumberString(int numberCount) {
			var sb = new StringBuilder(numberCount);

			for (int i = 0; i < numberCount; i++) {
				sb.Append((char) ('0' + _random.Next(10)));
			}

			return sb.ToString();
		}

		private string GetRandomEnglishUpperString(int letterCount) {
			var sb = new StringBuilder(letterCount);

			for (int i = 0; i < letterCount; i++) {
				sb.Append((char) ('A' + _random.Next('Z' - 'A' + 1)));
			}

			return sb.ToString();
		}

		private string GetRandomName(int length) {
			var sb = new StringBuilder(length);

			int type0LastIdx = -1;
			int type1LastIdx = -1;

			sb.Append((char) ('A' + _random.Next('Z' - 'A' + 1)));
			for (int i = 1; i < length; i++) {
				var type = _random.Next(6);
				if (type == 0 && i != type0LastIdx) {
					sb.Append((char) (0x300 + _random.Next(0x0A)));
					type0LastIdx = i;
				} else if (type == 1 && i != type1LastIdx) {
					sb.Append((char) (0x326 + _random.Next(0x32B - 0x326 + 1)));
					type1LastIdx = i;
				} else {
					sb.Append((char) ('a' + _random.Next('z' - 'a' + 1)));
				}
			}

			return sb.ToString();
		}

		private readonly string[] _authorNames = new string[1000];
		private readonly string[] _clientFirstNames = new string[100];
		private readonly string[] _clientLastNames = new string[1000];
		private string[] _shelfNumbers = Array.Empty<string>();

		public void Initialize() {
			for (int i = 0; i < _authorNames.Length; i++) {
				_authorNames[i] = GetRandomName(5) + " " + GetRandomName(10);
			}

			for (int i = 0; i < _clientFirstNames.Length; i++) {
				_clientFirstNames[i] = GetRandomName(7);
			}

			for (int i = 0; i < _clientLastNames.Length; i++) {
				_clientLastNames[i] += GetRandomName(12);
			}

			Span<char> chars = stackalloc char[3];
			_shelfNumbers = new string[10 * 10 * ('T' - 'A' + 1)];
			int index = 0;
			for (char a = '0'; a <= '9'; a++) {
				for (char b = '0'; b <= '9'; b++) {
					for (char c = 'A'; c <= 'T'; c++) {
						chars[0] = a;
						chars[1] = b;
						chars[2] = c;
						_shelfNumbers[index] = new string(chars);
						index++;
					}
				}
			}
		}

		public void FillLibrary(Library library) {
			for (int i = 0; i < BookCount; i++) {
				var b = new Book {
					Author = _authorNames[_random.Next(_authorNames.Length)],

					// Original: 30 = more realistic
					Title = GetRandomName(10),

					// Original: more realistic:
					// var l1 = _random.Next(7) + 2;
					// var l2 = _random.Next(l1 - 1) + 1;
					// Isbn = GetRandomNumberString(l2) + "-" + GetRandomNumberString(9 - l1) + "-" + GetRandomNumberString(l1 - l2) + "-" + GetRandomNumberString(1),
					Isbn = GetRandomNumberString(8),

					DatePublished = new DateTime(1950 + _random.Next(50), 1, 1),

					// Original:
					// Shelf = GetRandomNumberString(2) + GetRandomEnglishUpperString(1)
					Shelf = _shelfNumbers[_random.Next(_shelfNumbers.Length)]
				};

				int copies = _random.Next(MaxCopyCount) + 1;
				for (int j = 0; j < copies; j++) {
					var c = new Copy {
						Book = b,
						Id = GetRandomNumberString(2) + GetRandomEnglishUpperString(2) + GetRandomNumberString(4),
						State = CopyState.InShelf
					};

					b.Copies.Add(c);
					library.Copies.Add(c);
				}

				library.Books.Add(b);
			}

			for (int i = 0; i < ClientCount; i++) {
				var c = new Client {
					FirstName = _clientFirstNames[_random.Next(_clientFirstNames.Length)],
					LastName = _clientLastNames[_random.Next(_clientLastNames.Length)]
				};
				library.Clients.Add(c);
			}

			var today = DateTime.Today;
			for (int i = 0; i < LoanCount; i++) {
				var copy = library.Copies[_random.Next(library.Copies.Count)];
				if (copy.State == CopyState.OnLoan) continue;

				var l = new Loan {
					Client = library.Clients[_random.Next(ClientCount)],
					Copy = copy,
					DueDate = today.AddDays(_random.Next(31))
				};

				l.Copy.OnLoan = l;
				l.Copy.State = CopyState.OnLoan;

				library.Loans.Add(l);
			}
		}

	}
}
