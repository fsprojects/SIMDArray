[<RequireQualifiedAccess>]
module Array.SIMD

open System
open System.Threading
open System.Numerics
open FSharp.Core



let inline private checkNonNull arg =
    match box arg with
    | null -> nullArg "array"
    | _ -> ()


/// <summary>
/// Similar to the standard Fold functionality but you must also provide a combiner
/// function to combine each element of the Vector at the end. Not that acc
/// can be double applied, this will not behave the same as fold. Typically
/// 0 will be used for summing operations and 1 for multiplication.
/// </summary>
/// <param name="f">The folding function</param>
/// <param name="combiner">Function to combine the Vector elements at the end</param>
/// <param name="acc">Initial value to accumulate from</param>
/// <param name="array">Source array</param>
let inline fold
    (vf: ^State Vector -> ^T Vector -> ^State Vector)
    (sf : ^State -> ^T -> ^State)
    (combiner : ^State -> ^State -> ^State)
    (acc : ^State)
    (array: ^T[]) : ^State =

    checkNonNull array
    
    let len = array.Length
    let count = Vector< ^T>.Count
    let lenLessCount = len-count

    let mutable state = Vector< ^State> acc
    let mutable i = 0    
    while i <= lenLessCount do
        state <- vf state (Vector< ^T>(array,i))
        i <- i + count

    let mutable result = acc
    while i < len do
        result <- sf result array.[i]
        i <- i + 1
                   
    i <- 0    
    while i < Vector< ^State>.Count do
        result <- combiner result state.[i]
        i <- i + 1
    result

/// <summary>
/// Similar to the standard Fold2 functionality but you must also provide a combiner
/// function to combine each element of the Vector at the end. Not that acc
/// can be double applied, this will not behave the same as fold. Typically
/// 0 will be used for summing operations and 1 for multiplication.
/// </summary>
/// <param name="f">The folding function</param>
/// <param name="combiner">Function to combine the Vector elements at the end</param>
/// <param name="acc">Initial value to accumulate from</param>
/// <param name="array">Source array</param>
let inline fold2
    (vf : ^State Vector -> ^T Vector -> ^U Vector -> ^State Vector)   
    (sf : ^State -> ^T -> ^U -> ^State)
    (combiner : ^State -> ^State -> ^State)
    (acc : ^State)
    (array1: ^T[])
    (array2: ^U[]) : ^State =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count then invalidArg "array" "Inputs and output must all have same Vector width."
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
            
    let lenLessCount = len-count

    let mutable state = Vector< ^State> acc
    let mutable i = 0    
    while i <= lenLessCount do
        state <- vf state (Vector< ^T>(array1,i)) (Vector< ^U>(array2,i))
        i <- i + count


    let mutable result = acc
    while i < len do
        result <- sf result array1.[i] array2.[i]
        i <- i + 1 
        
    i <- 0    
    while i < Vector< ^State>.Count do
        result <- combiner result state.[i]
        i <- i + 1
    result


/// <summary>
/// A convenience function to call Fold with an acc of 0
/// </summary>
/// <param name="f">The folding function</param>
/// <param name="combiner">Function to combine the Vector elements at the end</param>
/// <param name="array">Source array</param>
let inline reduce
    (vf: ^State Vector -> ^T Vector -> ^State Vector)
    (sf: ^State -> ^T -> ^State )
    (combiner : ^State -> ^State -> ^State)
    (array: ^T[]) : ^State =
    fold vf sf combiner Unchecked.defaultof< ^State> array


/// <summary>
/// Creates an array filled with the value x. 
/// </summary>
/// <param name="count">How large to make the array</param>
/// <param name="x">What to fille the array with</param>
let inline create (count :int) (x:^T) =
    
    if count < 0 then invalidArg "count" "The input must be non-negative."

    let array = Array.zeroCreate count
    let v = Vector< ^T> x
    let vCount = Vector< ^T>.Count
    let lenLessCount = count-vCount

    let mutable i = 0
    while i <= lenLessCount do
        v.CopyTo(array,i)
        i <- i + vCount

    while i < array.Length do
        array.[i] <- x
        i <- i + 1

    array



