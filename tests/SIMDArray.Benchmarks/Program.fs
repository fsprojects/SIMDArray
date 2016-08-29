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

    
    //let mutable mathnetVector = vector [1.0f]
    //let mutable mathnetVector2 = vector [1.0f]

    [<Params (1000,100000,1000000)>]     
    member val public Length = 0 with get, set

    [<Params (10000,25000,50000,75000,100000,2000000)>]     
    member val public Density = 0 with get, set

   

    [<Setup>]
    member self.SetupData () =  
       
       let r = Random(self.Length+self.Density)
       //list <- List.init self.Length (fun i -> (1.0,2.0))

       //array <- Array.init self.Length (fun i -> 2)
       //array <- Array.create self.Length 10 
       //array2 <- Array.init self.Length (fun i -> 3)
        
       array <- Array.init self.Length (fun i ->(int)( r.NextDouble()*(float)self.Density))
        //array2 <- Array.init self.Length (fun i -> i)
        

        // for comparewith, exists, exists 2
        //array <- Array.create self.Length 10
      //  array2 <- Array.create self.Length 10


        //for concat
        //array <- Array.init self.Length (fun i -> [|1;2;3;4;5;|])
        
      

    
    
    [<Benchmark>]
    member self.distinct2 () =                    
        ()

      
    

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
   
                 
    let switch = 
        BenchmarkSwitcher [|
            typeof<CoreBenchmark>
        |] 

    switch.Run [|"CoreBenchmark"|] |> ignore
    0


