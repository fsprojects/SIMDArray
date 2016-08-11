module Program


open System
open System.Numerics
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs

#if MONO
#else
open BenchmarkDotNet.Diagnostics.Windows
#endif
                  

let inline f x =
    x*x+x

type SIMDConfig () =
    inherit ManualConfig()
    do 
        base.Add Job.RyuJitX64
        #if MONO
        #else
        base.Add(new MemoryDiagnoser())
        #endif

[<Config (typeof<SIMDConfig>)>]
type SIMDBenchmark () =    
    let r = Random()
    let mutable array = [||]
    let mutable array2 = [||]
    let mutable array3 = [||]
    //let mutable mathnetVector = vector [1.0f]
    //let mutable mathnetVector2 = vector [1.0f]

    [<Params (100,1001,1000001)>] 
    member val public Length = 0 with get, set

    [<Setup>]
    member self.SetupData () =        
        
        array <- Array.create self.Length 5
        //array2 <- Array.create self.Length 5
        //mathnetVector <- DenseVector.init self.Length (fun x -> (float32)(r.NextDouble()))
        
        //array3 <- Array.init self.Length (fun x -> (float32)(r.NextDouble()))
        //mathnetVector2 <- DenseVector.init self.Length (fun x -> (float32)(r.NextDouble()))

    
    
    
  



[<EntryPoint>]
let main argv =              
    
    
    

    let switch = 
        BenchmarkSwitcher [|
            typeof<SIMDBenchmark>
        |]

    switch.Run argv |> ignore
    0

