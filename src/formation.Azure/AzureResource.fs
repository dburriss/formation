namespace formation

module AzureResource =
    open formation.Azure.Resources
    open formation.Azure.Constants

    let resource_group name =
        let model : azurerm_resource_group = {
            name = name
            location = Some AzureRegion.westus
            tags = Some []
        }
        model

    let virtual_machine size location resource_group_name name lbl  =
        let model : azurerm_virtual_machine = {
            name = name
            resource_group_name = resource_group_name
            location = location
            network_interface_ids = [""]
            os_profile_linux_config = None
            os_profile_windows_config = None
            vm_size = size
            availability_set_id = None
            boot_diagnostics = None
            delete_os_disk_on_termination = false
            delete_data_disks_on_termination  = false
            identity = None
            license_type = None
            os_profile = None
            os_profile_secrets = None
            plan = None
            primary_network_interface_id  = None
            storage_data_disk = None
            storage_image_reference = None
            storage_os_disk = None
            tags = None
            zones = None
            //attributes
            id = None
        }
        model

    let subnet name resource_group_name virtual_network_name address_prefix =
        let model : azurerm_subnet = {
            name = name
            resource_group_name = resource_group_name
            virtual_network_name = virtual_network_name
            address_prefix = address_prefix
            network_security_group_id = None
            route_table_id = None
            service_endpoints = None
            delegation = None

            id = None
            ip_configuration = None
        }
        model

    let delegation name service_delegation =
        let model : delegation = {
            name = name
            service_delegation = service_delegation
        }
        model

    let service_delegation name =
        let model : service_delegation = {
            name = name
            actions = None
        }
        model