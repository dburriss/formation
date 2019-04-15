namespace formation

module Terraform =
    open System
    open System.Text
    open System.Reflection
    open Microsoft.FSharp.Reflection     
    open System.Collections.Generic
    
    let mapProperty (pi:PropertyInfo) = pi |> fun p -> (p.Name, p.PropertyType)

    let isOption (t:Type) = t.IsGenericType && (t.GetGenericTypeDefinition() = typedefof<Option<_>>)

    let isOptionT<'a> (t:Type) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Option<'a>>

    let isEnumerableType<'a> (t:Type) =
        let check = typedefof<IEnumerable<_>>
        let ts = t.GetInterfaces() |> Array.filter(fun i -> i.IsGenericType && i.GetGenericTypeDefinition() = check)
        ts |> Array.isEmpty |> not

    let isSeqOf<'a> (t:Type) = 
        let isGeneric = t.IsGenericType
        let isSeq = isGeneric && isEnumerableType t
        isSeq

    let isTupleOf<'a> (t:Type) =
        let check = FSharpType.GetTupleElements typedefof<'a>
        let isTuple = t.IsGenericType && FSharpType.IsTuple(t.GetGenericTypeDefinition())
        let isTupleOfType = isTuple && (FSharpType.GetTupleElements t = check)
        isTupleOfType


    let (|IsStringType|IsBoolType|IsIntType|IsSomeStringType|IsSomeBoolType|IsSomeIntType|Unknown|) (t:Type) =
        if(t = typedefof<string>) then IsStringType
        elif(t = typedefof<bool>) then IsBoolType
        elif(t = typedefof<int>) then IsIntType
        elif(isOptionT<string> t) then IsSomeStringType
        elif(isOptionT<bool> t) then IsSomeBoolType
        elif(isOptionT<int> t) then IsSomeIntType
        else Unknown

    let (|StringValue|_|) (o:obj) =
        match o with
        | :? string as x -> x |> Some 
        | :? Option<string> as x -> x
        | _ -> None

    let (|BoolValue|_|) (o:obj) =
        match o with
        | :? bool as x -> x |> Some 
        | :? Option<bool> as x -> x
        | _ -> None

    let (|IntValue|_|) (o:obj) =
        match o with
        | :? int as x -> x |> Some 
        | :? Option<int> as x -> x
        | _ -> None

    let (|ListOfString|_|) (o:obj) =
        match o with
        | :? Option<string list> as x -> x
        | :? (string list) as x -> x |> Some
        | _ -> None

    let (|ListOfKeyValue|_|) (o:obj) =
        match o with
        | :? Option<(string*string)list> as x -> x
        | :? ((string*string) list) as x -> x |> Some
        | _ -> None
        //let t0 = o.GetType()
        //if(isOption<_> t0) then
        //    let t1 = t0.GetGenericArguments().[0]
        //    if(isSeqOf<_> t1) then 
        //        let t2 = t1.GetGenericArguments().[0]
        //        if(isTupleOf<string*string> t2) then
        //            Some [("a1","a2")]
        //        else None
        //    else None
        //else None

    let (|Obj|_|) (o:obj) =
        if(isNull o) then None
        else
            let props x = 
                x.GetType().GetProperties()
                |> Array.toList
                |> List.map (fun pi -> (pi.Name, pi.GetValue(x, null)))
            let t = o.GetType()
            let ps = 
                match o with
                | null -> []
                | x when isOption t -> 
                    let y = FSharpValue.GetUnionFields (x, x.GetType()) |> snd |> Seq.head :?> obj
                    props y
                | _ -> props o

            if(List.isEmpty ps) then None else Some ps

    let boolToString b = if(b) then "true" else "false"

    let times n (s:string) = 
        let mutable sb = StringBuilder()
        [0..n] |> List.iter (fun _ -> sb <- sb.Append(s))
        sb |> string

    let typeName model = model.GetType().Name.ToLower()
    

    let makeName model label =
        let n = model |> typeName
        sprintf "${%s.%s.name}" n label

    let makeId model label =
        let n = model |> typeName
        sprintf "${%s.%s.id}" n label
    
    let append (s:string) (sb:StringBuilder) = sb.Append(s) |> ignore
    let appendLine (s:string) (sb:StringBuilder) = sb.AppendLine(s) |> ignore

    //let rec serializeObject (sb:StringBuilder) depth o =

    let serializeResouce (sb:StringBuilder) (label, model)  = 
        sb |> appendLine(sprintf "resource \"%s\" \"%s\" {\n" (model |> typeName) label)
        let indent = "  "
        for pi in (model.GetType().GetProperties()) do
            let (n,t) = pi |> mapProperty
            let value = pi.GetValue(model, null)
            match value with
            | StringValue s -> sb.AppendLine(sprintf "%s%s = \"%s\"" indent n s) |> ignore
            | BoolValue b -> sb |> appendLine(sprintf "%s%s = %s" indent n (b|>boolToString))
            | IntValue i -> sb |> appendLine(sprintf "%s%s = %i" indent n i)
            | ListOfString xs ->
                let lst = xs |> Seq.toList
                match lst with
                | [] -> ignore()
                | [x] -> sb |> append(sprintf "%s%s = [\"%s\"]" indent n x)
                | h::t -> 
                    sb |> append(sprintf "%s%s = [\"%s\"" indent n h)
                    t |> Seq.iter (fun x -> sb |> append(sprintf ", \"%s\"" x))
                    sb |> appendLine("]")
            | ListOfKeyValue kvs -> 
                let lst = kvs |> Seq.toList
                match lst with
                | [] -> ignore()
                | kvs -> 
                    sb |> appendLine(sprintf "%s%s = {" indent n)
                    kvs |> List.iter(fun (k,v) -> sb |> appendLine(sprintf "%s%s = \"%s\"" (indent |> times 2) k v))
                    sb |> appendLine(sprintf "%s}" indent)
            | Obj ps -> 
                sb |> appendLine(sprintf "%s%s = {" indent n)
                ps |> List.iter(fun (k,v) -> sb |> appendLine(sprintf "%s%s = \"%s\"" (indent |> times 2) k (v.ToString())))
                sb |> appendLine(sprintf "%s}" indent)
            | _ -> ignore()
        sb |> append("}") 
        sb

    let serialize (formation:Formation) =
        let sb = StringBuilder()
        let rec loop s f =
            match f with
            | Resource x -> x |> serializeResouce s
            | _ -> s
        (loop sb formation) |> string