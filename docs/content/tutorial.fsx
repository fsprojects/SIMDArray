(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../src/SIMDArray/bin/Release/netstandard2.1"
#r "SIMDArray.dll"

(**
Introducing your project
========================

Say more

*)

let inline testMap (array:'T[]) =
    let a = array |> Array.SIMD.map (fun x -> x*x) (fun x -> x*x)
    let b = array |> Array.map (fun x -> x*x)
    ()



(**
Some more info
*)
