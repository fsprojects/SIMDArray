module Test 

open System.Numerics
open System
open FsCheck
open NUnit.Framework
open Swensen.Unquote

//horizontal ops
let inline horizontal (f : ^T -> ^T -> ^T) (v :Vector< ^T>) : ^T =
    let mutable acc = Unchecked.defaultof< ^T>
    for i = 0 to Vector< ^T>.Count-1 do
        acc <- f acc v.[i]
    acc

let inline compareNums a b =
    let fa = float a
    let fb = float b
    a.Equals b || abs(float(a - b)) < 0.00001 ||
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


let inline lenAbove num = Gen.where (fun a -> (^a:(member Length:int)a) > num)
let inline lenBelow num = Gen.where (fun a -> (^a:(member Length:int)a) < num)
let inline between a b = lenAbove a >> lenBelow b

let arrayArb<'a> =
    Gen.arrayOf Arb.generate<'a> 
    |> between 1 10000 |> Arb.fromGen

let config testCount =
    { Config.QuickThrowOnFailure with 
        MaxTest = testCount
        StartSize = 1
    }

let quickCheck prop = Check.One(config 10000, prop)
let quickerCheck prop = Check.One(config 900, prop)
let sickCheck fn = Check.One(config 10000, Prop.forAll arrayArb fn)

[<Test>]
let ``SIMDParallel.sum`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMDParallel.sum array = Array.sum array)

[<Test>]                  
let ``SIMDParallel.map = Array.map`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let plusA   xs = xs |> Array.SIMDParallel.map (fun x -> x+x) (fun x -> x+x)
        let plusB   xs = xs |> Array.map (fun x -> x+x)
        let multA   xs = xs |> Array.SIMDParallel.map (fun x -> x*x) (fun x -> x*x)
        let multB   xs = xs |> Array.map (fun x -> x*x)
        let minusA  xs = xs |> Array.SIMDParallel.map (fun x -> x-x) (fun x -> x-x)
        let minusB  xs = xs |> Array.map (fun x -> x-x)
        (lazy test <@ plusA xs = plusB xs @>)   |@ "map x + x" .&.
        (lazy test <@ multA xs = multB xs @>)   |@ "map x * x" .&.
        (lazy test <@ minusA xs = minusB xs @>) |@ "map x - x" 


[<Test>]
let ``Performance.filter`` () =
    quickCheck <|
    fun (xs: int[]) (n : int) ->
        (xs.Length > 0 && xs <> [||]) ==>       
        lazy(            
             Array.filter (fun x -> x = n) xs = Array.Performance.filterEqual n xs &&
             Array.filter (fun x -> x < n) xs = Array.Performance.filterLessThan n xs &&
             Array.filter (fun x -> x > n) xs = Array.Performance.filterGreaterThan n xs &&
             Array.filter (fun x -> x <= n) xs = Array.Performance.filterLEq n xs &&
             Array.filter (fun x -> x >= n) xs = Array.Performance.filterGEq n xs &&
             Array.filter (fun x -> x*2 = n) xs = Array.Performance.filterSimplePredicate (fun x -> x*2 = n) xs &&
             Array.where (fun x -> x*2 = n) xs = Array.Performance.whereSimplePredicate (fun x -> x*2 = n) xs
            )

[<Test>]
let ``SIMD.forAll`` () =
    quickCheck <|
    fun (xs: int []) (n :int) ->
        (xs.Length > 0 && xs <> [||]) ==>       
        lazy(
            Array.forall(fun x -> x < n) xs = Array.SIMD.forall (fun x -> Vector.LessThanAll(x,Vector<int>(n))) (fun x -> x < n) xs
            )

