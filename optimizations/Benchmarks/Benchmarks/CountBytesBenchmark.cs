using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks.Benchmarks
{
    [DisassemblyDiagnoser]
    public class CountBytesBenchmark
    {
        private int[] _bytes;
        private byte[] _bytesToCount;

        [GlobalSetup]
        public void Setup()
        {
            var rnd = new Random();
            _bytesToCount = Enumerable.Range(0, 10_000_000).Select(x => (byte)0).ToArray();
            rnd.NextBytes(_bytesToCount);
            _bytes =  new int[byte.MaxValue + 1];
        }

        [Benchmark]
        public void ParallelCount()
        {
            Array.Fill(_bytes, 0);
            Parallel.ForEach(_bytesToCount, b => Interlocked.Increment(ref _bytes[b]));
        }
        
        [Benchmark]
        public void ForeachCount()
        {
            Array.Fill(_bytes, 0);
            foreach (var b in _bytesToCount)
            {
                _bytes[b]++;
            }
        }
    }
    
    

}