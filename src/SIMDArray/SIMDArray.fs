[<RequireQualifiedAccess>]
module Array.SIMD

open System.Numerics
open FSharp.Core


let inline private checkNonNull arg =
    match box arg with
    | null -> nullArg "array"
    | _ -> ()

/// <summary>
/// Creates a vector based on remaining elements of the array that were not
/// evenly divisible by the width of the vector, padding as necessary
/// </summary>
/// <param name="array"></param>
/// <param name="curIndex"></param>
let inline private getLeftovers (array: ^T []) (curIndex: int) : ^T Vector =
    let mutable vi = curIndex
    let d = Unchecked.defaultof< ^T>
    let leftOverArray =
        [| for i=1 to Vector< ^T>.Count do
            if vi < array.Length then
                yield array.[vi]
                vi <- vi + 1
            else
                yield d
        |]
    Vector< ^T> leftOverArray

/// <summary>
/// Applies the leftover Vector to the result vector, ignoring the padding
/// </summary>
/// <param name="count"></param>
/// <param name="input"></param>
/// <param name="result"></param>
let inline private applyLeftovers (count: int) (input: ^T Vector) (result: ^T Vector) =
    let vCount = Vector< ^T>.Count
    let newArray = Array.zeroCreate vCount
    for i=0 to vCount-1 do
        if i < count then
            newArray.[i] <- input.[i]
        else
            newArray.[i] <- result.[i]
    Vector< ^T> newArray


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
    (f: ^State Vector -> ^T Vector -> ^State Vector)
    (combiner : ^State -> ^State -> ^State)
    (acc : ^State)
    (array: ^T[]) : ^State =

    checkNonNull array
    let len = array.Length
    let mutable state = Vector< ^State> acc
    let mutable vi = 0
    let count = Vector< ^T>.Count
    while vi <= len - count do
        state <- f state (Vector< ^T>(array,vi))
        vi <- vi + count

    let leftoverCount = len - vi
    if leftoverCount <> 0 then
        let leftOver = f state (getLeftovers array vi )
        state <- applyLeftovers leftoverCount leftOver state

    vi <- 0
    let mutable result = acc
    while vi < Vector< ^State>.Count do
        result <- combiner result state.[vi]
        vi <- vi + 1
    result


/// <summary>
/// A convenience function to call Fold with an acc of 0
/// </summary>
/// <param name="f">The folding function</param>
/// <param name="combiner">Function to combine the Vector elements at the end</param>
/// <param name="array">Source array</param>
let inline reduce
    (f: ^State Vector -> ^T Vector -> ^State Vector)
    (combiner : ^State -> ^State -> ^State)
    (array: ^T[]) : ^State =
    fold f combiner Unchecked.defaultof< ^State> array


/// <summary>
/// Creates an array filled with the value x. Only faster than
/// Core lib create for larger width Vectors (byte, shorts etc)
/// </summary>
/// <param name="count">How large to make the array</param>
/// <param name="x">What to fille the array with</param>
let inline create (count :int) (x:^T) =
    //if count < 0 then invalidArg "count" "The input must be non-negative."
    let array = Array.zeroCreate count : ^T[]
    let mutable i = 0
    let v = Vector< ^T> x
    let vCount = Vector< ^T>.Count
    while i < count - vCount do
        v.CopyTo(array,i)
        i <- i + vCount

    while i < count do
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
    let mutable i = index
    let v = Vector< ^T> Unchecked.defaultof< ^T>
    let vCount = Vector< ^T>.Count
    while i < length - vCount do
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
    let mutable i = 0
    let vCount = Vector< ^T>.Count
    while i < count - vCount do
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
    let mutable vi = 0
    let count = Vector< ^T>.Count
    while vi <= array.Length - count do
        state <-  state + Vector< ^T>(array,vi)
        vi <- vi + count

    let mutable result = Unchecked.defaultof< ^T>
    while vi < array.Length do
        result <- result + array.[vi]
        vi <- vi + 1

    vi <- 0
    while vi < count do
        result <- result + state.[vi]
        vi <- vi + 1
    result


/// <summary>
/// Computes the average of the elements in the array
/// </summary>
/// <param name="array"></param>
let inline average (array:^T[]) : ^T =
    let sum = sum array
    LanguagePrimitives.DivideByInt< ^T> sum array.Length


