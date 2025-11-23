using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
//using WordFrequency_BenchmarkTests;
using System.Collections.Immutable;

BenchmarkRunner.Run<PrintFrequencies>();

public class PrintFrequencies
{
    SortedDictionary<string, int> frequencies_v2 = new SortedDictionary<string, int>();
    Dictionary<string, int> frequencies_v0_v3 = new Dictionary<string, int>();
    SortedList<string, int> frequencies_v1 = new SortedList<string, int>();


    static void Increment_V0_V3_FromMySolution(Dictionary<string, int> d, string word)
    {
        if (d.ContainsKey(word))
        {
            d[word]++;
        }
        else
        {
            d.Add(word, 1);
        }
    }
    public void PrintWords_V0_FromMySolution(Dictionary<string, int> d)
    {

        foreach (KeyValuePair<string, int> pair in d.OrderBy(pair => pair.Key))
        {
            Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
        }
    }
    static void Increment_V1_SortedList(SortedList<string, int> list, string word)
    {
        if (list.ContainsKey(word))
        {
            list[word]++;
        }
        else
        {
            list.Add(word, 1);
        }
    }
    public void PrintWords_V1_SortedList(SortedList<string, int> list)
    {
        for(int i = 0; i < list.Count; i++ ) 
        {
            Console.WriteLine("{0}: {1}", list.GetKeyAtIndex(i), list.GetValueAtIndex(i));
        }
    }
    static void Increment_V2_SortedDictionary(SortedDictionary<string, int> dictionary, string word)
    {
        if (dictionary.ContainsKey(word))
        {
            dictionary[word]++;
        }
        else {
            dictionary.Add(word, 1);
        }
    }
    public void PrintWords_V2_SortedDict(SortedDictionary<string, int> dictionary) 
    {
        foreach (KeyValuePair<string, int> pair in dictionary)
        {
            Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
        }
    }
    public void PrintWords_V3_Dict(Dictionary<string, int> dictionary)
    {

        var keysSorted = dictionary.Keys.ToList();
        keysSorted.Sort();
        foreach (var k in keysSorted)
        {
            Console.WriteLine("{0} {1}", k, dictionary[k]);
        }
    }


    [Benchmark]
    public void Run_V0_OnlyIncrement()
    {
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "If");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "If");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "station");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
    }

    [Benchmark]
    public void Run_V0()
    {
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "If");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "If");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "station");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");

        PrintWords_V0_FromMySolution(frequencies_v0_v3);
    }

    [Benchmark]
    public void Run_V1_OnlyIncrement()
    {
        Increment_V1_SortedList(frequencies_v1, "If");
        Increment_V1_SortedList(frequencies_v1, "If");
        Increment_V1_SortedList(frequencies_v1, "station");
        Increment_V1_SortedList(frequencies_v1, "stops");
        Increment_V1_SortedList(frequencies_v1, "stops");
        Increment_V1_SortedList(frequencies_v1, "stops");
    }

    [Benchmark]
    public void Run_V1()
    {
        Increment_V1_SortedList(frequencies_v1, "If");
        Increment_V1_SortedList(frequencies_v1, "If");
        Increment_V1_SortedList(frequencies_v1, "station");
        Increment_V1_SortedList(frequencies_v1, "stops");
        Increment_V1_SortedList(frequencies_v1, "stops");
        Increment_V1_SortedList(frequencies_v1, "stops");

        PrintWords_V1_SortedList(frequencies_v1);
    }

    [Benchmark]
    public void Run_V2_OnlyIncrement()
    {
        Increment_V2_SortedDictionary(frequencies_v2, "If");
        Increment_V2_SortedDictionary(frequencies_v2, "If");
        Increment_V2_SortedDictionary(frequencies_v2, "station");
        Increment_V2_SortedDictionary(frequencies_v2, "stops");
        Increment_V2_SortedDictionary(frequencies_v2, "stops");
        Increment_V2_SortedDictionary(frequencies_v2, "stops");
    }
    
    [Benchmark] 
    public void Run_V2() 
    {
        Increment_V2_SortedDictionary(frequencies_v2, "If");
        Increment_V2_SortedDictionary(frequencies_v2, "If");
        Increment_V2_SortedDictionary(frequencies_v2, "station");
        Increment_V2_SortedDictionary(frequencies_v2, "stops");
        Increment_V2_SortedDictionary(frequencies_v2, "stops");
        Increment_V2_SortedDictionary(frequencies_v2, "stops");

        PrintWords_V2_SortedDict(frequencies_v2);
    }

    [Benchmark]
    public void Run_V3() 
    {
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "If");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "If");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "station");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");
        Increment_V0_V3_FromMySolution(frequencies_v0_v3, "stops");

        PrintWords_V3_Dict(frequencies_v0_v3);

    }
}

