namespace formation.Azure

module AzureDatasource =
    
    open formation.Azure.Datasources

    let public_ip name resource_group_name =
        let model : azurerm_public_ip = {
            name = name
            resource_group_name = resource_group_name
            //attributes
            domain_name_label = None
            idle_timeout_in_minutes = None
            fqdn = None
            ip_address = None
            ip_version = None
            tags = None
        }
        model