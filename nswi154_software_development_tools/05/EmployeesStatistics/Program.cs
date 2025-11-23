using System;
using System.Collections.Generic;
using System.Linq;

namespace Stats {
    public class Employees {
        public int add(string name, int salary) // returns ID
        {
            return 0;
        }
        public HashSet<int> getAll() // returns a set of IDs
        {
            return new HashSet<int>();
        }
        public string getName(int id)
        {
            return "Name";
        }
        public int getSalary(int id)
        {
            return 0;
        }
        public void changeSalary(int id, int newSalary){}
    }
    public class Statistics {
        Employees employees;
        public Statistics(Employees employees) {
            this.employees = employees;
        }
        public int computeAverageSalary() {
            var ids = employees.getAll();
            int count = ids.Count;
            if (count == 0) return 0;
            int sumSalary = 0;
            foreach (var id in ids) {
                sumSalary += employees.getSalary(id);
            }
            return sumSalary / count;
        }
        public int getMinSalary() {
            var ids = employees.getAll();
            if (ids.Count == 0) return 0;
            int minSalary = employees.getSalary(ids.First());
            foreach (var id in ids) {
                var sal = employees.getSalary(id);
                if (minSalary > sal)
                    minSalary = sal;
            }
            return minSalary;
        }
        public void printSalariesByName() // prints the list of pairs <name, salary> that is sorted by employee names
        {
            var ids = employees.getAll();
            Dictionary<int, string> staff = new Dictionary<int, string>();
            foreach (var id in ids) {
                staff.Add(id, employees.getName(id));
            }
            foreach (var pair in staff.OrderBy(pair => pair.Value)) {
                Console.WriteLine(pair.Value + " " + employees.getSalary(pair.Key));
            }
            //Console.WriteLine("end");
        }

    }
	class Program {
		static void Main(string[] args) {

		}
	}
}
