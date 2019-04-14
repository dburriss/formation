module Tests

open System
open Xunit
open formation
open formation.Azure.Constants
open formation.Azure

// notes
// Functions return with type like Resource / Data
// CE could allow back to wrapped type
// What type? obj sucks

[<Fact>]
let ``Can create a model and override values`` () =

    let environment = "test"
    let rgName = "test_rg"

    let mySmallVm label name resource_group_name =
        // set custom company defaults
        AzureRM.virtual_machine VM_Size.Basic_A0 AzureRegion.westus (rgName |> Terraform.makeName label) "mytestvm" environment
        |> fun vm -> {vm with delete_data_disks_on_termination = true}
        

    let vm = mySmallVm environment "test_vm_name" "azure_test_group_rm"

    Assert.NotNull(vm)

[<Fact>]
let ``Can serialize a model`` () =
    
    let rg = 
        AzureRM.resource_group "azure_test_group_rm"
        |> fun res -> { res with location = Some AzureRegion.westeurope }
    
    // define a custom function that bakes in your basic resource setup
    let standardVm label name resourceGroupName =
        AzureRM.virtual_machine VM_Size.Standard_A0 AzureRegion.westeurope (resourceGroupName |> Terraform.makeName label) name label     
    
    // environment would be sent in via a command line argument to generate TF for environment
    // Could have conditions on each environment written in code
    let environment = "test"
    let tf = 
        standardVm environment "test_vm_name" rg.name
        |> fun model -> Resource(environment, model :> obj)
        |> Terraform.serialize

    Assert.NotNull(String.IsNullOrEmpty tf)

type TestModel = {
    s : string
    i : int
    b : bool
    slist : string list
    os : string option
    oslist : (string list) option
    tags : ((string * string) list) option
    ob : azurerm_resource_group
    sob : azurerm_resource_group option
}

[<Fact>]
let ``Can serialize resource to string`` () =
    
    let resource = Resource ("test",{
        s = "a string"
        os = Some "somestring"
        i = 1
        b = true
        slist = ["a1";"a2"]
        oslist = Some ["s1";"s2"]
        tags = Some [("key","value")]
        ob = AzureRM.resource_group "azure_rm"
        sob = Some (AzureRM.resource_group "some_azure_rm")
    }:>obj)
    let tf = Terraform.serialize resource
    
    Assert.NotNull(String.IsNullOrEmpty tf)    
    
    Assert.Contains("""s = "a string""", tf)    
    
    Assert.Contains("""os = "somestring""", tf)    
    
    Assert.Contains("i = 1", tf)
    
    Assert.Contains("b = true", tf)    
    
    Assert.Contains("""slist = ["a1", "a2"]""", tf)    
    
    Assert.Contains("""oslist = ["s1", "s2"]""", tf)   
    
    Assert.Contains("""tags = {""", tf)
    Assert.Contains("key = \"value\"", tf)

    Assert.Contains("""ob = {""", tf)
    Assert.Contains("name = \"azure_rm\"", tf)

    Assert.Contains("""sob = {""", tf)
    Assert.Contains("name = \"some_azure_rm\"", tf)