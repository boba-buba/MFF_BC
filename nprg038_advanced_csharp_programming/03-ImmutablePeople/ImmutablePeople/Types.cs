using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace ImmutablePeople {

	// IMPORTANT NOTE 1: It is strictly forbidden to use "records" (i.e. record classes or record structs) to implement Person, Student, Teacher or other types !!!


    public interface IPerson
	{
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Password { get; init; }

        public void AddWithNewPasswordTo<T>(List<T> people, string password) where T: IPerson;
    }

	public abstract class Person : IPerson
    {
		public string FirstName { get; init; }
		public string LastName { get; init; }
		public string Password { get; init; }
		
		public abstract Person WithName(string name);
		public abstract Person WithPassword(string password);

		public override string ToString() => $" {FirstName} {LastName} has password \"{Password}\"";

        public abstract void AddWithNewPasswordTo<T>(List<T> people, string password) where T : IPerson;
    }

    public class Student : Person
	{
		public static Student Default { get; } = new Student();
		public DateOnly DateEnrolled { get; init; }
		private Student() { }
		private Student(string firstName, string lastName, string password, DateOnly dateEnrolled)
		{
			FirstName = firstName;
			LastName = lastName;
			Password = password;
			DateEnrolled = dateEnrolled;
		}

        public Student WithDateEnrolled(DateOnly dateEnrolled) => new Student(FirstName, LastName, Password, dateEnrolled);

		public override Student WithName(string name) 
		{
			string firstName = FirstName;
			string lastName = LastName;
			var names = name.Split(' ');
			if (names.Length == 0 ) { throw new ArgumentException(); }
			firstName = names[0];
			if (names.Length == 2)
			{
				lastName = names[1];
			}
			return new Student(firstName, lastName, Password, DateEnrolled);
		}

        public override Student WithPassword(string password) => new Student(FirstName, LastName, password, DateEnrolled);

        public override string ToString() => this.GetType().Name + base.ToString();

        public override void AddWithNewPasswordTo<T>(List<T> people, string password)
        {
            var st = new Student(FirstName, LastName, password, DateEnrolled);
            people.Add((T)(IPerson)st);
        }
    }

	public class Teacher : Person
	{
		public static Teacher Default { get; } = new Teacher();
		public int CourcesHeld { get; init; }
		private Teacher() { }
		private Teacher(string firstName, string lastName, string password, int courcesHeld)
		{
			FirstName = firstName;
			LastName = lastName;
			Password = password;
			CourcesHeld = courcesHeld;
		}
		public Teacher WithCoursesHeld(int coursesHeld) => new Teacher(FirstName, LastName, Password, coursesHeld);

        public override Teacher WithName(string name)
        {
            string firstName = FirstName;
            string lastName = LastName;
            var names = name.Split(' ');
            if (names.Length == 0) { throw new ArgumentException(); }
            firstName = names[0];
            if (names.Length == 2)
            {
                lastName = names[1];
            }
            return new Teacher(firstName, lastName, Password, CourcesHeld);
        }
        public override Teacher WithPassword(string password) => new Teacher(FirstName, LastName, password, CourcesHeld);
        public override string ToString() => this.GetType().Name + base.ToString();
        public override void AddWithNewPasswordTo<T>(List<T> people, string password)
        {
            var t = new Teacher(FirstName, LastName, password, CourcesHeld);
            people.Add((T)(IPerson)t);
        }
    }

	public static class Extensions
	{
        public static void PrintAll<T>(this IReadOnlyList<T> people) where T: IPerson
		{
			foreach (var person in people)
			{
				Console.WriteLine(person);
			}
		}

		public static List<T> WithPasswordResetByFirstName<T>(this IReadOnlyList<T> people, string firstName, string newPassword) where T : IPerson
        {
            List<T> result = new List<T>();
            foreach (var person in people)
            {
                if (person.FirstName == firstName)
                {
                    person.AddWithNewPasswordTo(result, newPassword);
                }
                else
                {
                    result.Add(person);
                }
            }
            return result;
        }

    }

}