using My.System.Generic;
namespace DequeProgram
{
    internal class Program
    {
        class X
        {
            public int a;
            public int b;
            public int c;
            public int d;
            public int e;
            public int f;
            public int g;
            public int h;
            public int i;
            public int j;
        }
        const int BatchLength = 1000;
        static void Main(string[] args)
        {
            tInvOpRemove(BatchLength, 0);

        }


        static void tInvOpRemove(int length, int errorAt)
        {
            Deque<X> dx = new Deque<X>();
            for (int i = 0; i < length; i++)
            {
                X x = new X();
                x.a = i;
                x.b = -i;
                x.c = i;
                x.d = -i;
                x.e = i;
                x.f = -i;
                x.g = i;
                x.h = -i;
                x.i = i;
                x.j = -i;
                dx.Add(x);
            }
            for (int i = 0; i < BatchLength; i++)
            {
                X x = new X();
                x.a = i;
                x.b = -i;
                dx.Insert(0, x);
            }

            try
            {
                int j = 0;
                foreach (var t in dx)
                {
                    if (j == errorAt)
                    {
                        dx.RemoveAt(0);
                    }
                    j++;
                }

                Console.WriteLine("BAD");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("OK");
            }
        }


        static void ts1(IList<string> ds)
        {
            ds.Add("a");
            ds.Add("b");
            ds.Add("c");
            ds.Insert(0, "x");
            ds.Insert(0, "y");
            ds.Insert(0, "z");
            ds.Insert(3, null);
            ds.Insert(3, "X");
            ds.Insert(3, null);
            PrintListToConsoleOut(ds);
            Console.WriteLine(ds.IndexOf("a"));
            Console.WriteLine(ds.IndexOf("z"));
            Console.WriteLine(ds.IndexOf("X"));
            Console.WriteLine(ds.IndexOf(null));
        }

        static void ts2(IList<string> ds)
        {
            IList<string> dsr = DequeTest.GetReverseView((Deque<string>)ds);
            Console.Write("Reverse: ");
            PrintListToConsoleOut(dsr);
            Console.WriteLine(dsr.IndexOf("a"));
            Console.WriteLine(dsr.IndexOf("z"));
            Console.WriteLine(dsr.IndexOf("X"));
            Console.WriteLine(dsr.IndexOf(null));

            ds.Clear();
            PrintListToConsoleOut(ds);
            PrintListToConsoleOut(dsr);
        }


        static void tInvOpAdd(int length, int errorAt)
        {
            Deque<X> dx = new Deque<X>();
            for (int i = 0; i < length; i++)
            {
                X x = new X();
                x.a = i;
                x.b = -i;
                x.c = i;
                x.d = -i;
                x.e = i;
                x.f = -i;
                x.g = i;
                x.h = -i;
                x.i = i;
                x.j = -i;
                dx.Add(x);
            }
            for (int i = 0; i < BatchLength; i++)
            {
                X x = new X();
                x.a = i;
                x.b = -i;
                dx.Insert(0, x);
            }

            try
            {
                int j = 0;
                foreach (var t in dx)
                {
                    if (j == errorAt)
                    {
                        dx.Add(new X());
                    }
                    j++;
                }

                Console.WriteLine("BAD");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("OK");
            }
        }

        static void tcopy(IList<int> d)
        {
            int[] a = new int[2 * BatchLength];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (int)0x11BBCCDD;
            }

            d.CopyTo(a, BatchLength / 2);

            for (int i = 0; i < a.Length; i++)
            {
                Console.Write(a[i]);
                Console.Write(' ');
            }
            Console.WriteLine();
        }

        static void t0a(IList<int> d)
        {
            PrintListToConsoleOutEnum(d);
            d.Add(1);
            d.Add(2);
            d.Add(3);
            PrintListToConsoleOutEnum(d);
        }

        static void t0b(IList<int> d)
        {
            PrintListToConsoleOutList(d);
            d.Add(1);
            d.Add(2);
            d.Add(3);
            PrintListToConsoleOutList(d);
        }

        static void t1(IList<int> d)
        {
            PrintListToConsoleOut(d);
            d.Add(1);
            d.Add(2);
            d.Add(3);
            PrintListToConsoleOut(d);
        }

        static void t2X(IList<int> d)
        {
            PrintListToConsoleOut(d);
            for (int i = 0; i < BatchLength; i++)
            {
                d.Add(i);
            }
            PrintListToConsoleOut(d);
        }

