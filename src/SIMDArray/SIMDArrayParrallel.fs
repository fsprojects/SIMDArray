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