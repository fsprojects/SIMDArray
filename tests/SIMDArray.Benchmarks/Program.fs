﻿module Program

open System.Linq
open System.Linq.Expressions
open System
open System.Threading.Tasks
open System.Threading
open System.Numerics
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open SIMDArrayUtils
open BenchmarkDotNet.Diagnostics.Windows
open System.Collections.Generic
                
module Array =
    let inline zeroCreateUnchecked (count:int) = 
        Array.zeroCreate count

    let inline subUnchecked startIndex count (array : 'T[]) =
        Array.sub array startIndex count

// Almost every array function calls this, so mock it with 
// the exact same code
let inline checkNonNull argName arg = 
            match box arg with 
            | null -> nullArg argName 
            | _ -> ()

let empty = [||]

let inline indexNotFound() = raise (Exception())

                    
let partition f (array: _[]) = 
            checkNonNull "array" array
            let res = Array.zeroCreateUnchecked array.Length        
            let mutable upCount = 0
            let mutable downCount = array.Length-1    
            for x in array do                
                if f x then 
                    res.[upCount] <- x
                    upCount <- upCount + 1
                else
                    res.[downCount] <- x
                    downCount <- downCount - 1
                
            let res1 = Array.subUnchecked 0 upCount res
            let res2 = Array.zeroCreateUnchecked (array.Length - upCount)    
        
            downCount <- array.Length-1
            for i = 0 to res2.Length-1 do
                res2.[i] <- res.[downCount]        
                downCount <- downCount - 1
        
            res1 , res2



[<MemoryDiagnoser>]
type CoreBenchmark () =    

    let mutable list = []
    let mutable array = [||]
    let mutable array2 = [||]
    let mutable array3 = [||]
    let mutable resizeArray = ResizeArray<int>()

    
    //let mutable mathnetVector = vector [1.0f]
    //let mutable mathnetVector2 = vector [1.0f]

    [<Params (100,1000,1000000)>]     
    member val public Length = 0 with get, set

    member val public Half = Int32.MaxValue / 2
      
    [<GlobalSetup>]
    member self.SetupData () =  
       
       let r = Random(self.Length)
       //list <- List.init self.Length (fun i -> (1.0,2.0))

       array <- Array.init self.Length (fun i -> r.Next())
       //array <- Array.create self.Length 10 
       array2 <- Array.init self.Length (fun i -> r.Next())
        
//       array <- Array.create self.Length 
//       array2 <- Array.create self.Length 2
//       resizeArray <- ResizeArray<int>(self.Length)
       //for i = 0 to self.Length-1 do
        //resizeArray.Add(array.[i])
        
        //array2 <- Array.init self.Length (fun i -> i)
        

        // for comparewith, exists, exists 2
        //array <- Array.create self.Length 10
      //  array2 <- Array.create self.Length 10


        //for concat
        //array <- Array.init self.Length (fun i -> [|1;2;3;4;5;|])
        
                                                  
    [<Benchmark>]
    member self.ForSum () =                                
       array |> Array.map (fun x -> x*x)
    [<Benchmark>]
    member self.ForSumSIMD () =                                
       array |> Array.SIMD.map (fun x -> x*x) (fun x -> x*x)

    [<Benchmark>]
    member self.Dot () =                                
       array |> Array.fold2 (fun a x y -> a + x*y) 0 array2

    [<Benchmark>]
    member self.DotSIMD () =                                
       array |> Array.SIMD.dot array2

    [<Benchmark>]
    member self.Max () =
        array |> Array.max

    [<Benchmark>]
    member self.MaxSIMD () =
        array |> Array.SIMD.max

    [<Benchmark>]
    member self.MaxBy () =
        array |> Array.maxBy (fun x -> x*x)

    [<Benchmark>]
    member self.MaxBySIMD () =
        array |> Array.SIMD.maxBy (fun x -> x*x) (fun x -> x*x)
    
    [<Benchmark>]
    member self.Map () =
        array |> Array.map (fun x -> x + 2*x)

    [<Benchmark>]
    member self.MapSIMD () =
        array |> Array.SIMD.map (fun x -> x + 2*x) (fun x -> x + 2*x)
        
    [<Benchmark>]
    member self.Fold () =
        (0, array) ||> Array.fold (fun acc x -> x + acc)

    [<Benchmark>]
    member self.FoldSIMD () =
        let inline fn acc x = x + acc
        (0, array) ||> Array.SIMD.fold fn fn (+)

    [<Benchmark>]
    member self.Partition () =
        array |> Array.partition (fun x -> x > self.Half)

    [<Benchmark>]
    member self.PartitionPerformance () =
        array |> Array.Performance.partitionUnordered (fun x -> x > self.Half)
    
    [<Benchmark>]
    member self.Filter () =
        array |> Array.filter (fun x -> x % 2 = 0)

    [<Benchmark>]
    member self.FilterPerformance() =
        array |> Array.Performance.filterSimplePredicate (fun x -> x % 2 = 0)
        
        

[<EntryPoint>]
let main argv =              
    

   

     (*
    let r = Random(1)
    let array = Array.init 100000 (fun i ->(int)( r.NextDouble()*100.0))

    let result = array |> filterOldPlusPlus (fun x-> x < 10 )
    printf "*******\n"
    let result = array |> filterOldPlusPlus (fun x-> x < 25 )
    printf "*******\n"
    let result = array |> filterOldPlusPlus (fun x-> x < 50 )
    printf "*******\n"
    let result = array |> filterOldPlusPlus (fun x-> x < 75 )
    printf "*******\n"
    let result = array |> filterOldPlusPlus (fun x-> x < 90 )
    printf "*******\n"
    let result = array |> filterOldPlusPlus (fun x-> x < 100)
    printf "*******\n"*)
   
    let _ = BenchmarkRunner.Run<CoreBenchmark>()
    0
   


    