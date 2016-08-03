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
    |> between 1 10000 |> Arb.fromGen


let testCount = 10000
let config = 
    { Config.Quick with 
        MaxTest = testCount
        StartSize = 1
    }

let quickCheck prop = Check.One(config, prop)
let sickCheck fn = Check.One(config, Prop.forAll arrayArb fn)

[<Test>]                  
let ``SIMD.map = Array.map`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
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
let ``SIMD.map2 = Array.map2`` () =
    quickCheck <|
    fun (xs: int []) (xs2: int[]) ->
        (xs.Length > 0 && xs <> [||] ) ==>
        let xs2 = xs |> Array.map(fun x -> x+1)
        let plusA   xs xs2 = xs |> Array.SIMD.map2 (fun x y -> x+y) xs2
        let plusB   xs xs2 = xs |> Array.map2 (fun x y -> x+y) xs2
        let multA   xs xs2 = xs |> Array.SIMD.map2 (fun (x:Vector<int>) (y:Vector<int>) -> x*y) xs2
        let multB   xs xs2 = xs |> Array.map2 (fun x y -> x*y) xs2
        let minusA  xs xs2 = xs |> Array.SIMD.map2 (fun x y -> x-y) xs2
        let minusB  xs xs2 = xs |> Array.map2 (fun x y -> x-y) xs2
        (lazy test <@ plusA xs xs2 = plusB xs xs2 @>)   |@ "map x + y" .&.
        (lazy test <@ multA xs xs2 = multB xs xs2 @>)   |@ "map x * y" .&.
        (lazy test <@ minusA xs xs2 = minusB xs xs2 @>) |@ "map x - y" 

[<Test>]                  
let ``SIMD.mapi = Array.mapi`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let plusA   xs = xs |> Array.SIMD.mapi (fun i x -> x+x)
        let plusB   xs = xs |> Array.mapi (fun i x -> x+x)
        let multA   xs = xs |> Array.SIMD.mapi (fun i x -> x*x)
        let multB   xs = xs |> Array.mapi (fun i x -> x*x)
        let minusA  xs = xs |> Array.SIMD.mapi (fun i x -> x-x)
        let minusB  xs = xs |> Array.mapi (fun i x -> x-x)
        (lazy test <@ plusA xs = plusB xs @>)   |@ "map x + x" .&.
        (lazy test <@ multA xs = multB xs @>)   |@ "map x * x" .&.
        (lazy test <@ minusA xs = minusB xs @>) |@ "map x - x" 

  

[<Test>]                  
let ``SIMD.sum = Array.sum`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.sum array = Array.sum array)

[<Test>]                  
let ``SIMD.contains = Array.contains`` () =
    quickCheck <|
    fun (array: int []) (value:int) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.contains value array = Array.contains value array)

[<Test>]                  
let ``SIMD.exists = Array.exists`` () =
    quickCheck <|
    fun (array: int []) (value:int) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.exists (fun x -> Vector.EqualsAny(Vector<int>(value),x)) array = Array.exists (fun x -> x = value) array)


[<Test>]                  
let ``SIMD.average = Array.average`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.average array) (Array.average array)))


[<Test>]                  
let ``SIMD.max = Array.max`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.max array) (Array.max array)))


[<Test>]                  
let ``SIMD.min = Array.min`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (compareNums (Array.SIMD.min array) (Array.min array))





        
            
