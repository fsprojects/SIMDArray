[<RequireQualifiedAccess>]
module Parallel

open System.Threading.Tasks
open System.Threading
open System

let inline private applyTask fromInc toExc stride f = 
        
        let mutable i = fromInc
        while i < toExc do
            f i
            i <- i + stride


let inline ForStride (fromInclusive : int) (toExclusive :int) (stride : int) (f : int -> unit) =
    
    let len = toExclusive - fromInclusive 
    let numCores = Environment.ProcessorCount //logical cores    
    let numStrides = len/stride
    let stridesPerCore = numStrides/numCores
    let elementsPerCore = stridesPerCore * stride;
    let mutable remainderStrides = numStrides - (stridesPerCore*numCores)
    
//    printf "len:%A numCores:%A numStrides:%A stridesPerCore:%A elementsPerCore:%A remainderStrides:%A\n" len numCores numStrides stridesPerCore elementsPerCore remainderStrides
        
    let taskArray : Task[] = Array.create (numCores) null
    let mutable index = 0
    for i = 0 to taskArray.Length-1 do        
        let toExc =
            if remainderStrides = 0 then
                index + elementsPerCore
            else
                remainderStrides <- remainderStrides - 1
                index + elementsPerCore + stride
        let fromInc = index;
        //printf "index:%A toExc:%A\n" index toExc
        taskArray.[i] <- Task.Factory.StartNew( fun () -> applyTask fromInc toExc stride f)        
        index <- toExc
        
                
    Task.WaitAll(taskArray)
