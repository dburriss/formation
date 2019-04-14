namespace formation

//data
type Formation =
| Provider of obj
| Resource of (string * obj)
| Data of obj
| Output of string
| Formation of Formation list

module Formation =
    
    let provider model = Provider model
    let resource (label, model) = Resource (label, model)