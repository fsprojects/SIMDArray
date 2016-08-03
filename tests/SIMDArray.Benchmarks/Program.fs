module Program


open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
#if MONO
#else
open BenchmarkDotNet.Diagnostics.Windows
#endif
                  



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
    let mutable mathnetVector = vector [1.0f]
    let mutable mathnetVector2 = vector [1.0f]

    [<Params (100, 1000,100000,1000000)>] 
    member val public Length = 0 with get, set

    [<Setup>]
    member self.SetupData () =        
        
        array <- Array.init self.Length (fun x -> (float32)(r.NextDouble()))
        mathnetVector <- DenseVector.init self.Length (fun x -> (float32)(r.NextDouble()))
        array2 <- Array.init self.Length (fun x -> (float32)(r.NextDouble()))
        mathnetVector2 <- DenseVector.init self.Length (fun x -> (float32)(r.NextDouble()))

    
    [<Benchmark>]
    member self.SIMDMap2 ()  = Array.SIMD.map2 (fun x y -> x+y) array array2

       
    [<Benchmark>]
    member self.MathNETSum ()  =  mathnetVector.Add(mathnetVector2)


        
[<EntryPoint>]
let main argv =              
    Control.UseNativeMKL()
    printf "%A\n" (Control.LinearAlgebraProvider.ToString())

    let switch = 
        BenchmarkSwitcher [|
            typeof<SIMDBenchmark>
        |]

    switch.Run argv |> ignore
    0

