# SIMDArray FSharp
SIMD enhanced Array operations for F#

## Example Usage

``` F#
open SIMDArray

//Faster map
let array = [| 1 .. 1000 |]
let squaredArray = array |> Array.SIMD.map (fun x -> x*x) (fun x -> x*x)  

// Map needs one lambda to map the Vector<T>, and one to handle any leftover
// elements if array is not divisible by Vector<T>.Count. In the case of 
// simple arithmetic operations they can often be the same as shown here.


//Faster create and sum
let newArray = Array.SIMD.create 1000 5 //create a new array of length 1000 filled with 5
let sum = Array.SIMD.sum newArray

```

## Notes

Only 64 bit builds are supported.  Performance improvements will vary depending on your CPU architecture, width of Vector type, and the operations
you apply.  For small arrays the core libs may be faster due to increased fixed overhead for SIMD versions of the operations. To test
performance be sure to use Release builds with optimizations turned on.

## Performance Comparison vs Standrd Array Functions

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

### With 32bit Floats Vs Core Lib. Map function `(fun x -> x*x)`<a name="core32"></a>

       Method |  Length |            Median |         StdDev | Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------- |-------- |------------------ |--------------- |------ |------ |------- |------------------- |
 **SIMDContains** |      **10** |        **32.3354 ns** |      **0.0933 ns** |  **0.04** |     **-** |      **-** |              **22.80** |
     Contains |      10 |        13.0234 ns |      0.6457 ns |     - |     - |      - |               0.00 |
      SIMDMap |      10 |        37.3615 ns |      0.0693 ns |  0.09 |     - |      - |              53.95 |
          Map |      10 |        15.6651 ns |      0.2422 ns |  0.04 |     - |      - |              25.80 |
      SIMDSum |      10 |        19.3450 ns |      0.1866 ns |     - |     - |      - |               0.00 |
          Sum |      10 |         6.2273 ns |      0.2982 ns |     - |     - |      - |               0.00 |
      SIMDMax |      10 |        20.8972 ns |      0.7380 ns |     - |     - |      - |               0.00 |
          Max |      10 |         7.9275 ns |      0.9701 ns |     - |     - |      - |               0.00 |
 **SIMDContains** |     **100** |        **61.6295 ns** |      **5.0472 ns** |  **0.04** |     **-** |      **-** |              **24.92** |
     Contains |     100 |       140.9920 ns |      2.4739 ns |     - |     - |      - |               0.01 |
      SIMDMap |     100 |        75.8733 ns |      0.5875 ns |  0.33 |     - |      - |             192.40 |
          Map |     100 |       120.3029 ns |      0.4232 ns |  0.29 |     - |      - |             172.39 |
      SIMDSum |     100 |        32.0058 ns |      1.1225 ns |     - |     - |      - |               0.00 |
          Sum |     100 |        77.6100 ns |      2.4902 ns |     - |     - |      - |               0.00 |
      SIMDMax |     100 |        35.9042 ns |      2.0587 ns |     - |     - |      - |               0.00 |
          Max |     100 |        92.1754 ns |      9.6637 ns |     - |     - |      - |               0.00 |
 **SIMDContains** |    **1000** |       **417.0760 ns** |     **10.6672 ns** |     **-** |     **-** |      **-** |               **0.04** |
     Contains |    1000 |     1,333.0239 ns |     11.8959 ns |     - |     - |      - |               0.07 |
      SIMDMap |    1000 |       439.8549 ns |      7.5810 ns |  3.05 |     - |      - |           2,176.91 |
          Map |    1000 |     1,073.2894 ns |     16.1444 ns |  2.93 |     - |      - |           2,086.24 |
      SIMDSum |    1000 |       162.8308 ns |      5.8158 ns |     - |     - |      - |               0.01 |
          Sum |    1000 |       947.1124 ns |     14.4370 ns |     - |     - |      - |               0.07 |
      SIMDMax |    1000 |       167.0257 ns |      5.3584 ns |     - |     - |      - |               0.01 |
          Max |    1000 |       698.2252 ns |     21.2244 ns |     - |     - |      - |               0.03 |
 **SIMDContains** | **1000000** |   **427,765.2001 ns** |  **3,541.8344 ns** |     **-** |     **-** |   **0.23** |           **7,507.17** |
     Contains | 1000000 | 1,315,198.8375 ns | 19,634.6409 ns |     - |     - |   0.36 |          14,912.24 |
      SIMDMap | 1000000 | 1,747,002.9295 ns | 18,219.0807 ns |     - |     - | 519.18 |       1,198,305.57 |
          Map | 1000000 | 1,962,408.1761 ns | 23,319.8186 ns |     - |     - | 746.00 |       1,702,687.72 |
      SIMDSum | 1000000 |   160,972.7015 ns |  3,359.1696 ns |     - |     - |   0.05 |           1,960.97 |
          Sum | 1000000 |   955,224.0942 ns | 12,365.7613 ns |     - |     - |   0.38 |          14,853.87 |
      SIMDMax | 1000000 |   158,835.3746 ns |  3,773.1697 ns |     - |     - |   0.06 |           1,961.66 |
          Max | 1000000 |   633,761.7634 ns |  6,149.8767 ns |     - |     - |   0.24 |           7,495.76 |