/// <summary>
/// Sets a range of an array to the default value.
/// </summary>
/// <param name="array">The array to clear</param>
/// <param name="index">The starting index to clear</param>
/// <param name="length">The number of elements to clear</param>
let inline clear (array : ^T[]) (index : int) (length : int) : unit =
    
    let v = Vector< ^T>.Zero
    let vCount = Vector< ^T>.Count
    let lenLessCount = length-vCount

    let mutable i = index
    while i <= lenLessCount do
        v.CopyTo(array,i)
        i <- i + vCount

    while i < length do
        array.[i] <- Unchecked.defaultof< ^T>
        i <- i + 1



/// <summary>
/// Similar to the built in init function but f will get called with every
/// nth index, where n is the width of the vector, and you return a Vector.
/// </summary>
/// <param name="count">How large to make the array</param>
/// <param name="f">A function that accepts every Nth index and returns a Vector to be copied into the array</param>
let inline init (count :int) (f : int -> Vector< ^T>)  =
    
    if count < 0 then invalidArg "count" "The input must be non-negative."
    
    let array = Array.zeroCreate count : ^T[]    
    let vCount = Vector< ^T>.Count
    let lenLessCount = count - vCount

    let mutable i = 0
    while i <= lenLessCount do
        (f i).CopyTo(array,i)
        i <- i + vCount

    let leftOvers = f i
    let mutable leftOverIndex = 0
    while i < count do
        array.[i] <- leftOvers.[leftOverIndex]
        leftOverIndex <- leftOverIndex + 1
        i <- i + 1

    array


/// <summary>
/// Sums the elements of the array
/// </summary>
/// <param name="array"></param>
let inline sum (array:^T[]) : ^T =

    checkNonNull array

    let mutable state = Vector< ^T>.Zero    
    let count = Vector< ^T>.Count
    let len = array.Length
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do
        state <-  state + Vector< ^T>(array,i)
        i <- i + count

    let mutable result = Unchecked.defaultof< ^T>
    while i < len do
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
let inline sumBy (vf: Vector< ^T> -> Vector< ^U>) (sf : ^T -> ^U) (array:^T[]) : ^U =

    checkNonNull array
    
    let mutable state = Vector< ^U>.Zero    
    let count = Vector< ^T>.Count
    let len = array.Length
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do
        state <-  state + vf (Vector< ^T>(array,i))
        i <- i + count
    
    let mutable result = Unchecked.defaultof< ^U>    

    while i < len do
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
let inline averageBy (vf: Vector< ^T> -> Vector< ^U>) (sf: ^T -> ^U) (array:^T[]) : ^U =
    let sum = sumBy vf sf array
    LanguagePrimitives.DivideByInt< ^U> sum array.Length



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
    let result = Array.zeroCreate len    
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do        
        (vf (Vector< ^T>(array,i ))).CopyTo(result,i)   
        i <- i + count
               
    
    while i < len do
        result.[i] <- sf array.[i]
        i <- i + 1

    result



/// <summary>
/// Identical to the standard map2 function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes two Vectors and returns a Vector. Both vectors and the
/// returned vector do not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>
let inline map2
    (vf : ^T Vector -> ^U Vector -> ^V Vector) 
    (sf : ^T -> ^U -> ^V)
    (array1 : ^T[]) 
    (array2 :^U[]) : ^V[] =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count || count <> Vector< ^V>.Count then invalidArg "array" "Inputs and output must all have same Vector width."
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"

    let result = Array.zeroCreate len

    let lenLessCount = len - count
    let mutable i = 0    
    while i <= lenLessCount do
        (vf (Vector< ^T>(array1,i )) (Vector< ^U>(array2,i))).CopyTo(result,i)   
        i <- i + count

    while i < len do
        result.[i] <- sf array1.[i] array2.[i]
        i <- i + 1

    result
    

