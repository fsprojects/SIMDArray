namespace JMott.SIMDArray
open System
open System.Numerics



module Array =
    let inline SIMDFold<'T,'State
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
        while vi < array.Length do
            state <- f.Invoke(state,Vector<'T>(array,vi))
            vi <- vi + Vector<'T>.Count
        vi <- 0
        let mutable result = combinerAcc
        while vi < Vector<'State>.Count do
            result <- combiner result state.[vi]
            vi <- vi + 1
        result




