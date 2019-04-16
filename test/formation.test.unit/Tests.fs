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
let ``Can create a model of single Formation and override values`` () =

    let environment = "test"
    let rgName = "test_rg"

    let mySmallVm label name rg_name =
        // set custom company defaults
        AzureRM.virtual_machine VM_Size.Basic_A0 AzureRegion.westus (rg_name |> Terraform.makeName label) name environment
        |> fun vm -> {vm with delete_data_disks_on_termination = true}
        

    let vm = mySmallVm environment "test_vm_name" "azure_test_group_rm"

    Assert.NotNull(vm)

[<Fact>]
let ``Can serialize a model`` () =
    // Resource Group
    let rg = 
        AzureRM.resource_group "azure_test_group_rm"
        |> fun res -> { res with location = Some AzureRegion.westeurope }
    
    // define a custom function that bakes in your basic resource setup
    let standardVm label name resourceGroupName =
        AzureRM.virtual_machine VM_Size.Standard_A0 AzureRegion.westeurope (resourceGroupName |> Terraform.makeName label) name label     
    
    // environment would be sent in via a command line argument to generate TF for specific environment
    // Could have conditions on each environment written in code (ie. smaller sized machine for test)
    let envVMAmmend env vm =
        match env with
        | "test" -> vm |> fun res -> { res with vm_size = VM_Size.Basic_A0 }
        | _ -> vm

    let environment = "test"
    let tf = 
        standardVm environment "test_vm_name" rg.name
        |> envVMAmmend environment
        |> fun model -> Resource(environment, model :> obj)
        |> Terraform.serialize

    Assert.NotNull(String.IsNullOrEmpty tf)

type TestSimpleModel = {
    s : string
    i : int
    b : bool
    slist : string list
    os : string option
    oslist : (string list) option
    tags : ((string * string) list) option
}

type TestModelWithNesting = {
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

type TestModelWithDeepNesting = {
    s : string
    i : int
    b : bool
    slist : string list
    os : string option
    oslist : (string list) option
    tags : ((string * string) list) option
    ob : azurerm_resource_group
    sob : azurerm_resource_group option
    nestedO : azurerm_subnet
}

let testResource() =
    let resource = Resource ("test",{
            s = "a string"
            os = Some "somestring"
            i = 1
            b = true
            slist = ["a1";"a2"]
            oslist = Some ["s1";"s2"]
            tags = Some [("key","value")]
        }:>obj)

    resource

let testResourceWithNesting() =
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

    resource

[<Fact>]
let ``Can serialize string to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("""s = "a string""", tf) 

[<Fact>]
let ``Can serialize Some string to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("""os = "somestring""", tf)  

[<Fact>]
let ``Can serialize int to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("i = 1", tf)

[<Fact>]
let ``Can serialize boolean to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("b = true", tf)

[<Fact>]
let ``Can serialize list of string to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("""slist = ["a1", "a2"]""", tf)  

[<Fact>]
let ``Can serialize Some list of string to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("""oslist = ["s1", "s2"]""", tf)

[<Fact>]
let ``Can serialize Some list of tuple string*string to TF YAML`` () =
    let resource = testResource()
    let tf = Terraform.serialize resource
    Assert.Contains("""tags = {""", tf)
    Assert.Contains("key = \"value\"", tf)

[<Fact>]
let ``Can serialize an object to TF YAML`` () =
    let resource = testResourceWithNesting()
    let tf = Terraform.serialize resource
    Assert.Contains("""ob = {""", tf)
    Assert.Contains("name = \"azure_rm\"", tf)

[<Fact>]
let ``Can serialize an Some object to TF YAML`` () =
    let resource = testResourceWithNesting()
    let tf = Terraform.serialize resource
    Assert.Contains("""sob = {""", tf)
    Assert.Contains("name = \"some_azure_rm\"", tf)

[<Fact>]
let ``Can create a model of 2 Resource Formations and override values`` () =

    let environment = "test"
    let rgName = "test_rg"

    let mySmallVm label name rg_name =
        AzureRM.virtual_machine VM_Size.Basic_A0 AzureRegion.westus (rg_name |> Terraform.makeName label) name environment
        |> fun vm -> {vm with delete_data_disks_on_termination = true}
        

    let r1 = Resource ("test1", (mySmallVm environment "test_vm_name1" rgName):>obj)
    let r2 = Resource ("test2", (mySmallVm environment "test_vm_name2" rgName):>obj)
    let fm = Formation [r1;r2]
    let tf = Terraform.serialize fm
    Assert.Contains("""resource "azurerm_virtual_machine" "test1" """, tf)  
    Assert.Contains("name = \"test_vm_name1\"", tf)
    Assert.Contains("""resource "azurerm_virtual_machine" "test1" """, tf)  
    Assert.Contains("name = \"test_vm_name2\"", tf)

[<Fact>]
let ``Can serialize an object with nested to TF YAML`` () =
    let servd = AzureRM.service_delegation "servd"
    let del = AzureRM.delegation "d" servd
    let resource = Resource ("test_subnet", AzureRM.subnet "test_subnet" "rgn" "vnet" "ads" |> fun sub -> {sub with delegation = Some del}:>obj)
    let tf = Terraform.serialize resource
    Assert.Contains("""resource "azurerm_subnet" "test_subnet" {""", tf)    
    Assert.Contains("delegation = {", tf)    
    Assert.Contains("name = \"d\"", tf)    
    Assert.Contains("service_delegation = {", tf)    
    Assert.Contains("name = \"servd\"", tf)

[<Fact>]
let ``Can serialize an output`` () =
    let output = Output ("test", Terraform.var "default-ami")
    let tf = Terraform.serialize output
    Assert.Contains("""output "test" {""", tf)
    Assert.Contains("value = \"${var.default-ami}\"", tf)