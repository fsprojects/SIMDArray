(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r "System.Numerics"
#r @"../../bin/SIMDArray/SIMDArray.dll"
#r @"../../bin/SIMDArray/System.Numerics.Vectors.dll"


(**
Introducing your project
========================

Say more

*)

let inline testMap (array:'T[]) =
    let a = array |> Array.SIMD.map (fun x -> x*x)
    let b = array |> Array.map (fun x -> x*x)
    ()



(**
Some more info
*)
