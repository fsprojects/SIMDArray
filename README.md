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

## Current supported functions


##### Array.SIMD.min

 Exactly like the standard Min function, only faster 


##### Array.SIMD.max

 Exactly like the standard Max function, only faster 


##### Array.SIMD.simpleExists

 Helper function to simplify when you just want to check for existence of a value directly. 


##### Array.SIMD.exists

 Checks for the existence of a value. You provide a function that takes a Vector and returns whether the condition you want exists in the Vector. 

##### Array.SIMD.mapInPlace

 Identical to the SIMDMap except the operation is done in place, and thus the resulting Vector type must be the same as the intial type. This will perform better when it can be used. 

##### Array.SIMD.map

 Identical to the standard map function, but you must provide A Vector mapping function. 


##### Array.SIMD.average

 Computes the average of the elements in the array 


##### Array.SIMD.sum

 Computes the sum of the elements in the array

##### Array.SIMD.init

 Similar to the built in init function but f will get called with every nth index, where n is the width of the vector, and you return a Vector. 

##### Array.SIMD.clear

 Sets a range of an array to the default value. 

##### Array.SIMD.create

 Creates an array filled with the value x. Only faster than Core lib create for larger width Vectors (byte, shorts etc) 

##### Array.SIMD.reduce

 A convenience function to call Fold with an acc of 0 

##### Array.SIMD.fold

 Similar to the standard Fold functionality but you must also provide a combiner function to combine each element of the Vector at the end. Not that acc can be double applied, this will not behave the same as fold. Typically 0 will be used for summing operations and 1 for multiplication. 