/// <summary>
/// Identical to the standard map function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes a Vector and returns a Vector. The returned vector
/// does not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>

let inline map
    (f : ^T Vector -> ^U Vector) (array : ^T[]) : ^U[] =

    checkNonNull array
    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Output type must have the same width as input type."
    let len = array.Length    
    let result = Array.zeroCreate len
    
    let lenLessCount = len-count

    let rec loop i = 
        if i <= lenLessCount then
            (f (Vector< ^T>(array,i ))).CopyTo(result,i)   
            loop (i+count)
       
    loop 0
    
    let mutable i = len - len % count
    if i < len then 
        let leftOver = len - i
        let leftOverArray = Array.init count (fun x -> if x < leftOver then 
                                                        array.[x+i]
                                                       else
                                                        Unchecked.defaultof< ^T>)
        let v = f (Vector< ^T>(leftOverArray,0))
        let mutable j = 0
        while i < len do
            result.[i] <- v.[j]
            i <- i + 1
            j <- j + 1

    result



/// <summary>
/// Identical to the standard map2 function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes two Vectors and returns a Vector. Both vectors and the
/// returned vector do not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>

let inline map2
    (f : ^T Vector -> ^U Vector -> ^V Vector) (array1 : ^T[]) (array2 :^U[]) : ^V[] =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count || count <> Vector< ^V>.Count then invalidArg "array" "Inputs and output must all have same Vector wdith"
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"
    
    let result = Array.zeroCreate len
    let lenLessCount = len-count

    let rec loop i = 
        if i <= lenLessCount then
            (f (Vector< ^T>(array1,i )) (Vector< ^U>(array2,i))).CopyTo(result,i)   
            loop (i+count)
       
    loop 0
    
    let mutable i = len - len % count
    if i < len then 
        let leftOver = len - i
        let leftOverArray1 = Array.init count (fun x -> if x < leftOver then 
                                                          array1.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^T>)
        let leftOverArray2 = Array.init count (fun x -> if x < leftOver then 
                                                          array2.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^U>)
        let v = f (Vector< ^T>(leftOverArray1,0 )) (Vector< ^U>(leftOverArray2,0))
        let mutable j = 0
        while i < len do
            result.[i] <- v.[j]
            i <- i + 1
            j <- j + 1

    result
/// <summary>
/// Identical to the standard map2 function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes three Vectors and returns a Vector. All vectors and the
/// returned vector do not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>

let inline map3
    (f : ^T Vector -> ^U Vector -> ^V Vector -> ^W Vector) (array1 : ^T[]) (array2 :^U[]) (array3 :^V[]): ^W[] =

    checkNonNull array1
    checkNonNull array2
    checkNonNull array3

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count || count <> Vector< ^V>.Count || count <> Vector< ^W>.Count then invalidArg "array" "Inputs and output must all have same Vector wdith"
    
    let len = array1.Length        
    if len <> array2.Length || len <> array3.Length then invalidArg "array2" "Arrays must have same length"

    let f = OptimizedClosures.FSharpFunc<_,_,_,_>.Adapt(f)

    let result = Array.zeroCreate len
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        f.Invoke(Vector< ^T>(array1,i ),Vector< ^U>(array2,i),Vector< ^V>(array3,i)).CopyTo(result,i)        
        i <- i + count
    
    
    if i < len then 
        let leftOver = len - i
        let leftOverArray1 = Array.init count (fun x -> if x < leftOver then 
                                                          array1.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^T>)
        let leftOverArray2 = Array.init count (fun x -> if x < leftOver then 
                                                          array2.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^U>)
        let leftOverArray3 = Array.init count (fun x -> if x < leftOver then 
                                                          array3.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^V>)

        let v = f.Invoke(Vector< ^T>(leftOverArray1,0),Vector< ^U>(leftOverArray2,0),Vector< ^V>(leftOverArray3,0))
        let mutable j = 0
        while i < len do
            result.[i] <- v.[j]
            i <- i + 1
            j <- j + 1

    result

/// <summary>
/// Identical to the standard mapi2 function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes two Vectors and an index 
/// and returns a Vector. All vectors must be the same width</param>
/// <param name="array">The source array</param>

