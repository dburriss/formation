namespace formation.Azure

open System

//provider
type azurerm = {
    /// The version number for the AzureRM Provider
    Version : string option
    client_id : string option
    environemnt : string option
    subscription_id : string option
    tenant_id : string option
    client_certificate_password : string option
    client_certificate_path : string option
    client_secret : string option
    msi_endpoint : string option
    use_msi : bool
    partner_id : Guid option
    skip_credentials_validation : bool
    skip_provider_registration : bool
}

//resource
type azurerm_resource_group = {
    name : string
    location : string option
    tags : ((string * string) list) option
}


type azurerm_public_ip = {
    name : string
    resource_group_name : string
    location : string
    sku : string option
    allocation_method : string
    ip_version : string option
    idle_timeout_in_minutes : string option
    domain_name_label : string option
    reverse_fqdn : string option
    tags : ((string * string) list) option
    zones : (string list) option
    //attributes        
    id : string option
    ip_address : string option
    fqdn : string option
}


type azurerm_network_interface = {
    name : string
    resource_group_name : string
    location : string option
    network_security_group_id : string
    internal_dns_name_label : string option
    enable_ip_forwarding : string option
    enable_accelerated_networking : bool
    dns_servers : string list
    ip_configuration : ip_configuration list
    tags : (string * string) list
    //attributes
    id : string option
    mac_address : string option
    private_ip_address : string option
    private_ip_addresses : string option
    virtual_machine_id : string option
    applied_dns_servers : (string list) option
}
and ip_configuration = {
    name : string
    subnet_id : string
    private_ip_address : string
    private_ip_address_allocation : string
    public_ip_address_id : string
    application_gateway_backend_address_pools_ids : string list
    load_balancer_backend_address_pools_ids : string list
    load_balancer_inbound_nat_rules_ids : string list
    application_security_group_ids : string list
    primary : bool
}


type azurerm_subnet = {
    name : string
    resource_group_name : string
    virtual_network_name  : string
    address_prefix : string
    network_security_group_id : string option
    route_table_id : string option
    service_endpoints : string option
    delegation : delegation option

    id : string option
    ip_configuration : (string list) option
}
and delegation = {
    name : string
    service_delegation : service_delegation
}
and service_delegation = {
    name : string
    actions : (string list) option
}


type identity = {
    Type : string
    identity_ids : (string list) option
}


type azurerm_virtual_machine = {
    name : string
    resource_group_name : string
    location : string
    network_interface_ids : string list
    os_profile_linux_config : os_profile_linux_config option
    os_profile_windows_config : os_profile_windows_config option
    vm_size : string
    availability_set_id : string option
    boot_diagnostics : boot_diagnostics option
    delete_os_disk_on_termination : bool
    delete_data_disks_on_termination  : bool
    identity : identity option
    license_type : string option
    os_profile : os_profile option
    os_profile_secrets : os_profile_secrets option
    plan : plan option
    primary_network_interface_id  : string option
    storage_data_disk : (storage_data_disk list) option
    storage_image_reference : storage_image_reference option
    storage_os_disk : storage_os_disk option
    tags : ((string * string) list) option
    zones : (string list) option
    //attributes
    id : string option
}
and os_profile_linux_config = {
    disable_password_authentication : bool
    ssh_keys : (ssh_keys list) option
}
and ssh_keys = {
    key_data : string
    path : string
}
and os_profile_windows_config = {
    provision_vm_agent : bool
    enable_automatic_upgrades : bool
    timezone : string option
    winrm : winrm
}
and winrm = {
    protocol : string
    certificate_url :string option
}
and boot_diagnostics = {
    enabled : bool
    storage_uri : string    
}
and os_profile = {
    computer_name : string
    admin_username : string
    admin_password : string
    custom_data : string
}
and os_profile_secrets = {
    source_vault_id : string
    vault_certificates : vault_certificates list
}
and vault_certificates = {
    certificate_url : string
    certificate_store : string option
}
and plan = {
    name: string
    publisher : string
    product : string
}
and storage_data_disk = {
    name : string
    caching : string option
    create_option : string
    disk_size_gb : string
    lun : string
    write_accelerator_enabled : bool

    managed_disk_type : string option
    managed_disk_id : string option

    vhd_uri : string option
}
and storage_image_reference = {
    publisher : string
    offer : string
    sku : string
    version : string option
}
and storage_os_disk = {
    name : string
    create_option : string
    caching : string option
    disk_size_gb : string option
    image_uri : string option
    os_type : string option
    write_accelerator_enabled : bool
    managed_disk_id : string option
    managed_disk_type : string option
    vhd_uri : string option
}