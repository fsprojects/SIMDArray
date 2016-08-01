module Program

open System.Numerics
open System
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

    [<Params (1000, 100000, 10000000, 100000000)>] 
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

//let inline compareNums a b =
//    let fa = float a
//    let fb = float b
//    a.Equals b || float(a - b) < 0.00001 || float(b -  a) < 0.00001 ||
//    Double.IsNaN fa && Double.IsInfinity fb || Double.IsNaN fb && Double.IsInfinity fa
//
//let inline arrayComp (a: 'T[]) (b: 'T[]) =
//    Array.fold (&&) true (Array.zip a b |> Array.map (fun (aa,bb) -> compareNums aa bb ))
//        
//
//let inline testMap (array:'T[]) =
//        
//    let a = array |> Array.SIMD.map (fun x -> x*x)
//    let b = array |> Array.map (fun x -> x*x)
//
//    let c = array |> Array.SIMD.map (fun x -> x+x)
//    let d = array |> Array.map (fun x -> x+x)
//
//    let e = array |> Array.SIMD.map (fun x -> x-x)
//    let f = array |> Array.map (fun x -> x-x)
//
//    let g = array |> Array.SIMD.map (fun x -> x)
//    let h = array |> Array.map (fun x -> x)
//
//    arrayComp a b && arrayComp c d && arrayComp e f && arrayComp g h
//
//
//let inline testFold (array:'T[]) =
//        
//    let a = array |> Array.SIMD.fold (+) (+) Unchecked.defaultof<'T>
//    let b = array |> Array.fold (+) Unchecked.defaultof<'T>
//                
//    compareNums a b
//
//
//let inline testMinMax (array:'T[]) =
//    if array.Length <> 0 then
//        let a = array |> Array.SIMD.min
//        let b = array |> Array.min
//
//        let c = array |> Array.SIMD.max
//        let d = array |> Array.max
//                
//        compareNums a b && compareNums c d
//    else 
//        true
//
//let inline testSum (array:'T[]) =
//    let a = array |> Array.SIMD.sum
//    let b = array |> Array.sum
//
//    compareNums a b
//
//let inline testAverage (array:'T[]) =
//    if array.Length = 0 then
//        true
//    else
//        let a = array |> Array.SIMD.average
//        let b = array |> Array.average
//
//        compareNums a b
//
//let inline testCreate count =
//    if count < 1 then 
//        true 
//    else
//        let a = Array.SIMD.create count 5
//        let b = Array.create count 5
//
//        let c = Array.SIMD.create count 5.0
//        let d = Array.create count 5.0
//
//        arrayComp a b && arrayComp c d
//    
//
//let inline testInit count =
//    if count < 1 then 
//        true 
//    else
//        let a = Array.SIMD.init count (fun i -> Vector<float>((float)i))
//        let b = Array.init count (fun i -> (float)i)
//
//        let c = Array.SIMD.init count (fun i -> Vector<int>(i))
//        let d = Array.init count (fun i -> i)
//
//        arrayComp a b && arrayComp c d
//
//let testInt32 f (array:int[]) =
//    f array
//        
//let testInt64 f (array:int64[]) =
//    f array
//        
//let testFloat f (array:float[]) =
//    f array
//        
//let testFloat32 f (array:float32[]) =
//    f array

//    let s = Stopwatch()
//    let a = Array.create 10000000 5
//    let b = Array.create 10000000 5
//
//    s.Start()
//    Array.Clear( a, 0, a.Length)
//    s.Stop()
//    let t1 = s.ElapsedTicks
//
//    s.Restart()
//    Array.SIMD.clear b 0 b.Length
//    s.Stop();
//    let t2 = s.ElapsedTicks
//
//    printf "t1:%A  t2:%A\n" t1 t2
//    printf "a:%A\n b:%A\n" a b 
    
//    let testCount = 100000
//    let config = {Config.Quick with MaxTest = testCount}
//
//    printf "Test Create\n"
//    Check.One ({config with StartSize = 1},testCreate)
//
//    printf "Test Init\n"
//    Check.One ({config with StartSize = 1},testInit)
//
//
//    printf "***Test Int32***\n"
//    printf "Test Map\n"
//    Check.One (config, testInt32 testMap)
//    printf "Test Fold\n"
//    Check.One (config, testInt32 testFold)
//    printf "Test MinMax\n"
//    Check.One (config, testInt32 testMinMax)
//    printf "Test Sum\n"
//    Check.One (config, testInt32 testSum)
//        
//    printf "***Test Int64***\n"
//    printf "Test Map\n"
//    Check.One (config, testInt64 testMap)
//    printf "Test Fold\n"
//    Check.One (config, testInt64 testFold)
//    printf "Test MinMax\n"
//    Check.One (config, testInt64 testMinMax)
//    printf "Test Sum\n"
//    Check.One (config, testInt64 testSum)

    (*
    printf "Test Float\n"
    printf "Test Map\n"
    Check.One (config, testFloat testMap)
    printf "Test Fold\n"
    Check.One (config, testFloat testFold)
    printf "Test MinMax\n"
    Check.One (config, testFloat testMinMax)
    printf "Test Sum\n"
    Check.One (config, testFloat testSum)
    printf "Test Average\n"
    Check.One (config, testFloat testAverage)

    printf "Test Float32\n"
    printf "Test Map\n"
    Check.One (config, testFloat32 testMap)
    printf "Test Fold\n"
    Check.One (config, testFloat32 testFold)
    printf "Test MinMax\n"
    Check.One (config, testFloat32 testMinMax)
    printf "Test Sum\n"
    Check.One (config, testFloat32 testSum)
    printf "Test Average\n"
    Check.One (config, testFloat32 testAverage)
    *)
        

        
            
