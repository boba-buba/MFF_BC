using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Exam;
using System.Transactions;

namespace Exam2
{
    interface I1 { string f(); }
    interface I2 { string? f(); }
    class A : I1, I2
    {
        public string f() => "Hello";
        public string g => "Hello";
    }
    class B : I1, I2
    {
        public string? f() => null;
    }
    interface J1 { int f(); }
    interface J2 { int? f(); }
    /*class JA : J1, J2
    {
        public int f() => 42;
    }*/

    /// <summary>
    /// cast C
    /// </summary>
    interface I6
    {
        public char f();
    }
    class A6 : I6
    {
        public virtual char f() => 'A';
    }
    class B6 : A6
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
        (char)(((A6)this).f() + 4);
    }

    class X1
    {
        public int Value
        {
            get
            {
                Console.WriteLine("X");
                return 10;
            }
        }
    }

    public class Program
    {
        static void Main2(string[] args)
        {
            /*
            A a = new A();
            Console.WriteLine(a.f());
            Console.WriteLine(a.g);*/
            /*
            int x = 3;
            object o = x;
            var y = (long)o;
            Console.WriteLine(y);*/

            /*
            C6? c6 = new D6();
            Console.WriteLine(c6.f()); 
            I6 i6 = new C6();
            Console.WriteLine(i6.f());
            Console.WriteLine(c6 is C6);
            Console.WriteLine(c6.GetType() == typeof(C6));
            */

            try
            {
                try
                {
                    throw new ArgumentException();
                }
                finally
                {
                    Console.WriteLine("A");
                    try
                    {
                        try
                        {
                            throw new IndexOutOfRangeException();
                        }
                        finally
                        {
                            Console.WriteLine("B");
                            try
                            {
                                try
                                {
                                    throw new NotSupportedException();
                                }
                                finally
                                {
                                    Console.WriteLine("C");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{ex.GetType()}");
                                throw;
                            }
                            Console.WriteLine("D");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                        $"{ex.GetType()}"
                        );
                    }
                    Console.WriteLine("E");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}");
            }

            /*var x1 = new X1();
            if (x1.Value > 5 && x1.Value < 15 && x1.Value != 8)
            {
                Console.WriteLine("OK1");
            }
            if (x1.Value is > 5 and < 15 and not 8)
            {
                Console.WriteLine("OK2");
            }*/



        }
    }
}