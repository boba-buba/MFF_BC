namespace Deque.IntegrationTests.UglyReCodEx {

	public class DequeIntegrationTests {

		const int BatchLength = 1000;

		[Theory]
		[InlineData("01")]
		[InlineData("02")]
		[InlineData("03")]
		[InlineData("04")]
		[InlineData("05")]
		[InlineData("06")]
		[InlineData("07")]
		[InlineData("08")]
		[InlineData("09")]
		[InlineData("10")]
		[InlineData("11")]
		[InlineData("12")]
		[InlineData("13")]
		[InlineData("14")]
		[InlineData("15")]
		[InlineData("16")]
		[InlineData("17")]
		[InlineData("18")]
		[InlineData("19")]
		[InlineData("20")]
		[InlineData("21")]
		[InlineData("22")]
		[InlineData("23")]
		[InlineData("24")]
		[InlineData("25")]
		[InlineData("26")]
		[InlineData("27")]
		[InlineData("28")]
		[InlineData("29")]
		[InlineData("30")]
		[InlineData("31")]
		[InlineData("32")]
		[InlineData("33")]
		public void ReCodExTest(string testId) {
			using var outputWriter = new StringWriter();
			outputWriter.NewLine = "\n";

			var originalConsoleOut = Console.Out;
			Console.SetOut(outputWriter);

			IList<int> d;
			IList<string> ds;

			switch (testId) {
				case "01":
					d = new Deque<int>();
					t0a(d);
					break;

				case "02":
					d = new Deque<int>();
					t0b(d);
					break;

				case "03":
					d = new Deque<int>();
					t1(d);
					t3(d);
					break;

				case "04":
					d = new Deque<int>();
					t1(d);
					t3(d);
					t7(d);
					break;

				case "05":
					d = new Deque<int>();
					t1(d);
					t3(d);
					t7(d);
					t11(d);
					break;

				case "06":
					d = new Deque<int>();
					t1(d);
					t3(d);
					t7b(d);
					t11(d);
					t12(d);
					break;

				case "07":
					d = new Deque<int>();
					t1(d);
					t3(d);
					t7(d);
					t11(d);
					t12(d);
					break;

				case "08":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t7(d);
					t11(d);
					t12(d);
					break;

				case "09":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t7(d);
					t11(d);
					t12(d);
					break;

				case "10":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t6X(d);
					t7(d);
					t11(d);
					t12(d);
					break;

				case "11":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t6X(d);
					t7(d);
					t11(d);
					t12(d);
					break;

				case "12":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t7(d);
					t9X(d);
					t11(d);
					t12(d);
					break;

				case "13":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t7(d);
					t9X(d);
					t10X(d);
					t11(d);
					t12(d);
					break;

				case "14":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t7(d);
					t9X(d);
					t2X(d);
					t10X(d);
					t11(d);
					t12(d);
					break;

				case "15":
					d = new Deque<int>();
					t1(d);
					t3(d);
					t4R(d);
					break;

				case "16":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t7(d);
					t8R(d);
					t9X(d);
					t2X(d);
					t10X(d);
					t11(d);
					t12(d);
					break;

				case "17":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t3(d);
					t5X(d);
					t7(d);
					t8R(d);
					t9X(d);
					t2X(d);
					t10X(d);
					t8R(d);
					t11(d);
					t12(d);
					break;

				case "18":
					ds = new Deque<string>();
					ts1(ds);
					break;

				case "19":
					ds = new Deque<string>();
					ts1(ds);
					ts2(ds);
					break;

				case "20":
					tc();
					break;

				case "21":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t15X(d);
					break;

				case "22":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t14X(d);
					t2X(d);
					t15X(d);
					break;

				case "23":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t14X(d);
					t5X(d);
					t15X(d);
					t5X(d);
					t14X(d);
					break;

				case "24":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t14X(d);
					t5X(d);
					t15X(d);
					t5X(d);
					tcopy(d);
					break;

				case "25":
					d = new Deque<int>();
					t1(d);
					t2X(d);
					t14X(d);
					t5X(d);
					t15X(d);
					t5X(d);
					IList<int> dr = DequeTest.GetReverseView((Deque<int>) d);
					t1(dr);
					t2X(dr);
					t14X(dr);
					t5X(dr);
					t15X(dr);
					t5X(dr);
					t15X(dr);
					break;

				case "26":
					d = new Deque<int>();
					tlong1(d);
					break;

				case "27":
					d = new Deque<int>();
					tlong2(d);
					break;

				case "28":
					tInvOpAdd(BatchLength, 0);
					break;

				case "29":
					tInvOpAdd(BatchLength, BatchLength / 2);
					break;

				case "30":
					tInvOpAdd(BatchLength, BatchLength - 1);
					break;

				case "31":
					tInvOpRemove(BatchLength, 0);
					break;

				case "32":
					tInvOpRemove(BatchLength, BatchLength / 2);
					break;

				case "33":
					tInvOpRemove(BatchLength, BatchLength - 1);
					break;
			}

			Console.SetOut(originalConsoleOut);

			var expectedOutput = File.ReadAllText(Path.Combine("ExpectedTestOutputs", testId + ".out"));
			var actualOutput = outputWriter.ToString();

			Assert.Equal(expectedOutput, actualOutput);
		}

		static void t0a(IList<int> d) {
			PrintListToConsoleOutEnum(d);
			d.Add(1);
			d.Add(2);
			d.Add(3);
			PrintListToConsoleOutEnum(d);
		}

		static void t0b(IList<int> d) {
			PrintListToConsoleOutList(d);
			d.Add(1);
			d.Add(2);
			d.Add(3);
			PrintListToConsoleOutList(d);
		}

		static void t1(IList<int> d) {
			PrintListToConsoleOut(d);
			d.Add(1);
			d.Add(2);
			d.Add(3);
			PrintListToConsoleOut(d);
		}

		static void t2X(IList<int> d) {
			PrintListToConsoleOut(d);
			for (int i = 0; i < BatchLength; i++) {
				d.Add(i);
			}
			PrintListToConsoleOut(d);
		}

		static void t3(IList<int> d) {
			d.Insert(0, -1);
			PrintListToConsoleOut(d);
			d.Insert(0, -2);
			PrintListToConsoleOut(d);
			d.Insert(0, -3);
			PrintListToConsoleOut(d);
		}

		static void t4R(IList<int> d) {
			Console.Write("Reverse: ");
			PrintListToConsoleOut(DequeTest.GetReverseView((Deque<int>) d));
		}

		static void t5X(IList<int> d) {
			for (int i = 0; i < BatchLength; i++) {
				d.Insert(0, -i);
			}
			PrintListToConsoleOut(d);
		}

		static void t6X(IList<int> d) {
			for (int i = 0; i < BatchLength; i++) {
				d.Insert(i, -i);
			}
			PrintListToConsoleOut(d);
		}

		static void t7(IList<int> d) {
			d.Remove(1);
			PrintListToConsoleOut(d);
			d.Remove(-3);
			PrintListToConsoleOut(d);
			d.Remove(3);
			PrintListToConsoleOut(d);
		}

		static void t7b(IList<int> d) {
			d.RemoveAt(3);
			PrintListToConsoleOut(d);
			d.RemoveAt(0);
			PrintListToConsoleOut(d);
			d.RemoveAt(3);
			PrintListToConsoleOut(d);
		}

		static void t8R(IList<int> d) {
			Console.Write("Reverse: ");
			PrintListToConsoleOut(DequeTest.GetReverseView((Deque<int>) d));
		}

		static void t9X(IList<int> d) {
			for (int i = 0; i < BatchLength; i += 2) {
				d.Remove(i);
			}
		}

		static void t10X(IList<int> d) {
			for (int i = BatchLength; i > -BatchLength; i -= 2) {
				Console.Write(d.Remove(i));
				Console.Write(' ');
			}
			Console.WriteLine();
		}

		static void t11(IList<int> d) {
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

		static void t12(IList<int> d) {
			Console.WriteLine(d.IndexOf(1));
			Console.WriteLine(d.IndexOf(-2));
			Console.WriteLine(d.IndexOf(11));
			Console.WriteLine(d.IndexOf(2));
			Console.WriteLine(d.IndexOf(42));
		}

		static void t13C(IList<int> d) {
			d.Clear();
			PrintListToConsoleOut(d);
		}

		static void t14X(IList<int> d) {
			for (int i = 0; i < BatchLength; i++) {
				d.RemoveAt(0);
			}
			PrintListToConsoleOut(d);
		}

		static void t15X(IList<int> d) {
			for (int i = 0; i < BatchLength; i++) {
				d.RemoveAt(d.Count - 1);
			}
			PrintListToConsoleOut(d);
		}

		static void tcopy(IList<int> d) {
			int[] a = new int[2 * BatchLength];
			for (int i = 0; i < a.Length; i++) {
				a[i] = (int) 0x11BBCCDD;
			}

			d.CopyTo(a, BatchLength / 2);

			for (int i = 0; i < a.Length; i++) {
				Console.Write(a[i]);
				Console.Write(' ');
			}
			Console.WriteLine();
		}

		static void ts1(IList<string> ds) {
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

		static void ts2(IList<string> ds) {
			IList<string> dsr = DequeTest.GetReverseView((Deque<string>) ds);
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

		class X {
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

		static void tc() {
			const int Runs = 100;

			IList<X>[] dx = new Deque<X>[Runs];

			for (int r = 0; r < Runs; r++) {
				dx[r] = new Deque<X>();
				for (int i = 0; i < BatchLength; i++) {
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
					dx[r].Add(x);
				}
				for (int i = 0; i < BatchLength; i++) {
					X x = new X();
					x.a = i;
					x.b = -i;
					dx[r].Insert(0, x);
				}
				dx[r].Clear();

				// Console.WriteLine(GC.GetTotalMemory(false));
			}

			for (int r = 0; r < Runs; r++) {
				Console.WriteLine("dx[{0}].Count={1}", r, dx[r].Count);
			}
		}

		static void tInvOpAdd(int length, int errorAt) {
			Deque<X> dx = new Deque<X>();
			for (int i = 0; i < length; i++) {
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
			for (int i = 0; i < BatchLength; i++) {
				X x = new X();
				x.a = i;
				x.b = -i;
				dx.Insert(0, x);
			}

			try {
				int j = 0;
				foreach (var t in dx) {
					if (j == errorAt) {
						dx.Add(new X());
					}
					j++;
				}

				Console.WriteLine("BAD");
			} catch (InvalidOperationException) {
				Console.WriteLine("OK");
			}
		}

		static void tInvOpRemove(int length, int errorAt) {
			Deque<X> dx = new Deque<X>();
			for (int i = 0; i < length; i++) {
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
			for (int i = 0; i < BatchLength; i++) {
				X x = new X();
				x.a = i;
				x.b = -i;
				dx.Insert(0, x);
			}

			try {
				int j = 0;
				foreach (var t in dx) {
					if (j == errorAt) {
						dx.RemoveAt(0);
					}
					j++;
				}

				Console.WriteLine("BAD");
			} catch (InvalidOperationException) {
				Console.WriteLine("OK");
			}
		}

		const int LONGRUN = 1000000;

		static void tlong1(IList<int> d) {
			for (int i = 0; i < LONGRUN; i++) {
				d.Add(i + 37);
			}
			for (int i = 0; i < LONGRUN; i++) {
				d.Insert(0, (i + 37) * 13);
				d.RemoveAt(0);
			}

			bool ok = true;

			for (int i = 0; i < LONGRUN; i++) {
				if (d[i] != i + 37) {
					ok = false;
				}
			}

			if (d.Count != LONGRUN) ok = false;

			if (ok == true) {
				Console.WriteLine("OK");
			} else {
				Console.WriteLine("BAD");
			}
		}

		static void tlong2(IList<int> d) {
			for (int i = 0; i < LONGRUN; i++) {
				d.Add((i + 37) * 13);
			}
			for (int i = 0; i < LONGRUN; i++) {
				d.Add((i + 37) * 13);
				d.RemoveAt(LONGRUN);
			}

			bool ok = true;

			for (int i = 0; i < LONGRUN; i++) {
				if (d[i] != (i + 37) * 13) {
					ok = false;
				}
			}

			if (d.Count != LONGRUN) ok = false;

			if (ok == true) {
				Console.WriteLine("OK");
			} else {
				Console.WriteLine("BAD");
			}
		}

		static void PrintListToConsoleOut<T>(IList<T> list) {
			Console.Write("Count={0} Items=IEnumerable{{", list.Count);
			bool isFirst = true;
			foreach (var t in list) {
				if (isFirst) {
					isFirst = false;
				} else {
					Console.Write(' ');
				}
				Console.Write(t);
			}
			Console.Write("} IList{");
			for (int i = 0; i < list.Count; i++) {
				if (i > 0) {
					Console.Write(' ');
				}
				Console.Write(list[i]);
			}
			Console.WriteLine("}");
		}

		static void PrintListToConsoleOutEnum<T>(IList<T> list) {
			Console.Write("Count={0} Items=IEnumerable{{", list.Count);
			bool isFirst = true;
			foreach (var t in list) {
				if (isFirst) {
					isFirst = false;
				} else {
					Console.Write(' ');
				}
				Console.Write(t);
			}
			Console.WriteLine("}");
		}

		static void PrintListToConsoleOutList<T>(IList<T> list) {
			Console.Write("Count={0} Items=IList{{", list.Count);
			for (int i = 0; i < list.Count; i++) {
				if (i > 0) {
					Console.Write(' ');
				}
				Console.Write(list[i]);
			}
			Console.WriteLine("}");
		}
	}
}