/// <summary>
/// Identical to the standard map2 function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes three Vectors and returns a Vector. All vectors and the
/// returned vector do not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>


let inline map3
    (vf : ^T Vector -> ^U Vector -> ^V Vector -> ^W Vector) 
    (sf : ^T -> ^U -> ^V -> ^W)
    (array1 : ^T[]) (array2 :^U[]) (array3 :^V[]): ^W[] =

    checkNonNull array1
    checkNonNull array2
    checkNonNull array3

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count || count <> Vector< ^V>.Count || count <> Vector< ^W>.Count then invalidArg "array" "Inputs and output must all have same Vector wdith"
    
    let len = array1.Length        
    if len <> array2.Length || len <> array3.Length then invalidArg "array2" "Arrays must have same length"
    
    let result = Array.zeroCreate len
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        (vf (Vector< ^T>(array1,i )) (Vector< ^U>(array2,i)) (Vector< ^V>(array3,i))).CopyTo(result,i)        
        i <- i + count
    
    
    while i < len do
        result.[i] <- sf array1.[i] array2.[i] array3.[i]
        i <- i + 1
    

    result
/// <summary>
/// Identical to the standard mapi2 function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes two Vectors and an index 
/// and returns a Vector. All vectors must be the same width</param>
/// <param name="array">The source array</param>

let inline mapi2
    (vf : int -> ^T Vector -> ^U Vector -> ^V Vector) 
    (sf : int -> ^T -> ^U -> ^V)
    (array1 : ^T[]) (array2 :^U[]) : ^V[] =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count || count <> Vector< ^V>.Count then invalidArg "array" "Inputs and output must all have same Vector wdith"
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
    
    let result = Array.zeroCreate len
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        (vf i (Vector< ^T>(array1,i )) (Vector< ^U>(array2,i))).CopyTo(result,i)        
        i <- i + count
        
    while i < len do
        result.[i] <- sf i array1.[i] array2.[i]
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
    (vf : int -> ^T Vector -> ^U Vector) 
    (sf: int -> ^T -> ^U)
    (array : ^T[]) : ^U[] =

    checkNonNull array
    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Output type must have the same width as input type."
    
    let len = array.Length    
    let result = Array.zeroCreate len    
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        (vf i (Vector< ^T>(array,i ))).CopyTo(result,i)                
        i <- i + count
        
    while i < len do
        result.[i] <- sf i array.[i]
        i <- i + 1
    
    result

/// <summary>
/// Iterates over the array applying f to each Vector sized chunk
/// </summary>
/// <param name="f">Accepts a Vector</param>
/// <param name="array"></param>
let inline iter
    (vf : Vector< ^T> -> unit) 
    (sf : ^T -> unit) 
    (array : ^T[]) : unit  =

    checkNonNull array
        
    let len = array.Length        
    let count = Vector< ^T>.Count
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        vf (Vector< ^T>(array,i ))
        i <- i + count
     
    while i < len do
        sf array.[i]
        i <- i + 1
    

/// <summary>
/// Iterates over the two arrays applying f to each Vector pair
/// </summary>
/// <param name="f">Accepts two Vectors</param>
/// <param name="array"></param>
let inline iter2 
    (vf : Vector< ^T> -> Vector< ^U> -> unit)
    (sf : ^T -> ^U -> unit)
    (array1: ^T[]) (array2: ^U[]) : unit =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count then invalidArg "array" "Inputs and output must all have same Vector width."
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do 
        vf (Vector< ^T>(array1,i)) (Vector< ^U>(array2,i))
        i <- i + count

    while i < len do
        sf array1.[i] array2.[i]
        i <- i + 1
    

/// <summary>
/// Iterates over the array applying f to each Vector sized chunk
/// along with the current index.
/// </summary>
/// <param name="f">Accepts the current index and associated Vector</param>
/// <param name="array"></param>
let inline iteri
    (vf : int -> Vector< ^T> -> unit)
    (sf : int -> ^T -> unit)
    (array : ^T[]) : unit  =

    checkNonNull array
         
    let len = array.Length        
    let count = Vector< ^T>.Count
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        vf i (Vector< ^T>(array,i ))
        i <- i + count

    while i < len do
        sf i array.[i]
        i <- i + 1
        
    