        static void t3(IList<int> d)
        {
            d.Insert(0, -1);
            PrintListToConsoleOut(d);
            d.Insert(0, -2);
            PrintListToConsoleOut(d);
            d.Insert(0, -3);
            PrintListToConsoleOut(d);
        }

        static void t4R(IList<int> d)
        {
            Console.Write("Reverse: ");
            PrintListToConsoleOut(DequeTest.GetReverseView((Deque<int>)d));
        }

        static void t5X(IList<int> d)
        {
            for (int i = 0; i < BatchLength; i++)
            {
                d.Insert(0, -i);
            }
            PrintListToConsoleOut(d);
        }

        static void t6X(IList<int> d)
        {
            for (int i = 0; i < BatchLength; i++)
            {
                d.Insert(i, -i);
            }
            PrintListToConsoleOut(d);
        }

        static void t7(IList<int> d)
        {
            d.Remove(1);
            PrintListToConsoleOut(d);
            d.Remove(-3);
            PrintListToConsoleOut(d);
            d.Remove(3);
            PrintListToConsoleOut(d);
        }

        static void t7b(IList<int> d)
        {
            d.RemoveAt(3);
            PrintListToConsoleOut(d);
            d.RemoveAt(0);
            PrintListToConsoleOut(d);
            d.RemoveAt(3);
            PrintListToConsoleOut(d);
        }

        static void t8R(IList<int> d)
        {
            Console.Write("Reverse: ");
            PrintListToConsoleOut(DequeTest.GetReverseView((Deque<int>)d));
        }

        static void t9X(IList<int> d)
        {
            for (int i = 0; i < BatchLength; i += 2)
            {
                d.Remove(i);
            }
        }

        static void t10X(IList<int> d)
        {
            for (int i = BatchLength; i > -BatchLength; i -= 2)
            {
                Console.Write(d.Remove(i));
                Console.Write(' ');
            }
            Console.WriteLine();
        }

        static void t11(IList<int> d)
        {
            d.Insert(2, 11);
            PrintListToConsoleOut(d);
            d.Insert(2, 12);
            PrintListToConsoleOut(d);
            d.Insert(2, 13);
            PrintListToConsoleOut(d);
            d.Insert(2, 14);
            PrintListToConsoleOut(d);
            d.Insert(3, 111);
            PrintListToConsoleOut(d);
            d.Insert(4, 222);
            PrintListToConsoleOut(d);
            d.Insert(5, 333);
            PrintListToConsoleOut(d);
        }

        static void t12(IList<int> d)
        {
            Console.WriteLine(d.IndexOf(1));
            Console.WriteLine(d.IndexOf(-2));
            Console.WriteLine(d.IndexOf(11));
            Console.WriteLine(d.IndexOf(2));
            Console.WriteLine(d.IndexOf(42));
        }

        static void t13C(IList<int> d)
        {
            d.Clear();
            PrintListToConsoleOut(d);
        }

        static void t14X(IList<int> d)
        {
            for (int i = 0; i < BatchLength; i++)
            {
                d.RemoveAt(0);
            }
            PrintListToConsoleOut(d);
        }

        static void t15X(IList<int> d)
        {
            for (int i = 0; i < BatchLength; i++)
            {
                d.RemoveAt(d.Count - 1);
            }
            PrintListToConsoleOut(d);
        }
        static void PrintListToConsoleOutEnum<T>(IList<T> list)
        {
            Console.Write("Count={0} Items=IEnumerable{{", list.Count);
            bool isFirst = true;
            foreach (var t in list)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    Console.Write(' ');
                }
                Console.Write(t);
            }
            Console.WriteLine("}");
        }


        static void PrintListToConsoleOutList<T>(IList<T> list)
        {
            Console.Write("Count={0} Items=IList{{", list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    Console.Write(' ');
                }
                Console.Write(list[i]);
            }
            Console.WriteLine("}");
        }
        static void PrintListToConsoleOut<T>(IList<T> list)
        {
            Console.Write("Count={0} Items=IEnumerable{{", list.Count);
            bool isFirst = true;
            foreach (var t in list)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    Console.Write(' ');
                }
                Console.Write(t);
            }
            Console.Write("} IList{");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    Console.Write(' ');
                }
                Console.Write(list[i]);
            }
            Console.WriteLine("}");
        }

    }
}