### With 64bit Floats vs Core Lib. Map function `(fun x -> x*x+x)`<a name="core64"></a>

       Method |  Length |            Median |         StdDev | Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------- |-------- |------------------ |--------------- |------ |------ |------- |------------------- |
 **SIMDContains** |    **1000** |       **842.2604 ns** |     **36.6615 ns** |     **-** |     **-** |      **-** |               **0.13** |
     Contains |    1000 |     1,338.2032 ns |     21.7835 ns |     - |     - |      - |               0.13 |
      SIMDSum |    1000 |       302.8986 ns |     12.0417 ns |     - |     - |      - |               0.03 |
          Sum |    1000 |       953.9314 ns |      7.3770 ns |     - |     - |      - |               0.13 |
      SIMDMax |    1000 |       302.3690 ns |     11.8064 ns |     - |     - |      - |               0.03 |
          Max |    1000 |       713.9227 ns |     23.1721 ns |     - |     - |      - |               0.07 |
      SIMDMap |    1000 |       905.3396 ns |     21.1726 ns |  2.79 |     - |      - |           4,447.68 |
          Map |    1000 |     1,369.6668 ns |     17.1072 ns |  2.88 |     - |      - |           4,591.74 |
 **SIMDContains** |  **100000** |    **86,987.0417 ns** |    **212.5612 ns** |     **-** |     **-** |      **-** |             **204.08** |
     Contains |  100000 |   129,737.5287 ns |  2,300.6178 ns |     - |     - |      - |             398.91 |
      SIMDSum |  100000 |    30,836.7527 ns |     52.3596 ns |     - |     - |      - |             103.84 |
          Sum |  100000 |    97,310.6367 ns |    444.7469 ns |     - |     - |      - |             203.88 |
      SIMDMax |  100000 |    30,755.6959 ns |    189.2460 ns |     - |     - |      - |             103.84 |
          Max |  100000 |    65,190.8396 ns |    810.8605 ns |     - |     - |      - |             203.88 |
      SIMDMap |  100000 |   250,263.5686 ns | 23,822.3931 ns |     - |     - | 351.03 |         384,182.34 |
          Map |  100000 |   239,693.9435 ns | 20,283.1824 ns |     - |     - | 350.24 |         383,399.62 |
 **SIMDContains** | **1000000** |   **952,116.9191 ns** | **22,885.3666 ns** |     **-** |     **-** |   **0.17** |          **29,960.47** |
     Contains | 1000000 | 1,469,353.0761 ns | 44,872.5327 ns |     - |     - |   0.15 |          28,150.78 |
      SIMDSum | 1000000 |   493,523.5731 ns |  6,629.8292 ns |     - |     - |   0.12 |          15,020.79 |
          Sum | 1000000 | 1,059,862.2497 ns | 21,029.2608 ns |     - |     - |   0.17 |          29,921.97 |
      SIMDMax | 1000000 |   486,232.3883 ns |  3,963.6126 ns |     - |     - |   0.11 |          15,080.61 |
          Max | 1000000 |   771,554.3061 ns |  7,083.0659 ns |     - |     - |   0.12 |          15,008.20 |
      SIMDMap | 1000000 | 3,625,255.0307 ns | 40,939.9131 ns |     - |     - | 439.00 |       3,763,516.65 |
          Map | 1000000 | 3,490,854.2334 ns | 51,255.2300 ns |     - |     - | 413.00 |       3,589,365.95 |


