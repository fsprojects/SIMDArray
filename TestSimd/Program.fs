// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
//open JMott.SIMDArray


open Nessos.Streams
open System.Numerics
open System
open System.Threading.Tasks

module Array =
    let inline SIMDFold<'T,'State
                                 when 'T : (new:unit -> 'T) and 'T: struct and 'T :> ValueType
                                 and 'State : (new:unit -> 'State) and 'State: struct and 'State :> ValueType> 
        (f: Vector<'State> -> Vector<'T> -> Vector<'State>)          
        (combiner : 'State -> 'State -> 'State)          
        (acc : 'State) 
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
        
            state <- Vector<'State>( [| for i in 1 .. Vector<'State>.Count   do
                                         if i <= leftOverCount then
                                            yield leftOver.[i-1]
                                         else 
                                            yield state.[i-1]
                                      |])                        
        vi <- 0
        let mutable result = acc
        while vi < Vector<'State>.Count do
            result <- combiner result state.[vi]
            vi <- vi + 1
        result


    let SIMDMap<'T,'U when 'T : (new:unit -> 'T) and 'T: struct and 'T :> ValueType
                      and  'U : (new:unit -> 'U) and 'U: struct and 'U :> ValueType>
        ( f : Vector<'T> -> Vector<'U>) (array : 'T[]) : 'U[] =

        match box array with 
            | null -> nullArg "array"
            | _ -> ()
        
        let result = Array.zeroCreate array.Length
        Parallel.For(0,array.Length/Vector<'T>.Count,fun i ->            
            let r = f (Vector<'T>(array,i*Vector<'T>.Count))
            r.CopyTo(result,i*Vector<'U>.Count) 
            )  |> ignore
        result
    

module test = 


    let inline testSIMDFold f simdF c acc a =
          printf "Testing %A\n" a
          let SIMDSum = 
                try 
                    Some (                                                       
                        a 
                        |> Array.SIMDFold simdF c acc  )                    
                with
                    | _ -> None
          let sum = 
                try   
                    Some (             
                        a 
                        |> Array.fold f acc )                                            
                   with
                       | _ -> None 
                            
          match (sum,SIMDSum) with
          | (Some s, Some simd) when s <> simd -> printf "Sum Error.  SIMDSum: %A  Sum: %A\n" simd s
          | (Some _, None) -> printf "Sum Error. simd was null sum was not\n"
          | (None, Some _) ->  printf "Sum Error, sum was null simd was not\n"
          | _ -> printf "Success\n"

    let inline testSIMDMap simdF f a =      
          printf "Testing %A\n" a
          let SIMDSum = 
                try 
                    Some (                                                       
                        a 
                        |> Array.SIMDMap simdF  )                    
                with
                    | _ -> None
          let sum = 
                try   
                    Some (             
                        a 
                        |> Array.map f )                                            
                   with
                       | _ -> None 
                            
          match (sum,SIMDSum) with
          | (Some s, Some simd) when s <> simd -> printf "Sum Error.  SIMDSum: %A  Sum: %A\n" simd s
          | (Some _, None) -> printf "Sum Error. simd was null sum was not\n"
          | (None, Some _) ->  printf "Sum Error, sum was null simd was not\n"
          | _ -> printf "Success\n"


    [<EntryPoint>]
    let main argv = 
        let intArrays =
          [|
            null
            [||]              
            [|1|]
            [|0|]
            [|-1|]
            [|1;2;|]
            [|1;2;3|]
            [|1;2;3;4|]  
            [|1;2;3;4;5|]
            [|1;2;3;4;5;6|]
            [|1;2;3;4;5;6;7|]
            [|1;2;3;4;5;6;7;8|]
            [|1;2;3;4;5;6;7;8;9|]
            [|1;2;3;4;5;6;7;8;9;10|]            
          |]
       
        intArrays
        |> Array.iter (testSIMDMap (fun e -> e+Vector<int>(1)) (fun e -> e+1))

        
        0
        
            
