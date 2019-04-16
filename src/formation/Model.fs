namespace formation

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