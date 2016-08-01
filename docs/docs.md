## SIMDArray ##

##### M:Array.SIMD.min``1(``0[])

 Exactly like the standard Min function, only faster 

|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.max``1(``0[])

 Exactly like the standard Max function, only faster 

|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.simpleExists``1(``0,``0[])

 Helper function to simplify when you just want to check for existence of a value directly. 

|Name | Description |
|-----|------|
|x: ||
|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.exists``1(Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``0},System.Boolean},``0[])

 Checks for the existence of a value. You provide a function that takes a Vector and returns whether the condition you want exists in the Vector. 

|Name | Description |
|-----|------|
|f: |Takes a Vector and returns true or false to indicate existence|
|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.mapInPlace``1(Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``0},System.Numerics.Vector{``0}},``0[])

 Identical to the SIMDMap except the operation is done in place, and thus the resulting Vector type must be the same as the intial type. This will perform better when it can be used. 

|Name | Description |
|-----|------|
|f: |Mapping function that takes a Vector and returns a Vector of the same type|
|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.map``2(Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``0},System.Numerics.Vector{``1}},``0[])

 Identical to the standard map function, but you must provide A Vector mapping function. 

|Name | Description |
|-----|------|
|f: |A function that takes a Vector and returns a Vector. The returned vector does not have to be the same type|
|Name | Description |
|-----|------|
|array: |The source array|


---
##### M:Array.SIMD.average``1(``0[])

 Computes the average of the elements in the array 

|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.sum``1(``0[])

 Sums the elements of the array 

|Name | Description |
|-----|------|
|array: ||


---
##### M:Array.SIMD.init``1(System.Int32,Microsoft.FSharp.Core.FSharpFunc{System.Int32,System.Numerics.Vector{``0}})

 Similar to the built in init function but f will get called with every nth index, where n is the width of the vector, and you return a Vector. 

|Name | Description |
|-----|------|
|count: |How large to make the array|
|Name | Description |
|-----|------|
|f: |A function that accepts every Nth index and returns a Vector to be copied into the array|


---
##### M:Array.SIMD.clear``1(``0[],System.Int32,System.Int32)

 Sets a range of an array to the default value. 

|Name | Description |
|-----|------|
|array: |The array to clear|
|Name | Description |
|-----|------|
|index: |The starting index to clear|
|Name | Description |
|-----|------|
|length: |The number of elements to clear|


---
##### M:Array.SIMD.create``1(System.Int32,``0)

 Creates an array filled with the value x. Only faster than Core lib create for larger width Vectors (byte, shorts etc) 

|Name | Description |
|-----|------|
|count: |How large to make the array|
|Name | Description |
|-----|------|
|x: |What to fille the array with|


---
##### M:Array.SIMD.reduce``2(Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``0},Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``1},System.Numerics.Vector{``0}}},Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,``0}},``1[])

 A convenience function to call Fold with an acc of 0 

|Name | Description |
|-----|------|
|f: |The folding function|
|Name | Description |
|-----|------|
|combiner: |Function to combine the Vector elements at the end|
|Name | Description |
|-----|------|
|array: |Source array|


---
##### M:Array.SIMD.fold``2(Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``0},Microsoft.FSharp.Core.FSharpFunc{System.Numerics.Vector{``1},System.Numerics.Vector{``0}}},Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,``0}},``0,``1[])

 Similar to the standard Fold functionality but you must also provide a combiner function to combine each element of the Vector at the end. Not that acc can be double applied, this will not behave the same as fold. Typically 0 will be used for summing operations and 1 for multiplication. 

|Name | Description |
|-----|------|
|f: |The folding function|
|Name | Description |
|-----|------|
|combiner: |Function to combine the Vector elements at the end|
|Name | Description |
|-----|------|
|acc: |Initial value to accumulate from|
|Name | Description |
|-----|------|
|array: |Source array|


---
##### M:Array.SIMD.applyLeftovers``1(System.Int32,System.Numerics.Vector{``0},System.Numerics.Vector{``0})

 Applies the leftover Vector to the result vector, ignoring the padding 

|Name | Description |
|-----|------|
|count: ||
|Name | Description |
|-----|------|
|input: ||
|Name | Description |
|-----|------|
|result: ||


---
##### M:Array.SIMD.getLeftovers``1(``0[],System.Int32)

 Creates a vector based on remaining elements of the array that were not evenly divisible by the width of the vector, padding as necessary 

|Name | Description |
|-----|------|
|array: ||
|Name | Description |
|-----|------|
|curIndex: ||


---



