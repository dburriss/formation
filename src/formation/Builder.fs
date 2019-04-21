namespace formation

type FormationBuilder() =
    member this.Yield(_) = Formation []
    
    [<CustomOperation("resource")>]
    member this.Resource<'a>(Formation xs, (s:string,a:'a)) = Formation (xs @ [Resource (s,(a:>obj))])

    [<CustomOperation("data")>]
    member this.Data(Formation xs, (s:string,a:'a)) = Formation (xs @ [Data (s,(a:>obj))])

    [<CustomOperation("output")>]
    member this.Output(Formation xs, (k,v)) = Formation (xs @ [Output (k,v)])
    

[<AutoOpen>]
module Builder =
    let formation = new FormationBuilder()