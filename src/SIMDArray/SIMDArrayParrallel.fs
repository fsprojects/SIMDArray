[<RequireQualifiedAccess>]
module Array.SIMDParallel

open System.Threading.Tasks
open SIMDArrayUtils
open System.Numerics

 
let inline iter vf (array : ^T[]) =
    checkNonNull array
    let count = Vector< ^T>.Count
    Parallel.ForStride 0 (array.Length-count+1) count (fun i -> let v = Vector< ^T>(array,i)
                                                                vf v                                                                
                                                      )