using LibraryModel;
using LibraryQueryContracts;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.CompilerServices;

// NOTE: Run your solution with at least the "Case 1" launch setting. If you have enough physical memory, you should try "Case 2" and ideally "Case 3" and "Case 4" as well.
// NOTE: Run in "Release" configuration! Debug in "Debug" configuration.
// IMPORTANT NOTE: Verify, that all files ResultMergesort????Threads.txt generated for your solution have the same content as ResultReferenceParallel.txt and ResultReferenceSingleThreaded.txt !!!

// IMPORTANT NOTE: Put your solution into this project only!
//				   You can add any code into the MergeSortQuery.MergeSortQuery class, and its ExecuteQuery method.
//				   The MergeSortQuery.MergeSortQuery class has to implement the IParallelQuery interface.
//				   You can also add any additional nested types into MergeSortQuery.MergeSortQuery class if necessary.
//				   You can also add any additional types into this file and project if necessary.

namespace MergeSortQuery {
	public class MergeSortQuery : IParallelQuery {
		public Library? Library { get; set; }
		public int ThreadCount { get; set; }

		private List<Copy> FilteredCopies = new List<Copy>();

		private List<Copy> SortedCopies = new List<Copy>();

		public List<Copy> ExecuteQuery() {
			if (Library is null) throw new InvalidOperationException($"{nameof(Library)} property not set and default null value is not valid.");
			if (ThreadCount == 0) throw new InvalidOperationException($"{nameof(ThreadCount)} property not set and default value 0 is not valid.");

			FilterCopies();

			SortAndMerge();

			return SortedCopies;
		}

		private void FilterCopies()
		{
			var q = from c in Library!.Copies.AsParallel()
					where c.State == CopyState.OnLoan && c.Book.Shelf[2] <= 'Q'
					select c;
			FilteredCopies = q.ToList();
        }

		private void SortAndMerge()
		{
			int ElementsPerChunk = FilteredCopies.Count / ThreadCount;

			List<Thread> threads = new List<Thread>();
			List<Sort> sorts = new List<Sort>();

			for (int i = 0; i < ThreadCount; i++)
			{
				List<Copy> chunk = new List<Copy>();
				int end = ElementsPerChunk * (i + 1);
				if (i == ThreadCount - 1)
				{
					end = FilteredCopies.Count;
				}
                for (int j = i * ElementsPerChunk; j < end; j++)
				{
					chunk.Add(FilteredCopies[j]);
				}

                sorts.Add(new Sort(chunk));
				threads.Add(new Thread(sorts[i].SortEntry));
				threads[i].Start();

			}

			foreach (var thr in threads)
			{
				thr.Join();
			}

			threads.Clear();

			List<Merge> merges = new List<Merge>();
			for (int i = 0; i < sorts.Count; i+=2)
			{
				List<Copy> second;
				if ((i % 2 == 0) && (i == sorts.Count - 1))
				{
					second = new List<Copy>();
                }
				else
				{
					second = sorts[i + 1].sorted;
				}
				merges.Add(new Merge(sorts[i].sorted, second));
				threads.Add(new Thread(merges.Last().MergeLists));
				threads.Last().Start();
			}

            while (merges.Count > 1)
			{
                foreach (var thr in threads)
                {
                    thr.Join();
                }
                List<Copy> first = merges[0].result;
				merges.RemoveAt(0);

                List<Copy> second;
                if (merges.Count == 0)
                {
                    SortedCopies = first;
					return;
                }
                else
                {
                    second = merges[0].result;
					merges.RemoveAt(0);
						
                }

                merges.Add(new Merge(first, second));
                threads.Add(new Thread(merges.Last().MergeLists));
                threads.Last().Start();
            }
			if (merges.Count == 1) { threads.Last().Join(); }
			SortedCopies =  merges.Last().result;
		}

	}

	class Sort
	{
		private List<Copy> copies;
		public List<Copy> sorted { get; private set; }
		public Sort(List<Copy> copies) 
		{
			this.copies = copies;
			sorted = new List<Copy> ();
		}

		public void SortEntry()
		{
			SortList(copies);
		}
        private void SortList(List<Copy> copies)
        {
            var result = from copy in copies
                         orderby
                         copy.OnLoan!.DueDate,
                         copy.OnLoan!.Client.LastName,
                         copy.OnLoan!.Client.FirstName,
                         copy.Book.Shelf,
                         copy.Id
                         select copy;

            sorted = result.ToList();
        }

    }

    class Merge
	{
		private List<Copy> first;
		private List<Copy> second;
		public List<Copy> result { get; private set; }

		public Merge(List<Copy> first, List<Copy> second)
		{
			this.first = first;
			this.second = second;
			result = new List<Copy>();
		}

		public void MergeLists()
		{
			// one list is empty
			if (first.Count == 0)
			{
				this.result.AddRange(second);
				return;
			}
			if (second.Count == 0)
			{
				this.result.AddRange(first);
				return;
			}

			// iteration while one of the lists isnt on end
			int secondItr = 0;
			int firstItr = 0;
			while (firstItr < first.Count && secondItr < second.Count)
			{
				if (first[firstItr].IsGreaterThan(second[secondItr]))
				{
					result.Add(second[secondItr]);
					secondItr++;
				}
				else
				{
					result.Add(first[firstItr]);
					firstItr++;
				}
			}

			// if one list is longer than the other
			while (firstItr < first.Count)
			{
				result.Add(first[firstItr]);
				firstItr++;
			}
			
			while (secondItr < second.Count)
			{
				result.Add(second[secondItr]);
				secondItr++;
			}

		}


	}

	public static class CopyExtension{
		
		public static bool IsGreaterThan(this Copy copy1, Copy copy2)
		{
			if (copy1 == copy2) return false;
			
			//due Date
            if (copy2.OnLoan!.DueDate < copy1.OnLoan!.DueDate)
            {
				return true;
            }
            else if (copy2.OnLoan.DueDate > copy1.OnLoan.DueDate)
            {
                return false;
            }
            // Client Last Name 
			int lastNameComparison = String.Compare(copy1.OnLoan.Client.LastName, copy2.OnLoan.Client.LastName, StringComparison.InvariantCulture);
			if (lastNameComparison < 0) 
			{
				//copy2 is greater than copy1
				return false;
			}
			else if (lastNameComparison > 0)
			{
				return true;
			}
            // Client First Name
            int firstNameComparison = String.Compare(copy1.OnLoan.Client.FirstName, copy2.OnLoan.Client.FirstName, StringComparison.InvariantCulture);
            if (firstNameComparison < 0)
            {
                //copy2 is greater than copy1
                return false;
            }
            else if (firstNameComparison > 0)
            {
                return true;
            }
            // Book Shelf
			int bookShelfComparison = String.Compare(copy1.Book.Shelf, copy2.Book.Shelf, StringComparison.InvariantCulture);
			if (bookShelfComparison < 0)
			{
				return false;
			}
			else if (bookShelfComparison > 0)
			{
				return true;
			}
            // Copy id
			int copyIdComparison = String.Compare(copy1.Id, copy2.Id, StringComparison.InvariantCulture);
			if (copyIdComparison <= 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
