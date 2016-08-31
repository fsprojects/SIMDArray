[<RequireQualifiedAccess>]
module Parallel

open System.Threading.Tasks
open System

let inline private applyTask fromInc toExc stride f = 
       // printf "fromIncA:%A toExcA:%A stride:%A\n" fromInc toExc stride
        let mutable i = fromInc
        while i < toExc do
            f i
            i <- i + stride

let inline private applyTaskAggregate fromInc toExc stride acc f : ^T = 
       // printf "fromIncA:%A toExcA:%A stride:%A\n" fromInc toExc stride
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
    
        //printf "len:%A numTasks:%A numStrides:%A stridesPerCore:%A elementsPerCore:%A remainderStrides:%A\n" len numTasks numStrides stridesPerTask elementsPerTask remainderStrides    
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
            //printf "index:%A toExc:%A\n" index toExc
        
            taskArray.[i] <- Task.Factory.StartNew(fun () -> applyTask fromInc toExc stride f)                        
            index <- toExc
                        
        Task.WaitAll(taskArray)


let inline ForStrideAggreagate (fromInclusive : int) (toExclusive :int) (stride : int) (acc: ^T) (f : int -> ^T -> ^T) combiner =
            
    let numStrides = (toExclusive-fromInclusive)/stride
    if numStrides > 0 then
        let numTasks = Math.Min(Environment.ProcessorCount,numStrides)
        let stridesPerTask = numStrides/numTasks
        let elementsPerTask = stridesPerTask * stride;
        let mutable remainderStrides = numStrides - (stridesPerTask*numTasks)
    
      //  printf "numTasks:%A numStrides:%A stridesPerCore:%A elementsPerCore:%A remainderStrides:%A\n" numTasks numStrides stridesPerTask elementsPerTask remainderStrides    
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
            //printf "index:%A toExc:%A\n" index toExc
            let acc = acc
            taskArray.[i] <- Task< ^T>.Factory.StartNew(fun () -> applyTaskAggregate fromInc toExc stride acc f)                        
            index <- toExc
                        
        let mutable result = acc
        for i = 0 to taskArray.Length-1 do       
            result <- combiner result taskArray.[i].Result    
        result
    else
        acc