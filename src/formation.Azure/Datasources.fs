namespace formation.Azure.Datasources

type azurerm_resource_group = {
    name : string
    location : string option
    tags : ((string * string) list) option
}

/// Use this data source to access information about an existing Public IP Address.
type azurerm_public_ip = {
    /// Specifies the name of the public IP address.
    name : string
    /// Specifies the name of the resource group.
    resource_group_name : string
    //attributes
    domain_name_label : string option
    idle_timeout_in_minutes : string option
    fqdn : string option
    ip_address : string option
    ip_version : string option
    tags : ((string * string) list) option
}