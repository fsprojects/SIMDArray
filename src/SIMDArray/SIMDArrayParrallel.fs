[<RequireQualifiedAccess>]
module Array.SIMDParallel


open SIMDArrayUtils
open System.Numerics

 
/// <summary>
/// Iterates over the array applying f to each Vector sized chunk
/// </summary>
/// <param name="f">Accepts a Vector</param>
/// <param name="array"></param>
let inline iter vf sf (array : ^T[]) =
    checkNonNull array
    let count = Vector< ^T>.Count
    let len = array.Length
    Parallel.ForStride 0 (len-count+1) count (fun i -> vf (Vector< ^T>(array,i)))
                                                               
    let mutable i = len-len%count
    while i < array.Length do
        sf array.[i] 
        i <- i + 1                                

/// <summary>
/// Iterates over the array applying f to each Vector sized chunk
/// along with the current index.
/// </summary>
/// <param name="f">Accepts the current index and associated Vector</param>
/// <param name="array"></param>
let inline iteri vf sf (array : ^T[]) =
    checkNonNull array
    let count = Vector< ^T>.Count
    let len = array.Length
    Parallel.ForStride 0 (len-count+1) count (fun i -> vf i (Vector< ^T>(array,i)))
                                                               
    let mutable i = len-len%count
    while i < array.Length do
        sf i array.[i] 
        i <- i + 1  

/// <summary>
/// Identical to the standard map function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="vf">A function that takes a Vector and returns a Vector. The returned vector
/// does not have to be the same type but must be the same width</param>
/// <param name="sf">A function to handle the leftover scalar elements if array is not divisible by Vector.count</param>
/// <param name="array">The source array</param>
let inline map
    (vf : ^T Vector -> ^U Vector) (sf : ^T -> ^U) (array : ^T[]) : ^U[] =

    checkNonNull array
    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Output type must have the same width as input type."    
    let len = array.Length
    let result = Array.zeroCreate array.Length
        
    Parallel.ForStride 0 len count 
        (fun i -> (vf (Vector< ^T>(array,i ))).CopyTo(result,i))
                    
    let mutable i = len-len%count
    while i < result.Length do
        result.[i] <- sf array.[i]
        i <- i + 1

    result


/// <summary>
/// Identical to the standard mapi function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes the current index and it's Vector and returns a Vector. The returned vector
/// does not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>
let inline mapi
    (vf :int -> ^T Vector -> ^U Vector) (sf : int -> ^T -> ^U) (array : ^T[]) : ^U[] =

    checkNonNull array
    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Output type must have the same width as input type."    
    let len = array.Length
    let result = Array.zeroCreate array.Length
        
    Parallel.ForStride 0 len count 
        (fun i -> (vf i (Vector< ^T>(array,i ))).CopyTo(result,i))
                    
    let mutable i = len-len%count
    while i < result.Length do
        result.[i] <- sf i array.[i]
        i <- i + 1

    result     


/// <summary>
/// Similar to the standard Fold functionality but you must also provide a combiner
/// function to combine each element of the Vector at the end. Not that acc
/// can be double applied, this will not behave the same as fold. Typically
/// 0 will be used for summing operations and 1 for multiplication.
/// </summary>
/// <param name="f">The folding function</param>
/// <param name="vcombiner">Function to combine the parallel Vector states when parallel process ends</param>
/// <param name="scombiner">Function to combine the elements of the final SIMD vector/param>
/// <param name="acc">Initial value to accumulate from</param>
/// <param name="array">Source array</param>
let inline fold
    (vf: ^State Vector -> ^T Vector -> ^State Vector)
    (sf : ^State -> ^T -> ^State)
    (vcombiner : ^State Vector-> ^State Vector-> ^State Vector)
    (scombiner : ^State -> ^State -> ^State)
    (acc : ^State)
    (array: ^T[]) : ^State =

    checkNonNull array
        
    let count = Vector< ^T>.Count
    let len = array.Length
    
    let mutable state = Vector< ^State> acc
    
    state <- Parallel.ForStrideAggreagate 0 len count state
                (fun i acc -> vf acc (Vector< ^T>(array,i)))
                (fun x acc -> vcombiner acc x)

    let mutable i = len-len%count
    let mutable result = acc
    while i < array.Length do
        result <- sf result array.[i]
        i <- i + 1
                   
    i <- 0    
    while i < Vector< ^State>.Count do
        result <- scombiner result state.[i]
        i <- i + 1
    result


/// <summary>
/// Sums the elements of the array
/// </summary>
/// <param name="array"></param>
let inline sum (array:^T[]) : ^T =

    checkNonNull array

    let mutable state = Vector< ^T>.Zero    
    let count = Vector< ^T>.Count
    let len = array.Length
        
    state <- Parallel.ForStrideAggreagate 0 len count state
                (fun i acc -> acc + (Vector< ^T>(array,i)))
                (fun x acc -> x + acc)

    let mutable result = Unchecked.defaultof< ^T>
    let mutable i = len-len%count
    while i < array.Length do
        result <- result + array.[i]
        i <- i + 1

    i <- 0
    while i < count do
        result <- result + state.[i]
        i <- i + 1
    result

/// <summary>
/// Sums the elements of the array by applying the function to each Vector of the array.
/// </summary>
/// <param name="array"></param>
let inline sumBy 
    (vf: Vector< ^T> -> Vector< ^U>) 
    (sf : ^T -> ^U) 
    (array:^T[]) : ^U =

    checkNonNull array
    
    let mutable state = Vector< ^U>.Zero    
    let count = Vector< ^T>.Count
    let len = array.Length
        
    state <- Parallel.ForStrideAggreagate 0 len count state
                (fun i acc -> acc + vf (Vector< ^T>(array,i)))
                (fun x acc -> x + acc)
    
    let mutable result = Unchecked.defaultof< ^U>    
    let mutable i = array.Length-array.Length%count
    while i < array.Length do
        result <- result + sf array.[i]
        i <- i + 1

    i <- 0
    while i < count do
        result <- result + state.[i]
        i <- i + 1
    result

/// <summary>
/// Computes the average of the elements in the array
/// </summary>
/// <param name="array"></param>
let inline average (array:^T[]) : ^T =
    let sum = sum array
    LanguagePrimitives.DivideByInt< ^T> sum array.Length
    

/// <summary>
/// Computes the average of the elements in the array by applying the function to
/// each Vector of the array
/// </summary>
/// <param name="array"></param>
let inline averageBy 
    (vf: Vector< ^T> -> Vector< ^U>) (sf: ^T -> ^U) (array:^T[]) : ^U =
    let sum = sumBy vf sf array
    LanguagePrimitives.DivideByInt< ^U> sum array.Length

