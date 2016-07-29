// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
//open JMott.SIMDArray


open System.Numerics
open System
open System.Diagnostics
open JMott.SIMDArray
open FsCheck
                  
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
          | (Some s, Some simd) when s <> simd -> printf "Sum Error.  SIMDSum: %A  Sum: %A Array:%A\n" simd s a
          | (Some _, None) -> printf "Sum Error. simd was null sum was not Array:%A\n" a
          | (None, Some _) ->  printf "Sum Error, sum was null simd was not Array:%A\n" a
          | _ -> printf "Success. SIMD was %A ticks faster\n" (time-simdTime)


    let inline arrayComp (a: 'T[]) (b: 'T[]) =
       Array.fold (&&) true (Array.zip a b |> Array.map (fun (aa,bb) -> aa.Equals(bb) ))


    let inline compareNums a b =
        let fa = (float a)
        let fb = (float b)
        a.Equals(b) || float(a - b) < 0.00001 || float(b -  a) < 0.00001 ||
        Double.IsNaN(fa) && Double.IsInfinity(fb) || Double.IsNaN(fb) && Double.IsInfinity(fa) 
        

    let inline testMap (array:'T[]) =
        
        let a = array |> Array.SIMDMap (fun x -> x*x)
        let b = array |> Array.map (fun x -> x*x)

        let c = array |> Array.SIMDMap (fun x -> x+x)
        let d = array |> Array.map (fun x -> x+x)

        let e = array |> Array.SIMDMap (fun x -> x-x)
        let f = array |> Array.map (fun x -> x-x)

        let g = array |> Array.SIMDMap (fun x -> x)
        let h = array |> Array.map (fun x -> x)

        arrayComp a b && arrayComp c d && arrayComp e f && arrayComp g h

    let inline testFold (array:'T[]) =
        
        let a = array |> Array.SIMDFold (+) (+) Unchecked.defaultof<'T>
        let b = array |> Array.fold (+) Unchecked.defaultof<'T>
                
        compareNums a b

    let inline testMinMax (array:'T[]) =
        if array.Length <> 0 then
            let a = array |> Array.SIMDMin
            let b = array |> Array.min

            let c = array |> Array.SIMDMax
            let d = array |> Array.max
                
            compareNums a b && compareNums c d
        else 
            true

    let testInt32 (array:int[]) =
        testMap array &&
        testFold array &&
        testMinMax array

    let testInt64 (array:int64[]) =
        testMap array &&
        testFold array &&
        testMinMax array

    let testFloat (array:float[]) =
        testMap array &&
        testFold array &&
        testMinMax array

    let testFloat32 (array:float32[]) =
        testMap array &&
        testFold array &&
        testMinMax array


    
    [<EntryPoint>]
    let main argv =              
              
       
        let testCount = 100000
        printf "Test Int32\n"
        Check.One( {Config.Quick with MaxTest = testCount;}, testInt32)
        printf "Test Int64\n"
        Check.One( {Config.Quick with MaxTest = testCount;}, testInt64)
        printf "Test Float\n"
        Check.One( {Config.Quick with MaxTest = testCount;}, testFloat)
        printf "Test Float32\n"
        Check.One( {Config.Quick with MaxTest = testCount;}, testFloat32)
        

       
       
            
        
        
        0
        
            
