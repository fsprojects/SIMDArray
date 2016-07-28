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

    let inline checkNonNull arg = 
            match box arg with 
            | null -> nullArg "array"
            | _ -> ()

    let inline getLeftovers (array: ^T[]) (curIndex: int) : Vector<(^T)>   =
        let mutable vi = curIndex
        let leftOverArray = 
            [| for i in 1 .. (Vector<(^T)>.Count) do
                if vi < array.Length then
                    yield array.[vi]
                    vi <- vi + 1
                else 
                    yield Unchecked.defaultof<'T>
            |]
        Vector<(^T)>(leftOverArray)

    let inline applyLeftovers (count: int) (input: Vector<(^T)>) (result: Vector<(^T)>) =
         let vCount = Vector<(^T)>.Count
         let newArray = Array.zeroCreate vCount
         for i in 0 .. vCount-1 do
            if i < count then
                newArray.[i] <- input.[i]
            else
                newArray.[i] <- result.[i]
         Vector<(^T)>(newArray)

                    
    let inline SIMDFold
        (f: Vector<(^State)> -> Vector<(^T)> -> Vector<(^State)>)          
        (combiner : ^State -> ^State -> ^State)          
        (acc : ^State) 
        (array:^T[]) : ^State =
                
        checkNonNull array
        
        let mutable state = Vector<(^State)>(acc)
        let mutable vi = 0
        let vCount = Vector<(^T)>.Count
        while vi <= array.Length - vCount do
            state <- f state (Vector<(^T)>(array,vi))
            vi <- vi + vCount
        
        let leftoverCount = array.Length - vi
        if (leftoverCount <> 0) then                                                     
            let leftOver = f state (getLeftovers array vi )        
            state <- applyLeftovers leftoverCount leftOver state

        vi <- 0
        let mutable result = acc
        while vi < Vector<(^State)>.Count do
            result <- combiner result state.[vi]
            vi <- vi + 1
        result

    let inline SIMDReduce
        (f: Vector<(^State)> -> Vector<(^T)> -> Vector<(^State)>)          
        (combiner : ^State -> ^State -> ^State)                  
        (array:^T[]) : ^State =

        SIMDFold f combiner Unchecked.defaultof<(^State)> array
    
    let inline SIMDCreate (count :int) (x:^T) =         
         let array = (Array.zeroCreate count : ^T[]) 
         let mutable i = 0
         let v = Vector<(^T)>(x)
         let vCount = Vector<(^T)>.Count
         while i < count - vCount do
            v.CopyTo(array,i)
            i <- i + vCount

         while i < count do
            array.[i] <- x
            i <- i + 1

         array

    
    let inline SIMDSum (array:^T[]) : ^T =
                
        checkNonNull array
        
        let mutable state = Vector<(^T)>.Zero
        let mutable vi = 0
        let vCount = Vector<(^T)>.Count
        while vi <= array.Length - vCount do
            state <-  state + Vector<(^T)>(array,vi)
            vi <- vi + vCount
        
        let mutable result = Unchecked.defaultof<(^T)>
        while vi < array.Length do
            result <- result + array.[vi]
            vi <- vi + 1
        
        vi <- 0        
        while vi < vCount do            
            result <- result + state.[vi]
            vi <- vi + 1
        result

    let inline SIMDAverage (array:^T[]) : ^T =
        let sum = SIMDSum array
        LanguagePrimitives.DivideByInt<(^T)> sum array.Length

  

    let inline SIMDMap
        ( f : Vector<(^T)> -> Vector<(^U)>) (array : ^T[]) : ^U[] =

        checkNonNull array
        
        let len = array.Length
        let result = Array.zeroCreate len
        let inCount = Vector<(^T)>.Count        
        let outCount = Vector<(^U)>.Count
                   
        let mutable i = 0
        let mutable ri = 0
        while i < len - inCount do
            (f (Vector<(^T)>(array,i ))).CopyTo(result,ri)
            i <- i + inCount
            ri <- ri + outCount
                                                                 
        let leftoverCount = len - i
        if (leftoverCount <> 0) then                                                     
            let mutable ai = len - leftoverCount
            let leftOver = f (getLeftovers array ai)                    
            for i in 0 .. leftoverCount-1 do
                result.[ai] <- leftOver.[i]       
                ai <- ai + 1     
        result

    let inline SIMDMapInPlace
        ( f : Vector<(^T)> -> Vector<(^T)>) (array : ^T[]) :  unit =

        checkNonNull array
        
        let len = array.Length
        
        let inCount = Vector<(^T)>.Count        
                           
        let mutable i = 0
        
        while i < len - inCount do
            (f (Vector<(^T)>(array,i ))).CopyTo(array,i)
            i <- i + inCount
                                                                         
        let leftoverCount = len - i
        if (leftoverCount <> 0) then                                                     
            let mutable ai = len - leftoverCount
            let leftOver = f (getLeftovers array ai)                    
            for i in 0 .. leftoverCount-1 do
                array.[ai] <- leftOver.[i]       
                ai <- ai + 1     
        

  
    let inline SIMDExists (f : Vector<(^T)> -> bool) (array: ^T[]) : bool =
        let mutable vi = 0
        let vCount = Vector<(^T)>.Count        
        let mutable found = false
        let len = array.Length
        while vi < len - vCount do
            found <- f (Vector<(^T)>(array,vi))
            if found then 
                vi <- len
            else 
                vi <- vi + vCount
        
        if not found && vi < len then            
            let leftOverArray = 
              [| for i in 1 .. (Vector<(^T)>.Count) do
                     if vi < array.Length then
                        yield array.[vi]
                        vi <- vi + 1
                     else 
                        yield array.[len-1] //just repeat the last item
              |]
            found <- f (Vector<(^T)>(leftOverArray))
                                            
        found
    
   


    let inline SIMDMax (array :^T[]) : ^T =
        
         checkNonNull array
         let len = array.Length
         if len = 0 then invalidArg "array" "empty array"
         let mutable max = array.[0]
         let mutable vi = 0
         let count = Vector<(^T)>.Count
         if len >= count then
            let mutable maxV = Vector<(^T)>(array,0)
            vi <- vi + count            
            while vi < len - count do
               let v = Vector<(^T)>(array,vi)
               maxV <- Vector.Max(v,maxV)
               vi <- vi + count
            
            for i in 0 .. count-1 do          
                if maxV.[i] > max then max <- maxV.[i]
         while vi < len do
            if array.[vi] > max then max <- array.[vi]
            vi <- vi + 1
         max

    let inline SIMDMin (array :^T[]) : ^T =
        
         checkNonNull array
         let len = array.Length
         if len = 0 then invalidArg "array" "empty array"
         let mutable min = array.[0]
         let mutable vi = 0
         let count = Vector<(^T)>.Count
         if len >= count then
            let mutable minV = Vector<(^T)>(array,0)
            vi <- vi + count            
            while vi < len - count do
               let v = Vector<(^T)>(array,vi)
               minV <- Vector.Min(v,minV)
               vi <- vi + count
            
            for i in 0 .. count-1 do          
                if minV.[i] < min then min <- minV.[i]
         while vi < len do
            if array.[vi] < min then min <- array.[vi]
            vi <- vi + 1
         min
         
         

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
        x = 50000

    [<EntryPoint>]
    let main argv = 
        
        let mutable x = 1
        let s = Stopwatch()    
        while x <= 10000000 do
            let a = [| 1 .. x |]
                    
            s.Restart()
            let newmap =                                           
                 a |>
                 Array.SIMDMap testFunc
            s.Stop()
            printf "%A\n" newmap
            let simdt = s.ElapsedTicks                    
            System.GC.WaitForFullGCComplete() |> ignore
            s.Restart()
            
            a |>
            Array.SIMDMapInPlace testFunc

            s.Stop()        
            printf "%A\n" a
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
        
            
