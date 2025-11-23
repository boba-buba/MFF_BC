using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Drawing;
using System.Transactions;


namespace Exam4
{
    interface I1
    {
        int f();
    }
    class A1
    {
        public int f() { return 1; }
    }
    class B1 : A1, I1
    {
        public int f(int x) => x * 2;
    }
    class C1 : B1
    {
        public new int f() { return 3; }
    }

    struct S3
    {
        public int X;

        public void QuadrupleX()
        {
            X *= 4;
        }

    }

    enum DayOfWeek
    {
        Monday, Tuesday, Wednesday, Thursday,
        Friday, Saturday, Sunday
    };


    class A6
    {
        public int X { get; }
        public A6()
        {
            X = f();
        }
        public virtual int f() => 1;
    }
    class B6 : A6
    {
        private int _y = 2;
        public override int f() => _y + 10;
    }
    class C6 : B6
    {
        public new virtual int f() => 3;
    }
    class D6 : C6
    {
        public sealed override int f() => 4;
    }

    class Prg8
    {
        public static void Main4()
        {
            /*I1 i1 = new C1();
            Console.WriteLine(i1.f());
            A1 a1 = (A1)i1;
            Console.WriteLine(a1.f());*/

            /*S3? s = new S3 { X = 10 };
            S3 s2 = new S3 { X = 20 };

            if (s is not null)
            {
                s.Value.QuadrupleX();
                Console.WriteLine(s.Value.X);
            }

            s2.QuadrupleX();
            Console.WriteLine(s2.X);*/

            var d = new D6();
            Console.WriteLine(m1(d));
            Console.WriteLine(d.X);
            Console.WriteLine(((A6)d).f());


            /*
            var items = new List<int>();
            m1(items);
            Console.WriteLine(items.Count);
            m2(ref items);
            Console.WriteLine(items.Count);
            int k = 8;
            Console.WriteLine(Math.Sin(6));*/
        }

        public static int m1(A6 a) => m2((C6)a);
        public static int m2(C6 c) => c.f();
        /*static void m1(List<int> ints)
        {
            ints.Add(6);
        }
        static void m2(ref List<int> ints)
        {
            ints.Add(7);
        }*/
    }
}