/*
 // AfterAll
// Benchmark Process 33616 has exited with code 0.

Mean = 340.292 us, StdErr = 1.983 us (0.58%), N = 76, StdDev = 17.290 us
Min = 294.476 us, Q1 = 338.439 us, Median = 343.535 us, Q3 = 349.969 us, Max = 373.587 us
IQR = 11.531 us, LowerFence = 321.142 us, UpperFence = 367.266 us
ConfidenceInterval = [333.499 us; 347.085 us] (CI 99.9%), Margin = 6.793 us (2.00% of Mean)
Skewness = -1, Kurtosis = 3.5, MValue = 2

// ** Remained 0 (0.0%) benchmark(s) to run. Estimated finish 2023-11-16 19:17 (0h 0m from now) **
Successfully reverted power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// ***** BenchmarkRunner: Finish  *****

// * Export *
  BenchmarkDotNet.Artifacts\results\PrintFrequencies-report.csv
  BenchmarkDotNet.Artifacts\results\PrintFrequencies-report-github.md
  BenchmarkDotNet.Artifacts\results\PrintFrequencies-report.html

// * Detailed results *
PrintFrequencies.Run_V0_OnlyIncrement: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 173.110 ns, StdErr = 0.753 ns (0.44%), N = 14, StdDev = 2.819 ns
Min = 166.813 ns, Q1 = 173.130 ns, Median = 174.096 ns, Q3 = 174.702 ns, Max = 176.221 ns
IQR = 1.572 ns, LowerFence = 170.771 ns, UpperFence = 177.061 ns
ConfidenceInterval = [169.929 ns; 176.290 ns] (CI 99.9%), Margin = 3.180 ns (1.84% of Mean)
Skewness = -1.37, Kurtosis = 3.52, MValue = 2
-------------------- Histogram --------------------
[165.278 ns ; 171.574 ns) | @@
[171.574 ns ; 177.756 ns) | @@@@@@@@@@@@
---------------------------------------------------

PrintFrequencies.Run_V0: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 300.008 us, StdErr = 1.660 us (0.55%), N = 36, StdDev = 9.959 us
Min = 284.979 us, Q1 = 292.150 us, Median = 299.223 us, Q3 = 305.313 us, Max = 321.959 us
IQR = 13.163 us, LowerFence = 272.405 us, UpperFence = 325.058 us
ConfidenceInterval = [294.048 us; 305.969 us] (CI 99.9%), Margin = 5.961 us (1.99% of Mean)
Skewness = 0.44, Kurtosis = 2.2, MValue = 2
-------------------- Histogram --------------------
[281.020 us ; 294.503 us) | @@@@@@@@@@@@@@@
[294.503 us ; 305.971 us) | @@@@@@@@@@@@@
[305.971 us ; 317.014 us) | @@@@@@
[317.014 us ; 325.917 us) | @@
---------------------------------------------------

PrintFrequencies.Run_V1_OnlyIncrement: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.272 us, StdErr = 0.007 us (0.51%), N = 21, StdDev = 0.030 us
Min = 1.222 us, Q1 = 1.251 us, Median = 1.279 us, Q3 = 1.298 us, Max = 1.320 us
IQR = 0.047 us, LowerFence = 1.181 us, UpperFence = 1.369 us
ConfidenceInterval = [1.247 us; 1.297 us] (CI 99.9%), Margin = 0.025 us (1.98% of Mean)
Skewness = -0.23, Kurtosis = 1.7, MValue = 2.22
-------------------- Histogram --------------------
[1.216 us ; 1.244 us) | @@@@@
[1.244 us ; 1.275 us) | @@@@
[1.275 us ; 1.304 us) | @@@@@@@@@
[1.304 us ; 1.334 us) | @@@
---------------------------------------------------

PrintFrequencies.Run_V1: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 292.754 us, StdErr = 1.311 us (0.45%), N = 21, StdDev = 6.006 us
Min = 279.570 us, Q1 = 289.892 us, Median = 293.207 us, Q3 = 297.979 us, Max = 301.358 us
IQR = 8.087 us, LowerFence = 277.761 us, UpperFence = 310.110 us
ConfidenceInterval = [287.709 us; 297.799 us] (CI 99.9%), Margin = 5.045 us (1.72% of Mean)
Skewness = -0.39, Kurtosis = 2.12, MValue = 2
-------------------- Histogram --------------------
[276.713 us ; 282.976 us) | @
[282.976 us ; 289.784 us) | @@@@
[289.784 us ; 302.057 us) | @@@@@@@@@@@@@@@@
---------------------------------------------------

PrintFrequencies.Run_V2_OnlyIncrement: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.143 us, StdErr = 0.005 us (0.47%), N = 16, StdDev = 0.022 us
Min = 1.105 us, Q1 = 1.133 us, Median = 1.139 us, Q3 = 1.162 us, Max = 1.182 us
IQR = 0.029 us, LowerFence = 1.088 us, UpperFence = 1.206 us
ConfidenceInterval = [1.121 us; 1.165 us] (CI 99.9%), Margin = 0.022 us (1.92% of Mean)
Skewness = 0.04, Kurtosis = 1.98, MValue = 2
-------------------- Histogram --------------------
[1.102 us ; 1.129 us) | @@@
[1.129 us ; 1.183 us) | @@@@@@@@@@@@@
---------------------------------------------------

PrintFrequencies.Run_V2: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 298.899 us, StdErr = 1.400 us (0.47%), N = 16, StdDev = 5.601 us
Min = 281.995 us, Q1 = 297.922 us, Median = 299.959 us, Q3 = 301.648 us, Max = 305.574 us
IQR = 3.726 us, LowerFence = 292.332 us, UpperFence = 307.237 us
ConfidenceInterval = [293.196 us; 304.602 us] (CI 99.9%), Margin = 5.703 us (1.91% of Mean)
Skewness = -1.58, Kurtosis = 5.52, MValue = 2
-------------------- Histogram --------------------
[279.077 us ; 284.913 us) | @
[284.913 us ; 296.604 us) | @@
[296.604 us ; 308.492 us) | @@@@@@@@@@@@@
---------------------------------------------------

PrintFrequencies.Run_V3: DefaultJob
Runtime = .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 340.292 us, StdErr = 1.983 us (0.58%), N = 76, StdDev = 17.290 us
Min = 294.476 us, Q1 = 338.439 us, Median = 343.535 us, Q3 = 349.969 us, Max = 373.587 us
IQR = 11.531 us, LowerFence = 321.142 us, UpperFence = 367.266 us
ConfidenceInterval = [333.499 us; 347.085 us] (CI 99.9%), Margin = 6.793 us (2.00% of Mean)
Skewness = -1, Kurtosis = 3.5, MValue = 2
-------------------- Histogram --------------------
[289.119 us ; 298.489 us) | @
[298.489 us ; 309.204 us) | @@@@@@@@
[309.204 us ; 318.631 us) | @@@
[318.631 us ; 328.651 us) |
[328.651 us ; 337.909 us) | @@@@@@
[337.909 us ; 348.624 us) | @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
[348.624 us ; 360.471 us) | @@@@@@@@@@@@@@@@@
[360.471 us ; 370.743 us) | @@@@
[370.743 us ; 378.944 us) | @
---------------------------------------------------

// * Summary *

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.402
  [Host]     : .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2


| Method                  | Mean         | Error       | StdDev       |
|------------------------ |-------------:|------------:|-------------:|
| Run_V0_OnlyIncrement    |     173.1 ns |     3.18 ns |      2.82 ns |
| Run_V0                  | 300,008.4 ns | 5,960.57 ns |  9,958.78 ns |
| Run_V1_OnlyIncrement    |   1,272.1 ns |    25.17 ns |     29.97 ns |
| Run_V1                  | 292,754.0 ns | 5,044.86 ns |  6,005.55 ns |
| Run_V2_OnlyIncrement    |   1,142.8 ns |    21.92 ns |     21.53 ns |
| Run_V2                  | 298,899.1 ns | 5,703.23 ns |  5,601.33 ns |
| Run_V3                  | 340,291.6 ns | 6,792.92 ns | 17,290.15 ns |

// * Hints *
Outliers
  PrintFrequencies.Run_V0_OnlyIncrement: Default    -> 1 outlier  was  removed, 3 outliers were detected (168.68 ns, 168.85 ns, 179.01 ns)
  PrintFrequencies.Run_V0: Default                  -> 1 outlier  was  removed (367.87 us)
  PrintFrequencies.Run_V1_OnlyIncrement: Default    -> 2 outliers were removed (1.38 us, 1.45 us)
  PrintFrequencies.Run_V1: Default                  -> 1 outlier  was  removed (310.99 us)
  PrintFrequencies.Run_V2: Default                  -> 1 outlier  was  detected (282.00 us)
  PrintFrequencies.Run_V3: Default                  -> 9 outliers were removed, 21 outliers were detected (294.48 us..314.66 us, 400.83 us..518.84 us)

// * Legends *
  Mean   : Arithmetic mean of all measurements
  Error  : Half of 99.9% confidence interval
  StdDev : Standard deviation of all measurements
  1 ns   : 1 Nanosecond (0.000000001 sec)

// ***** BenchmarkRunner: End *****
Run time: 00:02:56 (176.1 sec), executed benchmarks: 7

Global total time: 00:03:15 (195.72 sec), executed benchmarks: 7

// * My Conclusion *

Tady bylo 7 benchmarků, protože: Run_V* zkouší 4 způsoby vytisku slov, 3 jsou ze zadání a 1 (0.) je to, co bylo použito v mém řešení.
Ostatní 3 benchmarky Run_V*_* jsou potřebné k rozlišení operace Add pro různé struktury (0. a 3. jsou stejné struktury, takže naní 
potřeba pro 3. vytvářet zvlaštní benchmark)

Ještě spočteme stejnou tabulku pro tisk slov (odečtením Run_V0_OnlyIncrement - Run_V0 apod.)

| Method                  | Mean         | Error       | StdDev       |
|------------------------ |-------------:|------------:|-------------:|
| Tisk_V0                 | 299,835.3 ns | 5,957.39 ns |  9,955.96 ns |
| Tisk_V1                 | 291,181.9 ns | 5,019.69 ns |  5,975.58 ns |
| Tisk_V2                 | 297,756.3 ns | 5,681.31 ns |  5,579.8  ns |
| Tisk_V3                 | 340,118.5 ns | 6,789.74 ns | 17,287.33 ns |

Takže předpokládáme tři měření:
1. Vložení páru do struktury
2. Tisk třiděných párů
3. Čas v součtu 1 a 2.

Jestli se díváme na střední hodnotu, tak v tom tisku je nejlepší Tisk_V1 pomocí použití struktury SortedList. 
Jestli se díváme na střední hodnotu v testech *_OnlyIncrement, tak na to vložení je nejlepší V0 V3, kde byl použit Dictionary.

Ohledně těch funkcí Increment_...: ty jsem chtěla mít maximalně shodné, jelikož mě nejvíc zajímala ta práce se strukturami
(Proto jsem nechtěla používat různé metody vložení párů, jak máme ve druhém úkolu).
Očekávaně vložení do Dictionary je rychlejší ve srovnání s ostátními. Předpokládám, protože při vložení do Dictionary není potřeba zatřídit 
klíč z páru.

Tá rychlost vložení ale nekompenzuje to, kolik času stravíme tříděním a dalším tískem. Hlavní závěr by mohl být v tom, že nám nestčí
dívat se jenom na jednu část pro měření rychlosti. Musíme zvažít všechna scenáře použití struktury v našem programu, podívat se na ně zvlašť,
pak spolu a z toho najít tu nejvhodnější pro naši situaci.

Zde bych řekla nejvhodnější je SortedList. A ten SortedList ještě nemá tak velkou odchylku, což zase je dobré (jistě je lepší než Dictionary, 
který má ve dvou ze tří měřeních nejhorší výsledky a ještě k tomu i velkou odchylku)

Testy s V0 jsou byly nejspiš pro mě, jelikož se mě zajímalo, jak pomalá byla moje implementace. V tom tisku ten .OrderBy není tak
pomalý, jako třidění klíčů z Dictionary.

 */