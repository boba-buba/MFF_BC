using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Drawing;
using System.Transactions;


namespace Exam5
{
    class A1
    {
        public int f() => 1;
    }
    class B1 : A1
    {
        public new int f() => 2;
    }

    interface I6
    {
        public char f();
    }
    class A6
    {
        public virtual char f() => 'A';
    }
    class B6 : A6, I6
    {
        public override char f() => 'B';
    }
    class C6 : B6
    {
        public virtual char f() => 'C';
    }
    class D6 : C6
    {
        public override char f() =>
        (char)(base.f() + 1);
    }
    

    enum StudyProgram
    {
        Mathematics, Physics, ComputerScience
    }
    class Person
    {
        public string Name { get; init; }
        public StudyProgram Program { get; init; }

        public void Deconstruct(out string name, out bool isComputerScientist)
        {
            name = Name;
            isComputerScientist = this.Program is StudyProgram.ComputerScience;
        }
    }
    class Prg8
    {
        public static void Main5()
        {
            var jan = new Person
            {
                Name = "Jan",
                Program = StudyProgram.Physics
            };
            PrintInfo(jan);
            PrintInfo("Pavel");

            //A1 a1 = new B1();
            //Console.WriteLine(a1.f());

            /*C6 c6 = new D6();
            Console.WriteLine(c6.f());
            I6 i6 = new C6();
            Console.WriteLine(i6.f());*/
        }
        private static void PrintInfo(object personOrName)
        {
            var info = personOrName switch
            {
                Person(var name, isComputerScientist: true) => $"nerd {name}",
                Person(var name, isComputerScientist: false) => $"student {name}",
                string name => name
            };
            Console.WriteLine(info);
        }
    }
}