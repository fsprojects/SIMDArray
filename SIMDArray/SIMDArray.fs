namespace JMott.SIMDArray
open System
open System.Numerics



module Array =
    let inline SIMDFold2<'T,'State
                                 when 'T : (new:unit -> 'T) and 'T: struct and 'T :> ValueType
                                 and 'State : (new:unit -> 'State) and 'State: struct and 'State :> ValueType> 
        (f: Vector<'State> -> Vector<'T> -> Vector<'State>)  
        (acc : 'State) 
        (combiner : 'State -> 'State -> 'State)  
        (combinerAcc : 'State)
        (array:'T[]) =

        

        match box array with 
            | null -> nullArg "array"
            | _ -> ()

        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(f)
        let mutable state = Vector<'State>(acc)
        let mutable vi = 0
        while vi <= array.Length - Vector<'T>.Count do
            state <- f.Invoke(state,Vector<'T>(array,vi))
            vi <- vi + Vector<'T>.Count
        
        let leftOverCount = array.Length - vi        
        if (leftOverCount <> 0) then    
            let leftOverArray = 
                [| for i in 1 .. (Vector<'T>.Count) do
                    if vi < array.Length then
                        yield array.[vi]
                        vi <- vi + 1
                    else 
                        yield array.[0]
                |]
                                     
            let leftOver = f.Invoke(state,Vector<'T>(leftOverArray))
        
            state <- Vector<'State>( [| for i in 1 .. Vector<'State>.Count do
                                         if Vector<'State>.Count - i >= leftOverCount then
                                            yield leftOver.[i]
                                         else 
                                            yield state.[i]
                                      |])
                        
        vi <- 0
        let mutable result = combinerAcc
        while vi < Vector<'State>.Count do
            result <- combiner result state.[vi]
            vi <- vi + 1
        result