/// <summary>
/// Iterates over the two arrays applying f to each Vector pair
/// and their current index.
/// </summary>
/// <param name="f">Accepts two Vectors</param>
/// <param name="array"></param>
let inline iteri2 
    (vf : int -> Vector< ^T> -> Vector< ^U> -> unit)
    (sf : int -> ^T -> ^U -> unit)
    (array1: ^T[]) (array2: ^U[]) : unit =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count then invalidArg "array" "Inputs and output must all have same Vector width."
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do 
        vf i (Vector< ^T>(array1,i)) (Vector< ^U>(array2,i))
        i <- i + count

    while i < len do
        sf i array1.[i] array2.[i]
        i <- i + 1
    
/// <summary>
/// Identical to the SIMDMap except the operation is done in place, and thus
/// the resulting Vector type must be the same as the intial type. This will
/// perform better when it can be used.
/// </summary>
/// <param name="f">Mapping function that takes a Vector and returns a Vector of the same type</param>
/// <param name="array"></param>

let inline mapInPlace
    ( vf : ^T Vector -> ^T Vector) 
    ( sf : ^T -> ^T )
    (array: ^T[]) : unit =

    checkNonNull array

    let len = array.Length
    let count = Vector< ^T>.Count
    let lenLessCount = len - count

    let mutable i = 0
    while i <= lenLessCount do
        (vf (Vector< ^T>(array,i ))).CopyTo(array,i)   
        i <- i + count
               
    while i < len do
        array.[i] <- sf array.[i]
        i <- i + 1
  
/// <summary>
/// Takes a function that accepts a vector and returns true or false. Returns the first Vector Option
/// that returns true or None if none match. 
/// </summary>
/// <param name="f">Takes a Vector and returns true or false</param>
/// <param name="array"></param>
let inline tryFindVector 
    (f : ^T Vector -> bool)  (array: ^T[]) : Vector< ^T> Option =

    checkNonNull array

    let count = Vector< ^T>.Count
    let mutable found = false
    let len = array.Length
    let lenLessCount = len-count
        
    let mutable i = 0
    while i <= lenLessCount && not found do
        found <- f (Vector< ^T>(array,i))
        i <- i + count

    if found then
        Some (Vector< ^T>(array,i-count))
    else
        None


/// <summary>
/// Takes a function that accepts a vector and returns true or false. Returns the first Vector that 
/// returns true, and then extracts the desired value with extractor or null if none is found.
/// Leftover array elements are ignored.
/// </summary>
/// <param name="finder">Takes a Vector and returns true or false</param>
/// <param name="extractor">Takes a vector and extracts the desied value from it</param>
/// <param name="array"></param>
let inline find (finder : ^T Vector -> bool) (extractor : ^T Vector -> ^T) (array: ^T[]) : ^T =

    let v = tryFindVector finder array
    match v with
    | Some v -> extractor v
    | None -> null
    
/// <summary>
/// Takes a function that accepts a vector and returns true or false. Returns the first Vector Option that 
/// returns true, and then extracts the desired value with extractor or returns None if not found. Leftover
/// array elements are ignored.
/// </summary>
/// <param name="finder">Takes a Vector and returns true or false</param>
/// <param name="extractor">Takes a vector and extracts the desied value Option from it</param>
/// <param name="array"></param>
let inline tryFind (finder : ^T Vector -> bool) (extractor : ^T Vector -> ^T Option) (array: ^T[]) : ^T Option =
 
    let v = tryFindVector finder array
    match v with
    | Some v -> extractor v
    | None -> None


          
