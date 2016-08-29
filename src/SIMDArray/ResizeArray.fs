[<RequireQualifiedAccess>]
module Array.ResizeArray

open System.Numerics
open FSharp.Core
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.Operators

let inline private checkNonNull arg =
    match box arg with
    | null -> nullArg "array"
    | _ -> ()

