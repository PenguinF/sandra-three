#region License
/*********************************************************************************
 * Program.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
**********************************************************************************/
#endregion

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Command line args examples:
            // --filter *
            // --filter JsonParserBenchmarks
            // --filter PgnParserBenchmarks
            new BenchmarkSwitcher(typeof(Program).Assembly).Run(args);
        }
    }

    [RyuJitX64Job]
    public class IndependentBenchmarks
    {
        // Serves as an independent non-trivial baseline method which depends about linearly on its input size.
        // Might be useful when comparing benchmarks run on different machines.
        //
        // Example:
        // > Enumerable.Range(0, 40).Select(Benchmarks.IndependentBenchmarks.GetPrime).ToArray()
        // int[40] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173 }
        public static int GetPrime(int primeIndex)
        {
            // 20 = some wild guess at the ratio between primes and non-primes.
            // sieve[index] == true if 'index' is definitely non-prime.
            // sieve[0] and sieve[1] are ignored.
            bool[] sieve = new bool[primeIndex * 20];

            int index = 2;
            int foundPrimes = 0;

            for (; ; )
            {
                if (foundPrimes == primeIndex) return index;
                for (int i = index * index; i < sieve.Length; i += index) sieve[i] = true;

                foundPrimes++;
                index++;

                // This throws IndexOutOfRangeException if the sieve is too small.
                while (sieve[index]) index++;
            }
        }

        [Params(45, 450)]
        public int N { get; set; }

        [Benchmark]
        public int GetPrime() => GetPrime(N);
    }
}
