// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
//open JMott.SIMDArray


open System.Numerics
open System.Diagnostics
open JMott.SIMDArray

                  
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

          
          let s = Stopwatch()
          s.Start()
          let SIMDSum = 
                try 
                    Some (                                                       
                        a 
                        |> Array.SIMDMap simdF  )                    
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
        x * x 
      
    let inline SIMDFindFunc (x:Vector<int>) : bool =
        Vector.EqualsAny(x,Vector<int>(50000))
    
    let inline findFunc x =
        x = 500000

    [<EntryPoint>]
    let main argv = 
        
        let mutable x = 1
        let s = Stopwatch()    
        while x <= 10000000 do
            let a = [| 1 .. x |]
                    
            s.Restart()
            let newmap =                                           
                 a |>
                 Array.SIMDSimpleExists 500000
            s.Stop()
            printf "%A\n" newmap
            let simdt = s.ElapsedTicks                    
            System.GC.WaitForFullGCComplete() |> ignore
            s.Restart()
            
            let newmap = 
                a |>
                Array.exists findFunc

            s.Stop()        
            printf "%A\n" newmap
            let t = s.ElapsedTicks        
            printf "x:%A Diff:%A  percent:%A\n" x (t-simdt) (((float)t/(float)simdt)*100.0)
            x <- x * 10

        (*
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

        *)
        0
        
            
