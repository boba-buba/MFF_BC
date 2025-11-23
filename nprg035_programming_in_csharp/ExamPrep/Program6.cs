using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Transactions;


namespace Exam6
{
    abstract class Animal
    {
        protected bool[] FeedDiary;

    }
    
    class Chicken : Animal {
    
        public Chicken() {
            FeedDiary = new bool[6];
        }
    }  

    interface I1
    {
        public char f();
    }
    
    class A1 : I1 
    {
        public virtual char f() => 'A';
    }

    class B1 : A1
    {
        public override char f() => 'B';
    }
    class C1: B1
    {
        public virtual char f() => (char) (base.f() + 10); 
    }

    class D1: C1, I1 {}

    class E1 : D1
    {
        public new char f() => (char)(base.f() + 5);
    }

    interface I5
    {
        public int X { get; set; }
    }

    struct S(int a, int b, int c) : I5 {
        public int X { get; set; } = a;
        public int Y { get; } = a;
        public int f()
        {
            return a + b + c;
        }
    }

    class Prg8
    {
        private static void Update(ref I5 i)
        {
            i.X = 10;
        }
        public static void Main()
        {
            A1? a1 = new E1();
            Console.WriteLine(a1.f());
            A1 b1 = new A1();
            Console.WriteLine(b1 is B1);

            I1? i1 = a1;
            Console.WriteLine(i1.f());
            
            Console.WriteLine(a1 is B1);
            a1 = null;
            Console.WriteLine(a1 is A1);
            E e1 = E.One;

            Console.WriteLine(e1);
            e1++;
            Console.WriteLine(e1);

            S s1 = new S(11, 12, 13);
            I5 ii = (I5)s1;
            Update(ref ii);
            Console.WriteLine(ii.X);

            S[] sa = new S[3];
            Console.WriteLine(sa[0].X);
        }
        enum E { One = 1, Four = 4 };

       
    }
}