let inline mapi2
    (f : int -> ^T Vector -> ^U Vector -> ^V Vector) (array1 : ^T[]) (array2 :^U[]) : ^V[] =

    checkNonNull array1
    checkNonNull array2

    let count = Vector< ^T>.Count    
    if count <> Vector< ^U>.Count || count <> Vector< ^V>.Count then invalidArg "array" "Inputs and output must all have same Vector wdith"
    
    let len = array1.Length        
    if len <> array2.Length then invalidArg "array2" "Arrays must have same length"

    let f = OptimizedClosures.FSharpFunc<_,_,_,_>.Adapt(f)

    let result = Array.zeroCreate len
    let lenLessCount = len-count

    let mutable i = 0    
    while i <= lenLessCount do
        f.Invoke(i,Vector< ^T>(array1,i ),Vector< ^U>(array2,i)).CopyTo(result,i)        
        i <- i + count
    
    
    if i < len then 
        let leftOver = len - i
        let leftOverArray1 = Array.init count (fun x -> if x < leftOver then 
                                                          array1.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^T>)
        let leftOverArray2 = Array.init count (fun x -> if x < leftOver then 
                                                          array2.[x+i]
                                                        else
                                                          Unchecked.defaultof< ^U>)
        let v = f.Invoke(i,Vector< ^T>(leftOverArray1,0),Vector< ^U>(leftOverArray2,0) )
        let mutable j = 0
        while i < len do
            result.[i] <- v.[j]
            i <- i + 1
            j <- j + 1

    result

/// <summary>
/// Identical to the standard mapi function, but you must provide
/// A Vector mapping function.
/// </summary>
/// <param name="f">A function that takes the current index and it's Vector and returns a Vector. The returned vector
/// does not have to be the same type but must be the same width</param>
/// <param name="array">The source array</param>

let inline mapi
    (f : int -> ^T Vector -> ^U Vector) (array : ^T[]) : ^U[] =

    checkNonNull array
    let count = Vector< ^T>.Count
    if count <> Vector< ^U>.Count then invalidArg "array" "Output type must have the same width as input type."
    let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(f)
    let len = array.Length    
    let result = Array.zeroCreate len
    
    let lenLessCount = len-count

    let mutable i = 0
    
    while i <= lenLessCount do
        f.Invoke(i,(Vector< ^T>(array,i ))).CopyTo(result,i)                
        i <- i + count
    
    
    if i < len then 
        let leftOver = len - i
        let leftOverArray = Array.init count (fun x -> if x < leftOver then 
                                                        array.[x+i]
                                                       else
                                                        Unchecked.defaultof< ^T>)
        let v = f.Invoke(i, (Vector< ^T>(leftOverArray,0)))
        let mutable j = 0
        while i < len do
            result.[i] <- v.[j]
            i <- i + 1
            j <- j + 1

    result

/// <summary>
/// Iterates over the array applying f to each Vector sized chunk
/// Returns the number of leftover array elements so the caller
/// can deal with them
/// </summary>
/// <param name="f">Accepts a Vector</param>
/// <param name="array"></param>
let inline iter
    f (array : ^T[]) : int  =

    checkNonNull array
        
    let len = array.Length        
    let count = Vector< ^T>.Count
    
    let mutable i = 0    
    while i <= (len-count) do
        f (Vector< ^T>(array,i ))
        i <- i + count
        
    len-i

/// <summary>
/// Iterates over the array applying f to each Vector sized chunk
/// along with the current index.
/// Returns the number of leftover array elements so the caller
/// can deal with them
/// </summary>
/// <param name="f">Accepts the current index and associated Vector</param>
/// <param name="array"></param>
let inline iteri
    f (array : ^T[]) : int  =

    checkNonNull array
     
    let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(f)
    let len = array.Length        
    let count = Vector< ^T>.Count
    
    let mutable i = 0    
    while i <= (len-count) do
        f.Invoke(i,(Vector< ^T>(array,i )))
        i <- i + count
        
    len-i