[<Test>]
let ``SIMD.forAll2`` () =
    quickCheck <|
    fun (xs: int []) (n :int) ->
        (xs.Length > 0 && xs <> [||]) ==>       
        let xs2 = Array.map (fun x -> x + 1) xs
        lazy(
            Array.forall2(fun x y -> x+y < n) xs xs2 = Array.SIMD.forall2 (fun x y -> Vector.LessThanAll(x+y,Vector<int>(n))) (fun x y -> x+y < n) xs xs2
            )

[<Test>]
let ``SIMD.skipWhile`` () =
    quickCheck <|
    fun (xs: int []) (n :int) ->
        (xs.Length > 0 && xs <> [||]) ==>       
        lazy(
            Array.skipWhile(fun x -> x < n) xs = Array.SIMD.skipWhile (fun x -> Vector.LessThanAll(x,Vector<int>(n))) (fun x -> x < n) xs
            )
[<Test>]
let ``SIMD.takeWhile`` () =
    quickCheck <|
    fun (xs: int []) (n :int) ->
        (xs.Length > 0 && xs <> [||]) ==>       
        lazy(
            Array.takeWhile(fun x -> x < n) xs = Array.SIMD.takeWhile (fun x -> Vector.LessThanAll(x,Vector<int>(n))) (fun x -> x < n) xs
            )
            


[<Test>]
let ``SIMD.tryPick`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
       
        lazy(
            let n = Array.max xs
            Array.tryPick (fun x -> if x = n then Some n else None) xs = Array.SIMD.tryPick (fun x -> if Vector.EqualsAny(Vector<int>(n),x) then Some n else None) (fun x -> if x = n then Some n else None) xs
            )
[<Test>]
let ``SIMD.pick`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
       
        lazy(
            let n = Array.max xs
            Array.pick (fun x -> if x = n then Some n else None) xs = Array.SIMD.pick (fun x -> if Vector.EqualsAny(Vector<int>(n),x) then Some n else None) (fun x -> if x = n then Some n else None) xs
            )

[<Test>]
let ``SIMD.min with initial NaN`` () = 
    let data = [|14.0; nan; 1.0; 0.0|]
    let min     = data |> Array.min
    let simdMin = data |> Array.SIMD.min
    Assert.AreEqual(min, simdMin)

[<Test>]
let ``SIMD.max with initial NaN`` () = 
    let data = [|14.0; nan; 1.0; 17.0|]
    let max     = data |> Array.max
    let simdMax = data |> Array.SIMD.max
    Assert.AreEqual(max, simdMax)

