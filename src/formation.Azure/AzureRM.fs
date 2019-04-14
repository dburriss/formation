namespace formation

module AzureRM =
    open formation.Azure
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