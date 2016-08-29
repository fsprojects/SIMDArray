[<RequireQualifiedAccess>]
module Array.Performance


open FSharp.Core
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.Operators
open System.Collections.Generic

let inline private checkNonNull arg =
    match box arg with
    | null -> nullArg "array"
    | _ -> ()


/// <summary>
/// Like Array.partition but results do not maintain order, thus slightly faster
/// </summary>
/// <param name="f"></param>
/// <param name="array"></param>
let inline partitionUnordered f (array: _[]) = 
    checkNonNull array
    let res = Array.zeroCreate array.Length        
    let mutable upCount = 0
    let mutable downCount = array.Length-1    
    for x in array do                
        if f x then 
            res.[upCount] <- x
            upCount <- upCount + 1
        else
            res.[downCount] <- x
            downCount <- downCount - 1
                            
    Array.sub res 0 upCount , Array.sub res upCount (array.Length-upCount)

/// <summary>
/// Like Array.distinct but results do not maintain order, improving runtime
/// slightly and allocations greatly.
/// </summary>
/// <param name="array"></param>
let inline distinctUnordered (array:'T[]) =
    checkNonNull array
                        
    let hashSet = HashSet<'T>(HashIdentity.Structural<'T>)
    for v in array do 
        hashSet.Add(v) |> ignore
                    
    let res = Array.zeroCreate hashSet.Count
    hashSet.CopyTo(res)
    res

/// <summary>
/// Like Array.distinctBy but results do not maintain order, improving runtime
/// slightly and allocations greatly.
/// </summary>
/// <param name="array"></param>
let inline distinctByUnordered keyf (array:'T[]) =
    checkNonNull array
        
    let hashSet = HashSet<_>(HashIdentity.Structural<_>)
    for v in array do
        hashSet.Add(keyf v) |> ignore

    let res = Array.zeroCreate hashSet.Count
    hashSet.CopyTo(res)
    res
        
/// <summary>
/// Performs a map operation in place. If you don't need the old array,
/// Save yourself time and GC pressure!
/// </summary>
/// <param name="f"></param>
/// <param name="array"></param>
let inline mapInPlace f (array :'T[]) =
    
    checkNonNull array

    for i = 0 to array.Length-1 do
        array.[i] <- f array.[i]

    ()

/// <summary>
/// Much faster and less allocation for sufficiently simple, pure predicates.
/// Predicates are computed twice to avoid allocating an array.
/// </summary>
/// <param name="f">Predicate to fitler with</param>
/// <param name="array"></param>
let inline filterSimplePredicate (f: ^T -> bool) (array: ^T[]) = 
    
    checkNonNull array
    if array.Length = 0 then invalidArg "array" "Array can not be empty."    
        
    let mutable count = 0

    for i = 0 to array.Length-1 do
        if f array.[i] then
            count <- count + 1
                    
    let result = Array.zeroCreate count
    let mutable j = 0
    for i = 0 to array.Length-1 do
        if f array.[i] then
            result.[j] <- array.[i]
            j <- j + 1
    result

/// <summary>
/// Much faster and less allocation for sufficiently simple, pure predicates.
/// Predicates are computed twice to avoid allocating an array.
/// </summary>
/// <param name="f">Predicate to fitler with</param>
/// <param name="array"></param>
let inline whereSimplePredicate (f: ^T -> bool) (array: ^T[]) = 
    filterSimplePredicate f array


/// <summary>
/// Returns an array with only elements equal to x.
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline filterEqual (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e = x) array

let inline whereEqual (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e = x) array


/// <summary>
/// Returns an array with only elements not equal to x.
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline filterNotEqual (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e <> x) array

let inline whereNotEqual (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e <> x) array

/// <summary>
/// Returns an array with only elements greater than x.
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline filterGreaterThan (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e > x) array

let inline whereGreaterThan (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e > x) array

/// <summary>
/// Returns an array with only elements less than x.
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline filterLessThan (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e < x) array

let inline whereLessThan (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e < x) array

/// <summary>
/// Returns an array with only elements greater than or equal to x.
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline filterGEq (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e >= x) array

let inline whereGEq (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e >= x) array

/// <summary>
/// Returns an array with only elements less than or equal to x.
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline filterLEq (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e <= x) array

let inline whereLEq (x : ^T) (array : ^T[]) = 
    filterSimplePredicate (fun e -> e <= x) array