/// <summary>
/// Identical to the SIMDMap except the operation is done in place, and thus
/// the resulting Vector type must be the same as the intial type. This will
/// perform better when it can be used.
/// </summary>
/// <param name="f">Mapping function that takes a Vector and returns a Vector of the same type</param>
/// <param name="array"></param>
let inline mapInPlace
    ( f : ^T Vector -> ^T Vector) (array: ^T[]) : unit =

    checkNonNull array

    let len = array.Length
    let count = Vector< ^T>.Count

    let mutable i = 0
    while i <= len - count do
        (f (Vector< ^T>(array,i))).CopyTo(array,i)
        i <- i + count

    if i < len then 
        let leftOver = len - i
        let leftOverArray = Array.init count (fun x -> if x < leftOver then 
                                                        array.[x+i]
                                                       else
                                                        Unchecked.defaultof< ^T>)
        let v = f (Vector< ^T>(leftOverArray,0))
        let mutable j = 0
        while i < len do
            array.[i] <- v.[j]
            i <- i + 1
            j <- j + 1


/// <summary>
/// Checks for the existence of a value. You provide a function that takes a Vector
/// and returns whether the condition you want exists in the Vector.
/// </summary>
/// <param name="f">Takes a Vector and returns true or false to indicate existence</param>
/// <param name="array"></param>
let inline exists (f : ^T Vector -> bool) (array: ^T[]) : bool =

    checkNonNull array

    let count = Vector< ^T>.Count
    let mutable found = false
    let len = array.Length

    let mutable vi = 0
    while vi < len - count do
        found <- f (Vector< ^T>(array,vi))
        if found then vi <- len
        else vi <- vi + count

    if not found && vi < len then
        let leftOverArray =
            [| for i=1 to Vector< ^T>.Count do
                if vi < array.Length then
                    yield array.[vi]
                    vi <- vi + 1
                else
                    yield array.[len-1] //just repeat the last item
            |]
        found <- f (Vector< ^T> leftOverArray)

    found

/// <summary>
/// Takes a function that accepts a vector and returns true or false. Returns the first Vector Option
/// that returns true or None if none match. Leftover array elements are ignored.
/// </summary>
/// <param name="f">Takes a Vector and returns true or false</param>
/// <param name="array"></param>
let inline tryFindVector (f : ^T Vector -> bool)  (array: ^T[]) : Vector< ^T> Option =

    checkNonNull array

    let count = Vector< ^T>.Count
    let mutable found = false
    let len = array.Length

    
    let mutable vi = 0
    while vi < len - count && not found do
        found <- f (Vector< ^T>(array,vi))
        vi <- vi + count

    if found then
        Some (Vector< ^T>(array,vi-count))
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
/// Identical to the standard contains, just faster
/// </summary>
/// <param name="x"></param>
/// <param name="array"></param>
let inline contains (x : ^T) (array:^T[]) : bool =
    
    checkNonNull array

    let count = Vector< ^T>.Count
    let mutable found = false
    let len = array.Length

    let mutable vi = 0
    let compareVector = Vector< ^T>(x)
    while vi < len - count do
        found <- Vector.EqualsAny(Vector< ^T>(array,vi),compareVector)
        if found then vi <- len
        else vi <- vi + count

    if not found && vi < len then
        let leftOverArray =
            [| for i=1 to Vector< ^T>.Count do
                if vi < array.Length then
                    yield array.[vi]
                    vi <- vi + 1
                else
                    yield array.[len-1] //just repeat the last item
            |]
        found <- Vector.EqualsAny(Vector< ^T>(leftOverArray),compareVector)

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

    let mutable vi = 0
    if len >= count then
        let mutable maxV = Vector< ^T>(array,0)
        vi <- vi + count
        while vi < len - count do
            let v = Vector< ^T>(array,vi)
            maxV <- Vector.Max(v,maxV)
            vi <- vi + count

        for i=0 to count-1 do
            if maxV.[i] > max then max <- maxV.[i]

    while vi < len do
        if array.[vi] > max then max <- array.[vi]
        vi <- vi + 1
    max

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

    let mutable vi = 0
    if len >= count then
        let mutable minV = Vector< ^T>(array,0)
        vi <- vi + count
        while vi < len - count do
            let v = Vector< ^T>(array,vi)
            minV <- Vector.Min(v,minV)
            vi <- vi + count

        for i=0 to count-1 do
            if minV.[i] < min then min <- minV.[i]

    while vi < len do
        if array.[vi] < min then min <- array.[vi]
        vi <- vi + 1
    min
