using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_LinqToObjects {

    class Program {
        void PrintEnumerable<T>(IEnumerable<T> enumerable)
        {
			foreach (var item in enumerable)
				Console
        }

        static void Main(string[] args) {
			Console.WriteLine("Press ENTER to run without debug prints,");
			Console.WriteLine("Press D1 + ENTER to enable some debug prints,");
			Console.Write("Press D2 + ENTER to enable all debug prints: ");
			string command = Console.ReadLine().ToUpper();
			DebugPrints1 = command == "D2" || command == "D1" || command == "D";
			DebugPrints2 = command == "D2";
			Console.WriteLine();

			var groupA = new Group();

			HighlightedWriteLine("Assignment 1: All persons, that consider no one as their friend.");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 2: All persons, sorted alphabeticaly by name, older than 15, and only those whose name starts with letter D or higher.");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 3: All persons, that are oldest in whole group and whose name starts with letter T or higher (U, V, W, X, Y, Z).");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 4: All persons, that are older than everyone who they consider as their friend.");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 5: All persons, that have no friends (they don't consider anyone as their frend and nobody considers them as their friend).");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 6: All persons, that are someone's oldest friend (with repeats).");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 6B: All persons, that are someone's oldest friend (without repeats).");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 7: All persons, who are oldest among someone's friends, but only those who are younger than that specific someone (with repeats).");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 7B: All persons, who are oldest among someone's friends, but only those who are younger than that specific someone (without repeats).");

			Console.WriteLine();
			HighlightedWriteLine("Assignment 7C: All persons, who are oldest among someone's friends, but only those who are younger than that specific someone (without repeats and sorted alphabeticaly in reverse by name ).");
		}

		public static void HighlightedWriteLine(string s) {
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(s);
			Console.ForegroundColor = oldColor;
		}

		public static bool DebugPrints1 = false;
		public static bool DebugPrints2 = false;

		class Person {
			public string Name { get; set; }
			public int Age { get; set; }
			public IEnumerable<Person> Friends { get; private set; }

			/// <summary>
			/// DO NOT USE in your LINQ queries!!!
			/// </summary>
			public IList<Person> FriendsListInternal { get; private set; }

			class EnumWrapper<T> : IEnumerable<T> {
				IEnumerable<T> innerEnumerable;
				Person person;
				string propName;

				public EnumWrapper(Person person, string propName, IEnumerable<T> innerEnumerable) {
					this.person = person;
					this.propName = propName;
					this.innerEnumerable = innerEnumerable;
				}

				public IEnumerator<T> GetEnumerator() {
					if (Program.DebugPrints1) Console.WriteLine(" # Person(\"{0}\").{1} is being enumerated.", person.Name, propName);

					foreach (var value in innerEnumerable) {
						yield return value;
					}

					if (Program.DebugPrints2) Console.WriteLine(" # All elements of Person(\"{0}\").{1} have been enumerated.", person.Name, propName);
				}

				System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
					return GetEnumerator();
				}
			}

			public Person() {
				FriendsListInternal = new List<Person>();
				Friends = new EnumWrapper<Person>(this, "Friends", FriendsListInternal);
			}

			public override string ToString() {
				return string.Format("Person(Name = \"{0}\", Age = {1})", Name, Age);
			}
		}

		class Group : IEnumerable<Person> {
			Person anna, blazena, ursula, daniela, emil, vendula, cyril, frantisek, hubert, gertruda;

			public Group() {
				anna = new Person { Name = "Anna", Age = 22 };
				blazena = new Person { Name = "Blazena", Age = 18 };
				ursula = new Person { Name = "Ursula", Age = 22, FriendsListInternal = { blazena } };
				daniela = new Person { Name = "Daniela", Age = 18, FriendsListInternal = { ursula } };
				emil = new Person { Name = "Emil", Age = 21 };
				vendula = new Person { Name = "Vendula", Age = 22, FriendsListInternal = { blazena, emil } };
				cyril = new Person { Name = "Cyril", Age = 21, FriendsListInternal = { daniela } };
				frantisek = new Person { Name = "Frantisek", Age = 15, FriendsListInternal = { anna, blazena, cyril, daniela, emil } };
				hubert = new Person { Name = "Hubert", Age = 10 };
				gertruda = new Person { Name = "Gertruda", Age = 10, FriendsListInternal = { frantisek } };

				blazena.FriendsListInternal.Add(ursula);
				blazena.FriendsListInternal.Add(vendula);
				ursula.FriendsListInternal.Add(daniela);
				daniela.FriendsListInternal.Add(cyril);
				emil.FriendsListInternal.Add(vendula);
			}

			public IEnumerator<Person> GetEnumerator() {
				if (Program.DebugPrints1) Console.WriteLine("*** Group is being enumerated.");

				yield return hubert;
				yield return anna;
				yield return frantisek;
				yield return blazena;
				yield return ursula;
				yield return daniela;
				yield return emil;
				yield return vendula;
				yield return cyril;
				yield return gertruda;

				if (Program.DebugPrints1) Console.WriteLine("*** All elements of Group have been enumerated.");
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}
		}
	}
}
