namespace formation

type Blocks<'a>() =
    member private this.data = Array.empty<'a>
    //new ctor with array
    static member map = fun f (xs:Blocks<'a>) -> xs.data |> Array.map f
    member this.Map(f) = this.data |> Array.map f |> id


//data
type Formation =
| Provider of obj
| Resource of (string * obj)
| Data of (string * obj)
| Output of string * string
//| Module ?
| Formation of Formation list

module Formation =
    
    let provider model = Provider model
    let resource (label, model) = Resource (label, model)    
    let data (label, model) = Data (label, model)
    let output (label,value) = Output (label,value)

    let rec bind (func:obj -> Formation) (expr:Formation) =
        match expr with
        | Provider value -> func value
        | Resource value | Data value | Data value  -> func value
        | Output (label,value) -> func (label, value)
        | Formation [] -> Formation []
        | Formation formations -> formations |> List.map (bind func) |> Formation