[<Test>]
let ``SIMD.find`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
       
        lazy(
            let n = Array.max xs
            Array.find (fun x -> x = n) xs = Array.SIMD.find (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.findIndex`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        
        lazy(
            let n = Array.max xs
            Array.findIndex (fun x -> x = n) xs = Array.SIMD.findIndex (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.findBack`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>        
        lazy(
            let n = Array.max xs
            Array.findBack (fun x -> x = n) xs = Array.SIMD.findBack (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.findIndexBack`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>        
        lazy(
            let n = Array.max xs
            Array.findIndexBack (fun x -> x = n) xs = Array.SIMD.findIndexBack (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.tryFind`` () =
    quickCheck <|
    fun (xs: int []) (n : int) ->
        (xs.Length > 0 && xs <> [||]) ==>       
        lazy(            
            Array.tryFind (fun x -> x = n) xs = Array.SIMD.tryFind (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.tryFindIndex`` () =
    quickCheck <|
    fun (xs: int []) (n:int) ->
        (xs.Length > 0 && xs <> [||]) ==>        
        lazy(            
            Array.tryFindIndex (fun x -> x = n) xs = Array.SIMD.tryFindIndex (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.tryFindBack`` () =
    quickCheck <|
    fun (xs: int []) (n:int)->
        (xs.Length > 0 && xs <> [||]) ==>        
        lazy(            
            Array.tryFindBack (fun x -> x = n) xs = Array.SIMD.tryFindBack (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )

[<Test>]
let ``SIMD.tryFindIndexBack`` () =
    quickCheck <|
    fun (xs: int []) (n:int)->
        (xs.Length > 0 && xs <> [||]) ==>        
        lazy(            
            Array.tryFindIndexBack (fun x -> x = n) xs = Array.SIMD.tryFindIndexBack (fun x -> Vector.EqualsAny(Vector<int>(n),x)) (fun x -> x = n) xs
            )


[<Test>]
let ``SIMD.iter`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let mutable vsum = Vector<int>(0)
        let mutable leftovers = 0
        Array.SIMD.iter (fun x -> vsum <- vsum + x) (fun x -> leftovers <- leftovers + x)  xs
        leftovers <- leftovers + (horizontal (+) vsum)
        lazy (leftovers = Array.sum xs)

[<Test>]
let ``SIMD.iteri`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let mutable vsum = Vector<int>(0)
        let mutable leftovers = 0
        Array.SIMD.iteri (fun _ x -> vsum <- vsum + x) (fun _ x -> leftovers <- leftovers + x)  xs
        leftovers <- leftovers + (horizontal (+) vsum)
        lazy (leftovers = Array.sum xs)


[<Test>]
let ``SIMD.iter2`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let mutable vsum = Vector<int>(0)
        let mutable leftovers = 0
        Array.SIMD.iter2 (fun x y -> vsum <- vsum + x + y) (fun x y -> leftovers <- leftovers + x + y)  xs xs
        leftovers <- leftovers + (horizontal (+) vsum)
        lazy (leftovers = Array.sum xs * 2)

[<Test>]
let ``SIMD.iteri2`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let mutable vsum = Vector<int>(0)
        let mutable leftovers = 0
        Array.SIMD.iteri2 (fun _ x y -> vsum <- vsum + x + y) (fun _ x y -> leftovers <- leftovers + x + y)  xs xs
        leftovers <- leftovers + (horizontal (+) vsum)
        lazy (leftovers = Array.sum xs * 2)

[<Test>]                  
let ``SIMD.clear = Array.clear`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let A xs = Array.SIMD.clear xs 0 xs.Length
        let B xs = Array.Clear(xs, 0, xs.Length)
        (lazy test <@ A xs = B xs @>)   |@ "clear" 


[<Test>]                  
let ``SIMD.create = Array.create`` () =
    quickerCheck <|
    fun (len: int) (value: int) ->
        (len >= 0 ) ==>
        let A (len:int) (value:int) = Array.SIMD.create len value
        let B (len:int) (value:int) = Array.create len value        
        (lazy test <@ A len value = B len value @>)   |@ "create len value" 

[<Test>]                  
let ``SIMD.replicate = Array.replicate`` () =
    quickerCheck <|
    fun (len: int) (value: int) ->
        (len >= 0 ) ==>
        let A (len:int) (value:int) = Array.SIMD.replicate len value
        let B (len:int) (value:int) = Array.replicate len value        
        (lazy test <@ A len value = B len value @>)   |@ "create len value" 


[<Test>]                  
let ``SIMD.init = Array.init`` () =
    quickerCheck <|
    fun (len: int) (n : int) ->
        (len >= 0 ) ==>
        let A (len:int) = Array.SIMD.init len (fun i -> Vector<int>(n)) (fun i -> n)
        let B (len:int) = Array.init len (fun i -> n)
        (lazy test <@ A len = B len  @>)   |@ "init len" 


[<Test>]                  
let ``SIMD.map = Array.map`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let plusA   xs = xs |> Array.SIMD.map (fun x -> x+x) (fun x -> x+x)
        let plusB   xs = xs |> Array.map (fun x -> x+x)
        let multA   xs = xs |> Array.SIMD.map (fun x -> x*x) (fun x -> x*x)
        let multB   xs = xs |> Array.map (fun x -> x*x)
        let minusA  xs = xs |> Array.SIMD.map (fun x -> x-x) (fun x -> x-x)
        let minusB  xs = xs |> Array.map (fun x -> x-x)
        (lazy test <@ plusA xs = plusB xs @>)   |@ "map x + x" .&.
        (lazy test <@ multA xs = multB xs @>)   |@ "map x * x" .&.
        (lazy test <@ minusA xs = minusB xs @>) |@ "map x - x" 


[<Test>]                  
let ``SIMD.mapInPlace = Array.map`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let plusA   xs =  
            let copy = Array.copy xs
            copy |> Array.SIMD.mapInPlace (fun x -> x+x) (fun x -> x+x)
            copy
        let plusB   xs = xs |> Array.map (fun x -> x+x)
        let multA   xs = 
            let copy = Array.copy xs
            copy |> Array.SIMD.mapInPlace  (fun x -> x*x) (fun x -> x*x)
            copy
        let multB   xs = xs |> Array.map (fun x -> x*x)
        let minusA  xs = 
            let copy = Array.copy xs
            copy |> Array.SIMD.mapInPlace  (fun x -> x-x) (fun x -> x-x)
            copy
        let minusB  xs = xs |> Array.map (fun x -> x-x)
        (lazy test <@ plusA xs = plusB xs @>)   |@ "mapInPlace x + x" .&.
        (lazy test <@ multA xs = multB xs @>)   |@ "mapInPlace x * x" .&.
        (lazy test <@ minusA xs = minusB xs @>) |@ "mapInPlace x - x" 


[<Test>]                  
let ``SIMD.map2 = Array.map2`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||] ) ==>
        let xs2 = xs |> Array.map(fun x -> x+1)
        let plusA   xs xs2 = xs |> Array.SIMD.map2 (fun x y -> x+y) (fun x y -> x+y) xs2
        let plusB   xs xs2 = xs |> Array.map2 (fun x y -> x+y) xs2
        let multA   xs xs2 = xs |> Array.SIMD.map2 (fun (x:Vector<int>) (y:Vector<int>) -> x*y) (fun (x:int) (y:int) -> x*y) xs2
        let multB   xs xs2 = xs |> Array.map2 (fun x y -> x*y) xs2
        let minusA  xs xs2 = xs |> Array.SIMD.map2 (fun x y -> x-y) (fun x y -> x-y) xs2
        let minusB  xs xs2 = xs |> Array.map2 (fun x y -> x-y) xs2
        (lazy test <@ plusA xs xs2 = plusB xs xs2 @>)   |@ "map2 x + y" .&.
        (lazy test <@ multA xs xs2 = multB xs xs2 @>)   |@ "map2 x * y" .&.
        (lazy test <@ minusA xs xs2 = minusB xs xs2 @>) |@ "map2 x - y" 

[<Test>]                  
let ``SIMD.map3 = Array.map3`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||] ) ==>
        let xs2 = xs |> Array.map(fun x -> x+1)
        let xs3 = xs |> Array.map(fun x -> x-1)
        let plusA   xs xs2 xs3 = xs |> Array.SIMD.map3 (fun x y z -> x+y+z) (fun x y z -> x+y+z) xs2 xs3
        let plusB   xs xs2 xs3 = xs |> Array.map3 (fun x y z -> x+y+z) xs2 xs3
        let multA   xs xs2 xs3 = xs |> Array.SIMD.map3 (fun (x:Vector<int>) (y:Vector<int>) (z:Vector<int>)-> x*y*z) (fun x y z -> x*y*z) xs2 xs3
        let multB   xs xs2 xs3 = xs |> Array.map3 (fun x y z-> x*y*z) xs2 xs3
        let minusA  xs xs2 xs3 = xs |> Array.SIMD.map3 (fun x y z-> x-y-z) (fun x y z-> x-y-z) xs2 xs3
        let minusB  xs xs2 xs3 = xs |> Array.map3 (fun x y z-> x-y-z) xs2 xs3
        (lazy test <@ plusA xs xs2 xs3 = plusB xs xs2 xs3 @>)   |@ "map3 x + y + z" .&.
        (lazy test <@ multA xs xs2 xs3 = multB xs xs2 xs3 @>)   |@ "map3 x * y * z" .&.
        (lazy test <@ minusA xs xs2 xs3 = minusB xs xs2 xs3 @>) |@ "map3 x - y - z" 

[<Test>]                  
let ``SIMD.mapi = Array.mapi`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let plusA   xs = xs |> Array.SIMD.mapi (fun i x -> x+x) (fun i x -> x+x)
        let plusB   xs = xs |> Array.mapi (fun i x -> x+x)
        let multA   xs = xs |> Array.SIMD.mapi (fun i x -> x*x) (fun i x -> x*x)
        let multB   xs = xs |> Array.mapi (fun i x -> x*x)
        let minusA  xs = xs |> Array.SIMD.mapi (fun i x -> x-x) (fun i x -> x-x)
        let minusB  xs = xs |> Array.mapi (fun i x -> x-x)
        (lazy test <@ plusA xs = plusB xs @>)   |@ "mapi x + x" .&.
        (lazy test <@ multA xs = multB xs @>)   |@ "mapi x * x" .&.
        (lazy test <@ minusA xs = minusB xs @>) |@ "mapi x - x" 

[<Test>]                  
let ``SIMD.mapi2 = Array.mapi2`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let xs2 = xs |> Array.map(fun x -> x+1)
        let plusA   xs xs2 = xs |> Array.SIMD.mapi2 (fun i x y -> x+y) (fun i x y -> x+y) xs2
        let plusB   xs xs2 = xs |> Array.mapi2 (fun i x y -> x+y) xs2
        let multA   xs xs2 = xs |> Array.SIMD.mapi2 (fun i x y -> (x:Vector<int>)*(y:Vector<int>)) (fun i x y -> x*y) xs2
        let multB   xs xs2 = xs |> Array.mapi2 (fun i x y -> x*y) xs2
        let minusA  xs xs2 = xs |> Array.SIMD.mapi2 (fun i x y -> x-y) (fun i x y -> x-y) xs2
        let minusB  xs xs2 = xs |> Array.mapi2 (fun i x y -> x-y) xs2
        (lazy test <@ plusA xs xs2 = plusB xs xs2 @>)   |@ "mapi2 x + y" .&.
        (lazy test <@ multA xs xs2 = multB xs xs2 @>)   |@ "mapi2 x * y" .&.
        (lazy test <@ minusA xs xs2 = minusB xs xs2 @>) |@ "mapi2 x - y" 

[<Test>]
let ``SIMDcompareWith = Array.compareWith`` () =
    quickCheck <|
    fun (array1 : int[]) (array2 : int[]) ->
        (array1.Length > 0 && array1 <> [||] && array2.Length > 0 && array2 <> [||]) ==>
        lazy (Array.SIMD.compareWith (fun x y -> if Vector.EqualsAny(x,y) then 1 else 0) (fun x y -> if x = y then 1 else 0 ) array1 array2 = Array.compareWith (fun x y -> if x = y then 1 else 0) array1 array2   )
        

[<Test>]                  
let ``SIMD.sum = Array.sum`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.sum array = Array.sum array)


[<Test>]                  
let ``SIMD.sumBy = Array.sumBy`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.sumBy (fun x -> x*x) (fun x -> x*x) array = Array.sumBy (fun x -> x*x) array)

[<Test>]                  
let ``SIMD.reduce = Array.reduce`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.reduce (+) (+) (+) array = Array.reduce (+) array)

[<Test>]                  
let ``SIMD.reduceBack = Array.reduceBack`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.reduceBack (+) (+) (+) array = Array.reduceBack (+) array)


[<Test>]                  
let ``SIMD.fold = Array.fold`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.fold (+) (+) (+) 0 array = Array.fold (+) 0 array)

[<Test>]                  
let ``SIMD.foldBack = Array.foldBack`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.foldBack (+) (+) (+) array 0 = Array.foldBack (+) array 0)

[<Test>]                  
let ``SIMD.fold2 = Array.fold2`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        let array2 = Array.map (fun x -> x*x) array
        lazy (Array.SIMD.fold2 (fun acc x y -> acc + x + y) (fun acc x y -> acc+x+y) (+) 0 array array2 = Array.fold2 (fun acc x y -> acc+x+y) 0 array array2)

[<Test>]                  
let ``SIMD.foldBack2 = Array.foldBack2`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        let array2 = Array.map (fun x -> x*x) array
        lazy (Array.SIMD.foldBack2 (fun acc x y -> acc + x + y) (fun acc x y -> acc+x+y) (+) array array2 0 = Array.foldBack2 (fun acc x y -> acc+x+y) array array2 0)



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
        lazy (Array.SIMD.exists (fun x -> Vector.EqualsAny(Vector<int>(value),x)) (fun x -> x = value) array = Array.exists (fun x -> x = value) array)

[<Test>]                  
let ``SIMD.exists2 = Array.exists2`` () =
    quickCheck <|
    fun (array: int []) (value:int) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (Array.SIMD.exists2 (fun x y-> Vector.EqualsAny(Vector<int>(value),x+y)) (fun x y -> x+y = value) array array = Array.exists2 (fun x y -> x+y = value) array array)


[<Test>]                  
let ``SIMD.average = Array.average`` () =
    quickCheck <|
    fun () -> 
        lazy ((compareNums (Array.SIMD.average [|1.0;2.0;3.0;4.0;5.0;6.0;7.0;8.0;9.0|]) (Array.average [|1.0;2.0;3.0;4.0;5.0;6.0;7.0;8.0;9.0|])))

[<Test>]                  
let ``SIMD.averageBy = Array.averageBy`` () =
    quickCheck <|
    fun () -> 
        lazy ((compareNums (Array.SIMD.averageBy (fun x -> x*x) (fun x -> x*x) [|1.0;2.0;3.0;4.0;5.0;6.0;7.0;8.0;9.0|]) (Array.averageBy (fun x -> x*x) [|1.0;2.0;3.0;4.0;5.0;6.0;7.0;8.0;9.0|])))


[<Test>]                  
let ``SIMD.max = Array.max`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.max array) (Array.max array)))

[<Test>]                  
let ``SIMD.maxBy = Array.maxBy`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.maxBy (fun x -> x+x) (fun x -> x+x) array) (Array.maxBy (fun x -> x+x) array)))

[<Test>]                  
let ``SIMD.minBy`` () =
    let xs = [| -5..-1 |]
    Assert.AreEqual(-1, xs |> Array.minBy (fun x -> -x))
    Assert.AreEqual(-1, xs |> Array.SIMD.minBy (fun v -> -v) (fun x -> -x))

[<Test>]                  
let ``SIMD.minBy = Array.minBy`` () =
    quickCheck <|
    fun (array: int []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy ((compareNums (Array.SIMD.minBy (fun x -> x+x) (fun x -> x+x) array) (Array.minBy (fun x -> x+x) array)))


[<Test>]                  
let ``SIMD.min = Array.min`` () =
    quickCheck <|
    fun (array: float []) ->
        (array.Length > 0 && array <> [||]) ==>
        lazy (compareNums (Array.SIMD.min array) (Array.min array))
    
[<Test>]                  
let ``SIMD.dot = multiply and sum`` () =
    quickCheck <|
    fun (xs: int []) ->
        (xs.Length > 0 && xs <> [||]) ==>
        let xs2 = Array.init xs.Length id
        lazy ((compareNums (Array.SIMD.dot xs xs2) (Array.fold2 (fun a x y -> a + x*y) 0 xs xs2)))





        
            