/// <summary>
/// Checks for the existence of a value satisfying the Vector predicate. 
/// </summary>
/// <param name="f">Takes a Vector and returns true or false to indicate existence</param>
/// <param name="array"></param>
let inline exists 
    (vf : ^T Vector -> bool) 
    (sf : ^T -> bool)
    (array: ^T[]) : bool =
    
    checkNonNull array

    let count = Vector< ^T>.Count
    let mutable found = false
    let len = array.Length
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do
        found <- vf (Vector< ^T>(array,i))
        if found then i <- len
        else i <- i + count

    while i < len && not found do
        found <- sf array.[i]
        i <- i + 1

    found

/// <summary>
/// Checks if all Vectors satisfy the predicate.
/// </summary>
/// <param name="f">Takes a Vector and returns true or false</param>
/// <param name="array"></param>
let inline forall (f : ^T Vector -> bool) (array: ^T[]) : bool =
    
    checkNonNull array

    let count = Vector< ^T>.Count
    let mutable found = true
    let len = array.Length
    let lenLessCount = len-count

    let mutable i = 0
    while i <= lenLessCount do
        found <- f (Vector< ^T>(array,i))
        if not found then i <- len
        else i <- i + count

    if i < len then
        let leftOverArray = Array.zeroCreate count
        for j=0 to leftOverArray.Length-1 do
            if i < len then
                leftOverArray.[j] <- array.[i]
                i <- i + 1
            else
                leftOverArray.[j] <- array.[len-1] //just repeat the last item
            
        found <- f (Vector< ^T> leftOverArray)

    found


/// <summary>
/// Checks for the existence of a pair of values satisfying the Vector predicate. 
/// </summary>
/// <param name="f">Takes two Vectors and returns true or false to indicate existence</param>
/// <param name="array"></param>
let inline exists2 
    (vf : ^T Vector -> ^U Vector -> bool) 
    (sf : ^T -> ^U -> bool)
    (array1: ^T[]) (array2: ^U[]) : bool =
    
    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Arrays must have same Vector width"
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
         
    let lenLessCount = len-count

    let mutable found = false
    let mutable i = 0
    while i <= lenLessCount do
        found <- vf (Vector< ^T>(array1,i)) (Vector< ^U>(array2,i))
        if found then i <- len
        else i <- i + count

    while i < len && not found do
        found <- sf array1.[i] array2.[i]
        i <- i + 1

    found

/// <summary>
/// Checks for the if all Vector pairs satisfy the predicate
/// </summary>
/// <param name="f">Takes two Vectors and returns true or false to indicate existence</param>
/// <param name="array"></param>
let inline forall2 (f : ^T Vector -> ^U Vector -> bool) (array1: ^T[]) (array2: ^U[]) : bool =
    
    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Arrays must have same Vector width"
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
         
    let lenLessCount = len-count

    let mutable found = true
    let mutable i = 0
    while i <= lenLessCount do
        found <- f (Vector< ^T>(array1,i)) (Vector< ^U>(array2,i))
        if not found then i <- len
        else i <- i + count

    if i < len then
        let leftOverArray1 = Array.zeroCreate count
        let leftOverArray2 = Array.zeroCreate count
        for j=0 to leftOverArray1.Length-1 do
            if i < len then
                leftOverArray1.[j] <- array1.[i]
                leftOverArray2.[j] <- array2.[i]
                i <- i + 1
            else
                leftOverArray1.[j] <- array1.[len-1] //just repeat the last item
                leftOverArray2.[j] <- array2.[len-1] //just repeat the last item
            
        found <- f (Vector< ^T> leftOverArray1) (Vector< ^U> leftOverArray2)

    found


/// <summary>
/// Identical to the standard contains, just faster
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline contains (x : ^T) (array:^T[]) : bool =
    
    checkNonNull array

    let count = Vector< ^T>.Count      
    let len = array.Length
    let lenLessCount = len - count    
    let compareVector = Vector< ^T>(x)    
    
    let mutable found = false
    let mutable i = 0
    while i <= lenLessCount do
        found <- Vector.EqualsAny(Vector< ^T>(array,i),compareVector)
        if found then i <- len
        else i <- i + count

    while i < len && not found do                
        found <- x = array.[i]
        i <- i + 1

    found


