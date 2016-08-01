module Test 

open System.Numerics
open System
open System.Diagnostics
open FsCheck
open NUnit                  
open NUnit.Framework
open Swensen.Unquote


let inline compareNums a b =
    let fa = float a
    let fb = float b
    a.Equals b || float(a - b) < 0.00001 || float(b -  a) < 0.00001 ||
    Double.IsNaN fa && Double.IsInfinity fb || Double.IsNaN fb && Double.IsInfinity fa

let inline arrayComp (a: 'T[]) (b: 'T[]) =
    Array.fold (&&) true (Array.zip a b |> Array.map (fun (aa,bb) -> compareNums aa bb ))

let inline areEqual (xs: 'T []) (ys: 'T []) =
    match xs, ys with
    | null, null -> true
    | [||], [||] -> true
    | null, _ | _, null -> false
    | _ when xs.Length <> ys.Length -> false
    | _ ->
        let mutable break' = false
        let mutable i = 0
        let mutable result = true
        while i < xs.Length && not break' do
            if xs.[i] <> ys.[i] then 
                break' <- true
                result <- false
            i <- i + 1
        result

open FsCheck.Gen

let inline lenAbove num = Gen.where (fun a -> (^a:(member Length:int)a) > num)
let inline lenBelow num = Gen.where (fun a -> (^a:(member Length:int)a) < num)
let inline between a b = lenAbove a >> lenBelow b

let arrayArb<'a> =  
    Gen.arrayOf Arb.generate<'a> 
    |> between 1000 10000 |> Arb.fromGen


let testCount = 10000
let config = 
    { Config.Quick with 
        MaxTest = testCount
        StartSize = 1
    }

let quickCheck prop = Check.One(config, prop)
let sickCheck fn = Check.One(config, Prop.forAll arrayArb fn)

[<Test>]                  
let ``SIMD.map = Array.Map`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > Vector<int>.Count && xs <> [||]) ==>
        let plusA   xs = xs |> Array.SIMD.map (fun x -> x+x)
        let plusB   xs = xs |> Array.map (fun x -> x+x)
        let multA   xs = xs |> Array.SIMD.map (fun x -> x*x)
        let multB   xs = xs |> Array.map (fun x -> x*x)
        let minusA  xs = xs |> Array.SIMD.map (fun x -> x-x)
        let minusB  xs = xs |> Array.map (fun x -> x-x)
        (lazy test <@ plusA xs = plusB xs @>)   |@ "map x + x" .&.
        (lazy test <@ multA xs = multB xs @>)   |@ "map x * x" .&.
        (lazy test <@ minusA xs = minusB xs @>) |@ "map x - x" 

  

[<Test>]                  
let ``SIMD.sum = Array.sum`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > Vector<int>.Count && array <> [||]) ==>
        lazy (Array.SIMD.sum array = Array.sum array)


[<Test>]                  
let ``SIMD.average = Array.average`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > Vector<float>.Count && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.average array) (Array.average array)))


[<Test>]                  
let ``SIMD.max = Array.max`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > Vector<float>.Count && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.max array) (Array.max array)))


[<Test>]                  
let ``SIMD.min = Array.min`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > Vector<float>.Count && array <> [||]) ==>
        lazy (compareNums (Array.SIMD.min array) (Array.min array))


let inline testMap (array:'T[]) =
        
    let a = array |> Array.SIMD.map (fun x -> x*x)
    let b = array |> Array.map (fun x -> x*x)

    let c = array |> Array.SIMD.map (fun x -> x+x)
    let d = array |> Array.map (fun x -> x+x)

    let e = array |> Array.SIMD.map (fun x -> x-x)
    let f = array |> Array.map (fun x -> x-x)

    let g = array |> Array.SIMD.map (fun x -> x)
    let h = array |> Array.map (fun x -> x)

    arrayComp a b && arrayComp c d && arrayComp e f && arrayComp g h


let inline testFold (array:'T[]) =
        
    let a = array |> Array.SIMD.fold (+) (+) Unchecked.defaultof<'T>
    let b = array |> Array.fold (+) Unchecked.defaultof<'T>
                
    compareNums a b


let inline testMinMax (array:'T[]) =
    if array.Length <> 0 then
        let a = array |> Array.SIMD.min
        let b = array |> Array.min

        let c = array |> Array.SIMD.max
        let d = array |> Array.max
                
        compareNums a b && compareNums c d
    else 
        true

let inline testSum (array:'T[]) =
    let a = array |> Array.SIMD.sum
    let b = array |> Array.sum

    compareNums a b

let inline testAverage (array:'T[]) =
    if array.Length = 0 then
        true
    else
        let a = array |> Array.SIMD.average
        let b = array |> Array.average

        compareNums a b

let inline testCreate count =
    if count < 1 then 
        true 
    else
        let a = Array.SIMD.create count 5
        let b = Array.create count 5

        let c = Array.SIMD.create count 5.0
        let d = Array.create count 5.0

        arrayComp a b && arrayComp c d
    

let inline testInit count =
    if count < 1 then 
        true 
    else
        let a = Array.SIMD.init count (fun i -> Vector<float>((float)i))
        let b = Array.init count (fun i -> (float)i)

        let c = Array.SIMD.init count (fun i -> Vector<int>(i))
        let d = Array.init count (fun i -> i)

        arrayComp a b && arrayComp c d

let testInt32 f (array:int[]) =
    f array
        
let testInt64 f (array:int64[]) =
    f array
        
let testFloat f (array:float[]) =
    f array
        
let testFloat32 f (array:float32[]) =
    f array


//let main argv =              
//
//    
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
//    
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
        
    0
        
            
