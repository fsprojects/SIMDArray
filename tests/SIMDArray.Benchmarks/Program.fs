module Program

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



#if MONO
#else
open BenchmarkDotNet.Diagnostics.Windows
open System.Collections.Generic

#endif
                
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

let inline indexNotFound() = raise (KeyNotFoundException())

                    
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



type CoreConfig () =
    inherit ManualConfig()
    do               
        //base.Add (Job.RyuJitX64.WithTargetCount(Count(100)))
        base.Add Job.RyuJitX64
        
        #if MONO
        #else
        base.Add(new MemoryDiagnoser())
        #endif

[<Config (typeof<CoreConfig>)>]
type CoreBenchmark () =    

    let mutable list = []
    let mutable array = [||]
    let mutable array2 = [||]
    let mutable array3 = [||]
    let mutable resizeArray = ResizeArray<int>()

    
    //let mutable mathnetVector = vector [1.0f]
    //let mutable mathnetVector2 = vector [1.0f]

    [<Params (100000)>]     
    member val public Length = 0 with get, set

   
      
    [<Setup>]
    member self.SetupData () =  
       
       let r = Random(self.Length)
       //list <- List.init self.Length (fun i -> (1.0,2.0))

       //array <- Array.init self.Length (fun i -> 2)
       //array <- Array.create self.Length 10 
       //array2 <- Array.init self.Length (fun i -> 3)
        
       array <- Array.init self.Length (fun i -> i)
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
    member self.skipWhile () =                        
        Array.skipWhile (fun x -> x < 50000) array
        Array.take
                                           

    [<Benchmark(Baseline=true)>]
    member self.simdSkipWhile () =    
        Array.SIMD.skipWhile (fun x -> Vector.LessThanAll(x,Vector<int>(50000))) (fun x -> x < 50000) array
        
          

[<EntryPoint>]
let main argv =              
    
  
    let a = [|1;2;3;4;5;6;7;8|]


    let r1 = Array.map (fun x -> x*x) a
    let r2 = Array.SIMD.map (fun x-> x*x) nop a

    printf "r1:%A\n" r1
    printf "r2:%A\n" r2

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
   
                 
    let switch = 
        BenchmarkSwitcher [|
            typeof<CoreBenchmark>
        |] 

    switch.Run [|"CoreBenchmark"|] |> ignore
    0


