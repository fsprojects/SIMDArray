// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open JMott.SIMDArray


module test = 


    let inline testSIMDFold f simdF c acc SIMDAcc a =
          printf "Testing %A\n" a
          let SIMDSum = 
                try 
                    Some (                                                       
                        a 
                        |> Array.SIMDFold simdF SIMDAcc c acc  )                    
                with
                    | :? System.ArgumentNullException -> None
          let sum = 
                try   
                    Some (             
                        a 
                        |> Array.fold f acc )                                            
                   with
                       | :? System.ArgumentNullException -> None 
                            
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
            [|1;2;3;4|]    
            [|1|]
            [|0|]
            [|1;2;3;4;5|]
          |]
                
        intArrays
        |> Array.iter (testSIMDFold (+) (+) (+) 0 0)
        
        0
            
