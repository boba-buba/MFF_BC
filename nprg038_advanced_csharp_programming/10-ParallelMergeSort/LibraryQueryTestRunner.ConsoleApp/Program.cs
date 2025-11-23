using LibraryModel;
using LibraryQueryContracts;

// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into MergeSortQuery project only.

namespace LibraryQueryTestRunner.ConsoleApp {

	class Program {
		static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("Missing command line argument - run with case number (1 to 4).");
				return;
			}

			Library library = new Library();

			var generator = args[0] switch {
				// Requires ~ 800 MB of RAM
				"1" => new RandomLibraryModelGenerator { RandomSeed = 42, BookCount = 2_500_000 / 4, ClientCount = 10_000, LoanCount = 500_000 * 5, MaxCopyCount = 10 },

				// Requires ~ 1500 MB of RAM
				"2" => new RandomLibraryModelGenerator { RandomSeed = 42, BookCount = 2_500_000 / 2, ClientCount = 10_000, LoanCount = 500_000 * 5, MaxCopyCount = 10 },

				// Requires ~ 2800 MB of RAM
				"3" => new RandomLibraryModelGenerator { RandomSeed = 42, BookCount = 2_500_000 / 2, ClientCount = 10_000, LoanCount = 500_000 * 20, MaxCopyCount = 10 },

				// Requires ~ 5000 MB of RAM	
				"4" => new RandomLibraryModelGenerator { RandomSeed = 42, BookCount = 2_500_000 * 2, ClientCount = 10_000, LoanCount = 500_000 * 10, MaxCopyCount = 10 },

				_ => null
			};

			if (generator is null) {
				Console.WriteLine($"Wrong command line argument '{args[0]}' - run with case number (1 to 4).");
				return;
			}

			Console.WriteLine("Generating library content ...");
			generator.Initialize();
			generator.FillLibrary(library);

            // Warm up run:
            Console.WriteLine("Warm up run starts ...");
            _ = ReferenceQuerySet.QuerySet.SingleThreadedQuery(library);
			Console.WriteLine("Warm up run ended ...");

			QueryTester.RunQuery("reference single threaded", ReferenceQuerySet.QuerySet.SingleThreadedQuery, new SimpleVisualizer.ResultVisualizer().PrintLoanedCopy, library);

			QueryTester.RunQuery("reference parallel", ReferenceQuerySet.QuerySet.ParallelQuery, new SimpleVisualizer.ResultVisualizer().PrintLoanedCopy, library);

			for (int threads = 1; threads <= 8; threads++) {
				RunParallelQuery(library, threads);
			}

			RunParallelQuery(library, 16);
			RunParallelQuery(library, 32);
			RunParallelQuery(library, 128);
			RunParallelQuery(library, 1024);
			RunParallelQuery(library, 8192);
		}

		static void RunParallelQuery(Library library, int threads) {
			QueryTester.RunQuery(
				string.Format("MergeSort {0:D4} threads", threads),

				testInstanceLibrary => {
					var query = (IParallelQuery) new MergeSortQuery.MergeSortQuery();
					// var query = (IParallelQuery) new StudentSolution1.MergeSortQuery();
					// var query = (IParallelQuery) new MergeSortQuery_COMPLETED.MergeSortQuery();
					query.Library = testInstanceLibrary;
					query.ThreadCount = threads;
					return query.ExecuteQuery();
				},

				new SimpleVisualizer.ResultVisualizer().PrintLoanedCopy,

				library
			);
		}
	}
}