/// <summary>
/// Exactly like the standard Max function, only faster
/// </summary>
/// <param name="array"></param>
let inline max (array :^T[]) : ^T =

    checkNonNull array

    let len = array.Length
    if len = 0 then invalidArg "array" "The input array was empty."
    let mutable max = array.[0]
    let count = Vector< ^T>.Count
    let lenLessCount = len-count

    let mutable i = 0
    if len >= count then
        let mutable maxV = Vector< ^T>(array,0)
        i <- i + count
        while i <= lenLessCount do
           let v = Vector< ^T>(array,i)
           maxV <- Vector.Max(v,maxV)
           i <- i + count

        for j=0 to count-1 do
            if maxV.[j] > max then max <- maxV.[j]

    while i < len do
        if array.[i] > max then max <- array.[i]
        i <- i + 1
    max

/// <summary>
/// Find the max by applying the function to each Vector in the array
/// </summary>
/// <param name="array"></param>
let inline maxBy 
    (vf: Vector< ^T> -> Vector< ^U>) 
    (sf: ^T -> ^U)
    (array :^T[]) : ^U =
    
    checkNonNull array

    let len = array.Length
    if len = 0 then invalidArg "array" "The input array was empty."    
    let count = Vector< ^T>.Count
    let lenLessCount = len-count
    let minValue = typeof< ^U>.GetField("MinValue").GetValue() |> unbox< ^U>
    let mutable max = minValue 
    let mutable maxV =  Vector< ^U>(minValue)
    let mutable i = 0
    if len >= count then
        maxV  <- vf (Vector< ^T>(array,0))
        max <- maxV.[0]
        i <- i + count
        while i <= lenLessCount do
            let v = vf (Vector< ^T>(array,i))
            maxV <- Vector.Max(v,maxV)
            i <- i + count                
    
    for j=0 to Vector< ^U>.Count-1 do
        if maxV.[j] > max then max <- maxV.[j]

    while i < len do
        let x = sf array.[i]
        if x > max then max <- x
        i <- i + 1
    
    max

        
/// <summary>
/// Find the min by applying the function to each Vector in the array
/// </summary>
/// <param name="array"></param>
let inline minBy 
    (vf: Vector< ^T> -> Vector< ^U>) 
    (sf: ^T -> ^U)
    (array :^T[]) : ^U =

    checkNonNull array
            
    let len = array.Length
    if len = 0 then invalidArg "array" "The input array was empty."    
    let count = Vector< ^T>.Count
    let lenLessCount = len-count
    let maxValue = typeof< ^U>.GetField("MaxValue").GetValue() |> unbox< ^U>
    let mutable min = maxValue 
    let mutable minV =  Vector< ^U>(maxValue)
    let mutable i = 0
    if len >= count then
        minV  <- vf (Vector< ^T>(array,0))
        min <- minV.[0]
        i <- i + count
        while i <= lenLessCount do
            let v = vf (Vector< ^T>(array,i))
            minV <- Vector.Min(v,minV)
            i <- i + count        
    
    for j=0 to Vector< ^U>.Count-1 do
        if minV.[j] < min then min <- minV.[j]

    while i < len do
        let x = sf array.[i]
        if x < min then min <- x
        i <- i + 1
    
    min


/// <summary>
/// Exactly like the standard Min function, only faster
/// </summary>
/// <param name="array"></param>
let inline min (array :^T[]) : ^T =

    checkNonNull array

    let len = array.Length
    if len = 0 then invalidArg "array" "empty array"
    let mutable min = array.[0]
    let count = Vector< ^T>.Count
    let lenLessCount = len-count

    let mutable i = 0
    if len >= count then
        let mutable minV = Vector< ^T>(array,0)
        i <- i + count
        while i <= lenLessCount do
            let v = Vector< ^T>(array,i)
            minV <- Vector.Min(v,minV)
            i <- i + count

        for j=0 to count-1 do
            if minV.[j] < min then min <- minV.[j]

    while i < len do
        if array.[i] < min then min <- array.[i]
        i <- i + 1
    min




