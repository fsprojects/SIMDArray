[<RequireQualifiedAccess>]
module Parallel

open System.Threading.Tasks
open System


let inline private applyTask fromInc toExc stride f = 
        let mutable i = fromInc
        while i < toExc do
            f i
            i <- i + stride


let inline ForStride (fromInclusive : int) (toExclusive :int) (stride : int) (f : int -> unit) =
    
    let len = toExclusive - fromInclusive 
    let numTasks = Environment.ProcessorCount
    let chunkSize = 
        if len / numTasks >= 1 then
            len / numTasks
        else 
            1
    
    let remainder = len - chunkSize*numTasks

    //printf "len:%A numTasks:%A chunkSize:%A remainder:%A\n" len numTasks chunkSize remainder

    let mutable i = 0
    
    let taskArray : Task[] = Array.create (numTasks+1) null

    while i < numTasks do
        let index = i * chunkSize
        taskArray.[i] <- Task.Factory.StartNew( fun () -> applyTask index (index+chunkSize) stride f)
        i <- i + 1
        
        

    if remainder <> 0 then
        let index = i*chunkSize
        taskArray.[i] <- Task.Factory.StartNew( fun () -> applyTask index (index+remainder) stride f)
        
    Task.WaitAll(taskArray)
