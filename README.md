[![Build & test for dotnet 3.1, 5.0, 6.0](https://github.com/fsprojects/SIMDArray/actions/workflows/test.yml/badge.svg)](https://github.com/fsprojects/SIMDArray/actions/workflows/test.yml)

# SIMDArray FSharp
SIMD and other Performance enhanced Array operations for F#

## Example Usage

``` F#
//Faster map

let array = [| 1 .. 1000 |]
let squaredArray = array |> Array.SIMD.map (fun x -> x*x) (fun x -> x*x)  

// Map and many other functions need one lambda to map the Vector<T>, 
// and one to handle any leftover elements if array is not divisible by 
// Vector<T>.Count. In the case of simple arithmetic operations they can
// often be the same as shown here. If you arrange your arrays such that 
// they will never have leftovers, or don't care how leftovers are treated 
// just pass a nop like so:

open SIMDArrayUtils

let array = [|1;2;3;4;5;6;7;8|]
let squaredArray = array |> Array.SIMD.map (fun x -> x*x) nop


// Some functions can be used just like the existing array functions but run faster
// such as create and sum:

let newArray = Array.SIMD.create 1000 5 //create a new array of length 1000 filled with 5
let sum = Array.SIMD.sum newArray

// The Performance module has functions that are faster and/or use less memory
// via other means than SIMD. Usually by relaxing ordering constraints or adding
// constraints to predicates:

let distinctElements = Array.Performance.distinctUnordered someArray
let filteredElements = Array.Performance.filterLessThan 5 someArray
let filteredElements = Array.Performance.filterSimplePredicate (fun x -> x*x < 100) someArray
Array.Performance.mapInPlace (fun x-> x*x) someArray

// The SIMDParallel module has parallelized versions of some of the SIMD operations:

let sum = Array.SIMDParallel.sum array
let map = Array.SIMDParallel.map (fun x -> x*x) array

// Two extensions are added to System.Threading.Tasks.Parallel, to enable Parallel.For loops
// with a stride length efficiently. They also have much less overhead. You can use them to roll your own 
// parallel SIMD functions, or any parallel operation that needs a stride length > 1

// Using:
// ForStride (fromInclusive : int) (toExclusive :int) (stride : int) (f : int -> unit)
// You can map each Vector in an array and store it in result
Parallel.ForStride 0 array.Length (Vector< ^T>.Count) 
        (fun i -> (vf (Vector< ^T>(array,i ))).CopyTo(result,i))

// Using:
// ForStrideAggreagate (fromInclusive : int) (toExclusive :int) (stride : int) (acc: ^T) (f : int -> ^T -> ^T) combiner
// You can sum or otherwise aggregate the elements of an array a Vector at a time, starting from an initial acc
let result = Parallel.ForStrideAggreagate 0 array.Length (Vector< ^T>.Count) Vector< ^T>(0)
					(fun i acc -> acc + (Vector< ^T>(array,i)))  
					(fun x acc -> x + acc)  //combines the results from each task into a final Vector that is returned


```

## Notes

Only 64 bit builds are supported.  Mono should work with 5.0+, but I have not yet tested it. Performance improvements will vary depending on your CPU architecture, width of Vector type, and the operations you apply.  For small arrays the core libs may be faster due SIMD overhead.
When measuring performance be sure to use Release builds with optimizations turned on.

Floating point addition is not associative, so results with SIMD operations will not be identical, though often
they will be more accurate, such as in the case of sum, or average.

## Upd: .NET 7.0 Basic Tests
```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1526 (21H2)
AMD Ryzen 7 3800X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.100-preview.3.22179.4
  [Host]     : .NET 7.0.0 (7.0.22.17504), X64 RyuJIT DEBUG
  DefaultJob : .NET 7.0.0 (7.0.22.17504), X64 RyuJIT


|     Method |  Length |            Mean |         Error |        StdDev |   Gen 0 |   Gen 1 |   Gen 2 |   Allocated |
|----------- |-------- |----------------:|--------------:|--------------:|--------:|--------:|--------:|------------:|
|     ForSum |     100 |        98.78 ns |      0.533 ns |      0.473 ns |  0.0507 |       - |       - |       424 B |
| ForSumSIMD |     100 |        56.32 ns |      0.378 ns |      0.353 ns |  0.0507 |  0.0001 |       - |       424 B |
|        Dot |     100 |       157.32 ns |      0.672 ns |      0.629 ns |       - |       - |       - |           - |
|    DotSIMD |     100 |        19.59 ns |      0.121 ns |      0.107 ns |       - |       - |       - |           - |
|        Max |     100 |        55.57 ns |      0.146 ns |      0.129 ns |       - |       - |       - |           - |
|    MaxSIMD |     100 |        13.53 ns |      0.070 ns |      0.065 ns |       - |       - |       - |           - |
|      MaxBy |     100 |        60.37 ns |      0.163 ns |      0.153 ns |       - |       - |       - |           - |
|  MaxBySIMD |     100 |        20.06 ns |      0.063 ns |      0.056 ns |       - |       - |       - |           - |
|     ForSum |    1000 |       862.28 ns |      5.412 ns |      5.063 ns |  0.4807 |  0.0067 |       - |     4,024 B |
| ForSumSIMD |    1000 |       441.22 ns |      2.874 ns |      2.548 ns |  0.4809 |  0.0072 |       - |     4,024 B |
|        Dot |    1000 |     1,484.23 ns |      5.292 ns |      4.691 ns |       - |       - |       - |           - |
|    DotSIMD |    1000 |       162.66 ns |      1.095 ns |      0.971 ns |       - |       - |       - |           - |
|        Max |    1000 |       526.03 ns |      2.177 ns |      1.818 ns |       - |       - |       - |           - |
|    MaxSIMD |    1000 |        44.45 ns |      0.101 ns |      0.094 ns |       - |       - |       - |           - |
|      MaxBy |    1000 |       506.51 ns |      0.619 ns |      0.548 ns |       - |       - |       - |           - |
|  MaxBySIMD |    1000 |       139.48 ns |      0.126 ns |      0.106 ns |       - |       - |       - |           - |
|     ForSum | 1000000 | 1,642,884.15 ns | 32,686.799 ns | 52,783.087 ns | 93.7500 | 93.7500 | 93.7500 | 4,000,061 B |
| ForSumSIMD | 1000000 |   484,576.66 ns |  9,685.048 ns |  9,512.012 ns | 95.7031 | 95.7031 | 95.7031 | 4,000,055 B |
|        Dot | 1000000 | 1,468,907.49 ns |  6,495.111 ns |  5,070.956 ns |       - |       - |       - |           - |
|    DotSIMD | 1000000 |   160,549.66 ns |    277.915 ns |    232.071 ns |       - |       - |       - |           - |
|        Max | 1000000 |   485,969.64 ns |    565.230 ns |    501.061 ns |       - |       - |       - |           - |
|    MaxSIMD | 1000000 |    48,748.71 ns |     72.373 ns |     67.698 ns |       - |       - |       - |           - |
|      MaxBy | 1000000 |   490,922.69 ns |    563.828 ns |    470.822 ns |       - |       - |       - |           - |
|  MaxBySIMD | 1000000 |   135,049.15 ns |     57.546 ns |     51.013 ns |       - |       - |       - |           - |


```

## Performance Comparison vs Standard Array Functions

* [VS Core Lib Parallel](#parallel)
* [VS Core Lib 32bit Floats](#core32)
* [VS Core Lib 64bit Floats](#core64)
* [VS MathNET.Numerics 32bit Floats](#mathnet)
* [VS MathNET.Numerics MKL Native 32bit Floats](#mathnetnative)

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4712HQ CPU 2.30GHz, ProcessorCount=8
Frequency=2240907 ticks, Resolution=446.2479 ns, Timer=TSC
CLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1590.0

Type=SIMDBenchmark  Mode=Throughput  Platform=X64  
Jit=RyuJit  GarbageCollection=Concurrent Workstation  

```

### Sum 1 million 32bit ints, ParallelSIMD vs SIMD vs Core Lib <a name="parallel"></a>

|		  Method |  Length |      Median |     StdDev | Scaled | Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
|---------------- |-------- |------------ |----------- |------- |------ |------ |------ |------------------- |
|             sum | 1000000 | 979.9477 us | 15.4036 us |   1.00 |     - |     - |  1.00 |          14,967.09 |
|         SIMDsum | 1000000 | 163.5663 us |  2.7872 us |   0.17 |     - |     - |  0.17 |           1,960.97 |
| SIMDParallelsum | 1000000 |  82.3069 us |  6.4637 us |   0.08 |  3.74 |     - |  0.04 |           1,674.94 |



### With 32bit Floats Vs Core Lib. Map function `(fun x -> x*x)`<a name="core32"></a>

|       Method |  Length |            Median |         StdDev | Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
|------------- |-------- |------------------ |--------------- |------ |------ |------- |------------------- |
| **SIMDContains** |      **10** |        **32.3354 ns** |      **0.0933 ns** |  **0.04** |     **-** |      **-** |              **22.80** |
|     Contains |      10 |        13.0234 ns |      0.6457 ns |     - |     - |      - |               0.00 |
|      SIMDMap |      10 |        37.3615 ns |      0.0693 ns |  0.09 |     - |      - |              53.95 |
|          Map |      10 |        15.6651 ns |      0.2422 ns |  0.04 |     - |      - |              25.80 |
|      SIMDSum |      10 |        19.3450 ns |      0.1866 ns |     - |     - |      - |               0.00 |
|          Sum |      10 |         6.2273 ns |      0.2982 ns |     - |     - |      - |               0.00 |
|      SIMDMax |      10 |        20.8972 ns |      0.7380 ns |     - |     - |      - |               0.00 |
|          Max |      10 |         7.9275 ns |      0.9701 ns |     - |     - |      - |               0.00 |
| **SIMDContains** |     **100** |        **61.6295 ns** |      **5.0472 ns** |  **0.04** |     **-** |      **-** |              **24.92** |
|     Contains |     100 |       140.9920 ns |      2.4739 ns |     - |     - |      - |               0.01 |
|      SIMDMap |     100 |        75.8733 ns |      0.5875 ns |  0.33 |     - |      - |             192.40 |
|          Map |     100 |       120.3029 ns |      0.4232 ns |  0.29 |     - |      - |             172.39 |
|      SIMDSum |     100 |        32.0058 ns |      1.1225 ns |     - |     - |      - |               0.00 |
|          Sum |     100 |        77.6100 ns |      2.4902 ns |     - |     - |      - |               0.00 |
|      SIMDMax |     100 |        35.9042 ns |      2.0587 ns |     - |     - |      - |               0.00 |
|          Max |     100 |        92.1754 ns |      9.6637 ns |     - |     - |      - |               0.00 |
| **SIMDContains** |    **1000** |       **417.0760 ns** |     **10.6672 ns** |     **-** |     **-** |      **-** |               **0.04** |
|     Contains |    1000 |     1,333.0239 ns |     11.8959 ns |     - |     - |      - |               0.07 |
|      SIMDMap |    1000 |       439.8549 ns |      7.5810 ns |  3.05 |     - |      - |           2,176.91 |
|          Map |    1000 |     1,073.2894 ns |     16.1444 ns |  2.93 |     - |      - |           2,086.24 |
|      SIMDSum |    1000 |       162.8308 ns |      5.8158 ns |     - |     - |      - |               0.01 |
|          Sum |    1000 |       947.1124 ns |     14.4370 ns |     - |     - |      - |               0.07 |
|      SIMDMax |    1000 |       167.0257 ns |      5.3584 ns |     - |     - |      - |               0.01 |
|          Max |    1000 |       698.2252 ns |     21.2244 ns |     - |     - |      - |               0.03 |
| **SIMDContains** | **1000000** |   **427,765.2001 ns** |  **3,541.8344 ns** |     **-** |     **-** |   **0.23** |           **7,507.17** |
|     Contains | 1000000 | 1,315,198.8375 ns | 19,634.6409 ns |     - |     - |   0.36 |          14,912.24 |
|      SIMDMap | 1000000 | 1,747,002.9295 ns | 18,219.0807 ns |     - |     - | 519.18 |       1,198,305.57 |
|          Map | 1000000 | 1,962,408.1761 ns | 23,319.8186 ns |     - |     - | 746.00 |       1,702,687.72 |
|      SIMDSum | 1000000 |   160,972.7015 ns |  3,359.1696 ns |     - |     - |   0.05 |           1,960.97 |
|          Sum | 1000000 |   955,224.0942 ns | 12,365.7613 ns |     - |     - |   0.38 |          14,853.87 |
|      SIMDMax | 1000000 |   158,835.3746 ns |  3,773.1697 ns |     - |     - |   0.06 |           1,961.66 |
|          Max | 1000000 |   633,761.7634 ns |  6,149.8767 ns |     - |     - |   0.24 |           7,495.76 |

### With 64bit Floats vs Core Lib. Map function `(fun x -> x*x+x)`<a name="core64"></a>

|       Method |  Length |            Median |         StdDev | Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
|------------- |-------- |------------------ |--------------- |------ |------ |------- |------------------- |
| **SIMDContains** |    **1000** |       **842.2604 ns** |     **36.6615 ns** |     **-** |     **-** |      **-** |               **0.13** |
|     Contains |    1000 |     1,338.2032 ns |     21.7835 ns |     - |     - |      - |               0.13 |
|      SIMDSum |    1000 |       302.8986 ns |     12.0417 ns |     - |     - |      - |               0.03 |
|          Sum |    1000 |       953.9314 ns |      7.3770 ns |     - |     - |      - |               0.13 |
|      SIMDMax |    1000 |       302.3690 ns |     11.8064 ns |     - |     - |      - |               0.03 |
|          Max |    1000 |       713.9227 ns |     23.1721 ns |     - |     - |      - |               0.07 |
|      SIMDMap |    1000 |       905.3396 ns |     21.1726 ns |  2.79 |     - |      - |           4,447.68 |
|          Map |    1000 |     1,369.6668 ns |     17.1072 ns |  2.88 |     - |      - |           4,591.74 |
| **SIMDContains** |  **100000** |    **86,987.0417 ns** |    **212.5612 ns** |     **-** |     **-** |      **-** |             **204.08** |
|     Contains |  100000 |   129,737.5287 ns |  2,300.6178 ns |     - |     - |      - |             398.91 |
|      SIMDSum |  100000 |    30,836.7527 ns |     52.3596 ns |     - |     - |      - |             103.84 |
|          Sum |  100000 |    97,310.6367 ns |    444.7469 ns |     - |     - |      - |             203.88 |
|      SIMDMax |  100000 |    30,755.6959 ns |    189.2460 ns |     - |     - |      - |             103.84 |
|          Max |  100000 |    65,190.8396 ns |    810.8605 ns |     - |     - |      - |             203.88 |
|      SIMDMap |  100000 |   250,263.5686 ns | 23,822.3931 ns |     - |     - | 351.03 |         384,182.34 |
|          Map |  100000 |   239,693.9435 ns | 20,283.1824 ns |     - |     - | 350.24 |         383,399.62 |
| **SIMDContains** | **1000000** |   **952,116.9191 ns** | **22,885.3666 ns** |     **-** |     **-** |   **0.17** |          **29,960.47** |
|     Contains | 1000000 | 1,469,353.0761 ns | 44,872.5327 ns |     - |     - |   0.15 |          28,150.78 |
|      SIMDSum | 1000000 |   493,523.5731 ns |  6,629.8292 ns |     - |     - |   0.12 |          15,020.79 |
|          Sum | 1000000 | 1,059,862.2497 ns | 21,029.2608 ns |     - |     - |   0.17 |          29,921.97 |
|      SIMDMax | 1000000 |   486,232.3883 ns |  3,963.6126 ns |     - |     - |   0.11 |          15,080.61 |
|          Max | 1000000 |   771,554.3061 ns |  7,083.0659 ns |     - |     - |   0.12 |          15,008.20 |
|      SIMDMap | 1000000 | 3,625,255.0307 ns | 40,939.9131 ns |     - |     - | 439.00 |       3,763,516.65 |
|          Map | 1000000 | 3,490,854.2334 ns | 51,255.2300 ns |     - |     - | 413.00 |       3,589,365.95 |


### With 32bit Floats vs MathNET.Numerics managed. Map function `(fun x -> x*x+x)` <a name="mathnet"></a>

|            Method |  Length |          Median |         StdDev | Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
|------------------ |-------- |---------------- |--------------- |------ |------ |------ |------------------- |
|    **SIMDMapInPlace** |     **100** |      **46.5269 ns** |      **4.9229 ns** |  **0.08** |     **-** |     **-** |              **22.54** |
| MathNETMapInPlace |     100 |     354.0866 ns |      7.5375 ns |  0.36 |     - |     - |              99.59 |
|           SIMDSum |     100 |      32.0283 ns |      2.9529 ns |     - |     - |     - |               0.00 |
|        MathNETSum |     100 |      88.7532 ns |      1.9561 ns |     - |     - |     - |               0.00 |
|    **SIMDMapInPlace** |    **1000** |     **165.7885 ns** |      **9.0778 ns** |     **-** |     **-** |     **-** |               **0.01** |
| MathNETMapInPlace |    1000 |   3,057.9378 ns |     56.8845 ns |  0.30 |     - |     - |              94.64 |
|           SIMDSum |    1000 |     163.1672 ns |      6.7001 ns |     - |     - |     - |               0.01 |
|        MathNETSum |    1000 |     962.2084 ns |     13.9839 ns |     - |     - |     - |               0.12 |
|    **SIMDMapInPlace** |  **100000** |  **21,078.0491 ns** |    **627.8978 ns** |     **-** |     **-** |     **-** |              **56.61** |
| MathNETMapInPlace |  100000 | 104,831.7547 ns |  8,823.8473 ns |  5.26 |     - |     - |           2,267.50 |
|           SIMDSum |  100000 |  15,134.0240 ns |    708.8177 ns |     - |     - |     - |              46.02 |
|        MathNETSum |  100000 |  97,051.7780 ns |    875.9276 ns |     - |     - |     - |             217.82 |
|    **SIMDMapInPlace** | **1000000** | **220,760.2212 ns** |  **7,167.1597 ns** |     **-** |     **-** |  **0.46** |           **7,402.18** |
| MathNETMapInPlace | 1000000 | 824,388.9221 ns | 47,134.8321 ns |     - |     - |  1.87 |          33,210.67 |
|           SIMDSum | 1000000 | 159,887.6959 ns |  5,030.3486 ns |     - |     - |  0.18 |           3,433.93 |
|        MathNETSum | 1000000 | 967,761.7422 ns | 17,557.1206 ns |     - |     - |  2.00 |          29,450.93 |

### With 32bit Floats vs MathNET.Numerics MKL Native. Adding two arrays <a name="mathnetnative"></a>
|     Method |  Length |            Median |          StdDev | Gen 0 | Gen 1 |    Gen 2 | Bytes Allocated/Op |
|----------- |-------- |------------------ |---------------- |------ |------ |--------- |------------------- |
|   **SIMDMap2** |     **100** |        **92.1515 ns** |       **3.0304 ns** |  **2.70** |     **-** |        **-** |             **212.76** |
| MathNETAdd |     100 |       156.7522 ns |       7.3969 ns |  2.92 |     - |        - |             230.42 |
|   **SIMDMap2** |    **1000** |       **493.5448 ns** |       **8.1340 ns** | **21.40** |     **-** |        **-** |           **2,048.32** |
| MathNETAdd |    1000 |       444.0753 ns |       5.9375 ns | 20.12 |     - |        - |           1,553.56 |
|   **SIMDMap2** |  **100000** |   **161,024.7782 ns** |  **24,704.0627 ns** |     **-** |     **-** | **2,348.29** |         **197,602.33** |
| MathNETAdd |  100000 |   155,985.3149 ns |   1,478.0502 ns |     - |     - | 1,755.36 |         155,754.29 |
|   **SIMDMap2** | **1000000** | **2,024,351.2170 ns** | **242,101.0167 ns** |     **-** |     **-** | **3,317.76** |       **2,025,584.78** |
| MathNETAdd | 1000000 | 1,551,270.9391 ns | 216,545.6630 ns |     - |     - | 2,466.00 |       1,693,319.93 |
