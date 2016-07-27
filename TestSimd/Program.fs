// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
//open JMott.SIMDArray


open System.Numerics
open System
open System.Threading.Tasks
open System.Diagnostics
open System.Linq
open System.Threading

module Array =

    let inline getLeftovers (array: 'T[]) (curIndex: int) : Vector<'T>  =
        let mutable vi = curIndex
        let leftOverArray = 
            [| for i in 1 .. (Vector<'T>.Count) do
                if vi < array.Length then
                    yield array.[vi]
                    vi <- vi + 1
                else 
                    yield Unchecked.defaultof<'T>
            |]
        Vector<'T>(leftOverArray)

    let inline applyLeftovers (count: int) (input: Vector<'T>) (result: Vector<'T>) =
         let newArray = Array.zeroCreate Vector<'T>.Count
         for i in 0 .. Vector<'T>.Count-1 do
            if i < count then
                newArray.[i] <- input.[i]
            else
                newArray.[i] <- result.[i]
         Vector<'T>(newArray)

            
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

        //let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(f)
        let mutable state = Vector<'State>(acc)
        let mutable vi = 0
        let vCount = Vector<'T>.Count
        while vi <= array.Length - vCount do
            state <- f state (Vector<'T>(array,vi))
            vi <- vi + vCount
        
        let leftoverCount = array.Length - vi
        if (leftoverCount <> 0) then                                                     
            let leftOver = f state (getLeftovers array vi)        
            state <- applyLeftovers leftoverCount leftOver state

        vi <- 0
        let mutable result = acc
        while vi < Vector<'State>.Count do
            result <- combiner result state.[vi]
            vi <- vi + 1
        result


    let inline SIMDMap<'T,'U when 'T : (new:unit -> 'T) and 'T: struct and 'T :> ValueType
                      and  'U : (new:unit -> 'U) and 'U: struct and 'U :> ValueType>
        ( f : Vector<'T> -> Vector<'U>) (array : 'T[]) : 'U[] =

        match box array with 
            | null -> nullArg "array"
            | _ -> ()
        
        let result = Array.zeroCreate array.Length  
        let inCount = Vector<'T>.Count        
        let outCount = Vector<'U>.Count

                
        Parallel.For(0,array.Length/inCount,(fun i ->
            (f (Vector<'T>(array,i * inCount))).CopyTo(result,i * outCount)                                     
        )) |> ignore
        
        let leftoverCount = array.Length % inCount
        if (leftoverCount <> 0) then                                                     
            let mutable ai = array.Length - leftoverCount
            let leftOver = f (getLeftovers array ai)                    
            for i in 0 .. leftoverCount-1 do
                result.[ai] <- leftOver.[i]       
                ai <- ai + 1     
        result


    let inline SIMDMapInPlace<'T when 'T : (new:unit -> 'T) and 'T: struct and 'T :> ValueType>
                      
        ( f : Vector<'T> -> Vector<'T>) (array : 'T[]) : 'T[] =

        match box array with 
            | null -> nullArg "array"
            | _ -> ()
                
        let count = Vector<'T>.Count        
                        
        Parallel.For(0,array.Length/count,(fun i ->
            let index = i * count
            (f (Vector<'T>(array,index))).CopyTo(array,index)                                     
        )) |> ignore
        
        let leftoverCount = array.Length % count
        if (leftoverCount <> 0) then                                                     
            let mutable ai = array.Length - leftoverCount
            let leftOver = f (getLeftovers array ai)                    
            for i in 0 .. leftoverCount-1 do
                array.[ai] <- leftOver.[i]       
                ai <- ai + 1     
        array
    

module test = 


    let inline testSIMDFold f simdF c acc (a:'T[]) =
          if a <> null then
            printf "Testing Fold With len %A\n" a.Length
          else 
            printf "Test Fold With Null"
          let s = Stopwatch()
          s.Start()
          let SIMDSum = 
                try 
                  
                    Some (                                                       
                        a 
                        |> Array.SIMDFold simdF c acc  )                    
                    
                with
                    | _ -> None
          s.Stop()
          let simdTime = s.ElapsedTicks
          s.Restart()
          let sum = 
                try   
                    Some (             
                        a 
                        |> Array.fold f acc )                                            
                   with
                       | _ -> None 
          s.Stop()
          let time = s.ElapsedTicks
                            
          match (sum,SIMDSum) with
          | (Some s, Some simd) when s <> simd -> printf "Sum Error.  SIMDSum: %A  Sum: %A\n" simd s
          | (Some _, None) -> printf "Sum Error. simd was null sum was not\n"
          | (None, Some _) ->  printf "Sum Error, sum was null simd was not\n"
          | _ -> printf "Success. SIMD was %A ticks faster\n" (time-simdTime)

    let inline testSIMDMap simdF f (a:'T[]) =
          if a <> null then
            printf "Testing Map With len %A\n" a.Length
          else 
            printf "Test Map With Null"

          let sa = Array.copy a
          let s = Stopwatch()
          s.Start()
          let SIMDSum = 
                try 
                    Some (                                                       
                        sa 
                        |> Array.SIMDMapInPlace simdF  )                    
                with
                    | _ -> None
          s.Stop()
          let simdTime = s.ElapsedTicks
          s.Restart()
          let sum = 
                try   
                    Some (             
                        a 
                        |> Array.map f )                                            
                   with
                       | _ -> None 
          s.Stop()
          let time = s.ElapsedTicks    
          match (sum,SIMDSum) with
          | (Some s, Some simd) when s <> simd -> printf "Sum Error.  SIMDSum: %A  Sum: %A\n" simd s
          | (Some _, None) -> printf "Sum Error. simd was null sum was not\n"
          | (None, Some _) ->  printf "Sum Error, sum was null simd was not\n"
          | _ -> printf "Success. SIMD was %A ticks faster\n" (time-simdTime)


    let inline testFunc x =
        x*x/x+x+x-x*x+x/x

    [<EntryPoint>]
    let main argv = 
        
        let intArrays =
          [|            
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
            [|0 .. 101|]   
            [|0 .. 1001|]
            [|0 .. 10001|]
            [|0 .. 100001|]
            [|0 .. 1000001|]
            [|0 .. 10000001|]
            [|0 .. 100000001|] 
          |]
       
        intArrays
        |> Array.iter (testSIMDMap (fun x -> x * x * x) (fun x -> x*x *x ))

        intArrays
        |> Array.iter (testSIMDFold (+) (+) (+) 0)

        
        0
        
            
