module ResizeArray


open FSharp.Core
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.Operators

let inline private checkNonNull arg =
    match box arg with
    | null -> nullArg "resizearray"
    | _ -> ()


let inline partition f (array: ResizeArray<_>) = 
    checkNonNull array

    let res1 = ResizeArray<_>()
    let res2 = ResizeArray<_>()

    for i = 0 to array.Count-1 do
        let e = array.[i]
        if f e then
            res1.Add(e)
        else
            res2.Add(e)

    res1,res2

let inline map f (array: ResizeArray<_>) =
    
    checkNonNull array

    let res = ResizeArray<_>(array.Count)
    for i = 0 to array.Count-1 do
        res.[i] <- f array.[i]

    res

let unfold<'T,'State> (f:'State -> ('T*'State) option) (s:'State) =
    let res = ResizeArray<_>()
    let rec loop state =
        match f state with
        | None -> ()
        | Some (x,s') ->
            res.Add(x)
            loop s'
    loop s
    res


