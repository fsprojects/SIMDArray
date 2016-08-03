# SIMDArray FSharp
SIMD enhanced Array operations for F#

## Example Usage

``` F#
open SIMDArray

//Faster map
let array = [| 1 .. 1000 |]
let squaredArray = array |> Array.SIMD.map (fun x -> x*x)

//Faster create and sum
let newArray = Array.SIMD.create 1000 5 //create a new array of length 1000 filled with 5
let sum = Array.SIMD.sum newArray

```

## Notes

Only 64 bit builds are supported.  Performance improvements will vary depending on your CPU architecture, width of Vector type, and the operations
you apply.  For small arrays the core libs may be faster due to increased fixed overhead for SIMD versions of the operations. To test
performance be sure to use Release builds with optimizations turned on.

