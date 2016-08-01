# SIMDArray FSharp
SIMD enhanced Array operations for F#

### Usage

``` F#
open SIMDArray

//Faster map
let array = [| 1 .. 1000 |]
let squaredArray = array |> Array.SIMD.map (fun x -> x*x)

//Faster create and sum
let newArray = Array.SIMD.create 1000 5 //create a new array of length 1000 filled with 5
let sum = Array.SIMD.sum newArray

```

