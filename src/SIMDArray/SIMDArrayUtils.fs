module SIMDArrayUtils

/// <summary>
/// Utility function for use with SIMD higher order functions
/// When you don't have leftover elements
/// example:
/// Array.SIMD.Map (fun x -> x*x) nop array
/// Where array is divisible by your SIMD width or you don't
/// care about what happens to the leftover elements
/// </summary>
let inline nop _ = Unchecked.defaultof<_>

let inline checkNonNull arg =
    match box arg with
    | null -> nullArg "array"
    | _ -> ()


open System.Threading.Tasks
open System

let inline private applyTask fromInc toExc stride f =        
        let mutable i = fromInc
        while i < toExc do
            f i
            i <- i + stride

let inline private applyTaskAggregate fromInc toExc stride acc f : ^T =        
        let mutable i = fromInc
        let mutable acc = acc
        while i < toExc do
            acc <- f i acc
            i <- i + stride
        acc


let inline ForStride (fromInclusive : int) (toExclusive :int) (stride : int) (f : int -> unit) =
            
    let numStrides = (toExclusive-fromInclusive)/stride
    if numStrides > 0 then
        let numTasks = Math.Min(Environment.ProcessorCount,numStrides)
        let stridesPerTask = numStrides/numTasks
        let elementsPerTask = stridesPerTask * stride;
        let mutable remainderStrides = numStrides - (stridesPerTask*numTasks)
            
        let taskArray : Task[] = Array.zeroCreate numTasks
        let mutable index = 0    
        for i = 0 to taskArray.Length-1 do        
            let toExc =
                if remainderStrides = 0 then
                    index + elementsPerTask
                else
                    remainderStrides <- remainderStrides - 1
                    index + elementsPerTask + stride
            let fromInc = index;            
        
            taskArray.[i] <- Task.Factory.StartNew(fun () -> applyTask fromInc toExc stride f)                        
            index <- toExc
                        
        Task.WaitAll(taskArray)


let inline ForStrideAggregate (fromInclusive : int) (toExclusive :int) (stride : int) (acc: ^T) (f : int -> ^T -> ^T) combiner =      
    let numStrides = (toExclusive-fromInclusive)/stride
    if numStrides > 0 then
        let numTasks = Math.Min(Environment.ProcessorCount,numStrides)
        let stridesPerTask = numStrides/numTasks
        let elementsPerTask = stridesPerTask * stride;
        let mutable remainderStrides = numStrides - (stridesPerTask*numTasks)
          
        let taskArray : Task< ^T>[] = Array.zeroCreate numTasks
        let mutable index = 0    
        for i = 0 to taskArray.Length-1 do        
            let toExc =
                if remainderStrides = 0 then
                    index + elementsPerTask
                else
                    remainderStrides <- remainderStrides - 1
                    index + elementsPerTask + stride
            let fromInc = index;            
            taskArray.[i] <- Task< ^T>.Factory.StartNew(fun () -> applyTaskAggregate fromInc toExc stride acc f)                        
            index <- toExc
                        
        let mutable result = acc
        for i = 0 to taskArray.Length-1 do       
            result <- combiner result taskArray.[i].Result    
        result
    else
        acc

// Avoid reflection costs on min and max functions
type MinValue =
    static member MinValue (_:byte          , _:MinValue) = System.Byte.MinValue
    static member MinValue (_:sbyte         , _:MinValue) = System.SByte.MinValue
    static member MinValue (_:float         , _:MinValue) = System.Double.NegativeInfinity
    static member MinValue (_:int16         , _:MinValue) = System.Int16.MinValue
    static member MinValue (_:int           , _:MinValue) = System.Int32.MinValue
    static member MinValue (_:int64         , _:MinValue) = System.Int64.MinValue
    static member MinValue (_:float32       , _:MinValue) = System.Single.NegativeInfinity
    static member MinValue (_:uint16        , _:MinValue) = System.UInt16.MinValue
    static member MinValue (_:uint32        , _:MinValue) = System.UInt32.MinValue
    static member MinValue (_:uint64        , _:MinValue) = System.UInt64.MinValue

    static member inline Invoke() =
        let inline call_2 (a:^a, b:^b) = ((^a or ^b) : (static member MinValue: _*_ -> _) b, a)
        let inline call (a:'a) = call_2 (a, Unchecked.defaultof<'r>) :'r
        call Unchecked.defaultof<MinValue>

type MaxValue =
    static member MaxValue (_:byte          , _:MaxValue) = System.Byte.MaxValue
    static member MaxValue (_:sbyte         , _:MaxValue) = System.SByte.MaxValue
    static member MaxValue (_:float         , _:MaxValue) = System.Double.PositiveInfinity
    static member MaxValue (_:int16         , _:MaxValue) = System.Int16.MaxValue
    static member MaxValue (_:int           , _:MaxValue) = System.Int32.MaxValue
    static member MaxValue (_:int64         , _:MaxValue) = System.Int64.MaxValue
    static member MaxValue (_:float32       , _:MaxValue) = System.Single.PositiveInfinity
    static member MaxValue (_:uint16        , _:MaxValue) = System.UInt16.MaxValue
    static member MaxValue (_:uint32        , _:MaxValue) = System.UInt32.MaxValue
    static member MaxValue (_:uint64        , _:MaxValue) = System.UInt64.MaxValue

    static member inline Invoke() =
        let inline call_2 (a:^a, b:^b) = ((^a or ^b) : (static member MaxValue: _*_ -> _) b, a)
        let inline call (a:'a) = call_2 (a, Unchecked.defaultof<'r>) :'r
        call Unchecked.defaultof<MaxValue>