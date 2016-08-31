[<RequireQualifiedAccess>]
module Array.SIMDParallel

open System.Threading.Tasks
open SIMDArrayUtils
open System.Numerics

 
let inline iter vf sf (array : ^T[]) =
    checkNonNull array
    let count = Vector< ^T>.Count
    let len = array.Length
    Parallel.ForStride 0 (len-count+1) count (fun i -> vf (Vector< ^T>(array,i)))
                                                               
    let mutable i = len-len%count
    while i < array.Length do
        sf array.[i] 
        i <- i + 1                                
        
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
     