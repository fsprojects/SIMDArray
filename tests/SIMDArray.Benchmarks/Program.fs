module Program


open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
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
    let mutable array = [||]

    [<Params (100, 1000,1000000)>] 
    member val public Length = 0 with get, set

    [<Setup>]
    member self.SetupData () =
        array <- Array.create self.Length 5

    [<Benchmark>]
    member self.ArrayMap () = Array.map (fun x -> x+x) array
    
    [<Benchmark>]
    member self.SIMDMap ()  = Array.SIMD.map (fun x -> x+x) array


        
[<EntryPoint>]
let main argv =              

    let switch = 
        BenchmarkSwitcher [|
            typeof<SIMDBenchmark>
        |]

    switch.Run argv |> ignore
    0

