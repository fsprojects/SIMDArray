module SIMDArrayUtils

/// <summary>
/// Utility function for use with SIMD higher order functions
/// When you don't have leftover elements
/// example:
/// Array.SIMD.Map (fun x -> x*x) nop array
/// Where array is divisible by your SIMD width or you don't
/// care about what happens to the leftover elements
/// </summary>
let inline nop _ = Unchecked.defaultof<_>

let inline checkNonNull arg =
    match box arg with
    | null -> nullArg "array"
    | _ -> ()