### With 32bit Floats vs MathNET.Numerics managed. Map function `(fun x -> x*x+x)` <a name="mathnet"></a>

            Method |  Length |          Median |         StdDev | Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------ |-------- |---------------- |--------------- |------ |------ |------ |------------------- |
    **SIMDMapInPlace** |     **100** |      **46.5269 ns** |      **4.9229 ns** |  **0.08** |     **-** |     **-** |              **22.54** |
 MathNETMapInPlace |     100 |     354.0866 ns |      7.5375 ns |  0.36 |     - |     - |              99.59 |
           SIMDSum |     100 |      32.0283 ns |      2.9529 ns |     - |     - |     - |               0.00 |
        MathNETSum |     100 |      88.7532 ns |      1.9561 ns |     - |     - |     - |               0.00 |
    **SIMDMapInPlace** |    **1000** |     **165.7885 ns** |      **9.0778 ns** |     **-** |     **-** |     **-** |               **0.01** |
 MathNETMapInPlace |    1000 |   3,057.9378 ns |     56.8845 ns |  0.30 |     - |     - |              94.64 |
           SIMDSum |    1000 |     163.1672 ns |      6.7001 ns |     - |     - |     - |               0.01 |
        MathNETSum |    1000 |     962.2084 ns |     13.9839 ns |     - |     - |     - |               0.12 |
    **SIMDMapInPlace** |  **100000** |  **21,078.0491 ns** |    **627.8978 ns** |     **-** |     **-** |     **-** |              **56.61** |
 MathNETMapInPlace |  100000 | 104,831.7547 ns |  8,823.8473 ns |  5.26 |     - |     - |           2,267.50 |
           SIMDSum |  100000 |  15,134.0240 ns |    708.8177 ns |     - |     - |     - |              46.02 |
        MathNETSum |  100000 |  97,051.7780 ns |    875.9276 ns |     - |     - |     - |             217.82 |
    **SIMDMapInPlace** | **1000000** | **220,760.2212 ns** |  **7,167.1597 ns** |     **-** |     **-** |  **0.46** |           **7,402.18** |
 MathNETMapInPlace | 1000000 | 824,388.9221 ns | 47,134.8321 ns |     - |     - |  1.87 |          33,210.67 |
           SIMDSum | 1000000 | 159,887.6959 ns |  5,030.3486 ns |     - |     - |  0.18 |           3,433.93 |
        MathNETSum | 1000000 | 967,761.7422 ns | 17,557.1206 ns |     - |     - |  2.00 |          29,450.93 |

### With 32bit Floats vs MathNET.Numerics MKL Native. Adding two arrays <a name="mathnetnative"></a>
     Method |  Length |            Median |          StdDev | Gen 0 | Gen 1 |    Gen 2 | Bytes Allocated/Op |
----------- |-------- |------------------ |---------------- |------ |------ |--------- |------------------- |
   **SIMDMap2** |     **100** |        **92.1515 ns** |       **3.0304 ns** |  **2.70** |     **-** |        **-** |             **212.76** |
 MathNETAdd |     100 |       156.7522 ns |       7.3969 ns |  2.92 |     - |        - |             230.42 |
   **SIMDMap2** |    **1000** |       **493.5448 ns** |       **8.1340 ns** | **21.40** |     **-** |        **-** |           **2,048.32** |
 MathNETAdd |    1000 |       444.0753 ns |       5.9375 ns | 20.12 |     - |        - |           1,553.56 |
   **SIMDMap2** |  **100000** |   **161,024.7782 ns** |  **24,704.0627 ns** |     **-** |     **-** | **2,348.29** |         **197,602.33** |
 MathNETAdd |  100000 |   155,985.3149 ns |   1,478.0502 ns |     - |     - | 1,755.36 |         155,754.29 |
   **SIMDMap2** | **1000000** | **2,024,351.2170 ns** | **242,101.0167 ns** |     **-** |     **-** | **3,317.76** |       **2,025,584.78** |
 MathNETAdd | 1000000 | 1,551,270.9391 ns | 216,545.6630 ns |     - |     - | 2,466.00 |       1,693,319.93 |
