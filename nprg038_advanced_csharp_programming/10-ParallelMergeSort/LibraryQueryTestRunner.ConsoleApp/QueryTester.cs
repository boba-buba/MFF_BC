using System.Diagnostics;

using LibraryModel;

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace LibraryQueryTestRunner.ConsoleApp {

	public delegate List<Copy> QueryDelegate(Library library);

	public delegate void PrintCopyDelegate(TextWriter writer, Copy copy);

	public class QueryTester {
		public static void RunQuery(string queryName, QueryDelegate query, PrintCopyDelegate printCopy, Library library) {
			Console.WriteLine("Executing {0} query ...", queryName);

			Console.WriteLine($"Original copy count: {library.Copies.Count}");
			var swQuery = Stopwatch.StartNew();
			var result = query(library);
			swQuery.Stop();
			Console.WriteLine($"Filtered copy count: {result.Count} (and sorted)");

			Console.WriteLine("Printing query results ...");

			var parts = queryName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var fileName = new System.Text.StringBuilder("Result");
			Array.ForEach(parts, p => {
				fileName.Append(char.ToUpperInvariant(p[0]));
				fileName.Append(p.Substring(1).ToLowerInvariant());
			});
			fileName.Append(".txt");

			Stopwatch? swPrint = null;
			using (var writer = new System.IO.StreamWriter(fileName.ToString())) {
				swPrint = Stopwatch.StartNew();
				foreach (var c in result) {
					printCopy(writer, c);
				}
				swPrint.Stop();
			}

			Console.WriteLine("Query time: {0} s", swQuery.Elapsed.TotalSeconds);
			Console.WriteLine("Print time: {0} s", swPrint.Elapsed.TotalSeconds);
			Console.WriteLine();
		}
	}

}