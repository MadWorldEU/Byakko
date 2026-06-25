workspace "Byakko" "A file sharing platform for uploading, managing, and retrieving digital assets." {

    !identifiers hierarchical

    model {
        user = person "User"
        administrator = person "Administrator"
        system = softwareSystem "Byakko" {
            description "VPS"
            api = container "Api" {
                technology "Asp.Net Core"
                url "https://api.mad-world.eu"
            }
            status = container "Status" {
                technology "Asp.Net Core"
                url "https://status.mad-world.eu"
            }
            admin = container "Admin" {
                technology "Blazor WASM"
                url "https://admin.mad-world.eu"
            }
            portal = container "Portal" {
                technology "Blazor WASM"
                url "https://fileshare.mad-world.eu"
            }
            keycloak = container "KeyCloak" {
                technology "Java"
                url "https://authentication.mad-world.eu"
            }
            db = container "Database Schema" {
                tags "Database"
                technology "Postgresql"
                url "https://database.mad-world.eu"
            }
        }
        ovhcloud = softwareSystem "OvhCloud" {
            description "Cloud Service"
        }
        proton = softwareSystem "Proton" {
            description "Mail Service"
        }

        user -> system.portal "Uses"
        administrator -> system.admin "Uses"
        system.portal -> system.api "Uses"
        system.admin -> system.api "Uses"
        system.status -> system.api "Health check"
        system.api -> system.keycloak "Authorize"
        system.api -> system.db "Reads from and writes to"
        system.api -> ovhcloud "Downloads from and uploads to"
        system.api -> proton "Sends mail"
        system.keycloak -> proton "Sends mail"
    }

    views {
        systemContext system "SystemContext" {
            include *
        }

        container system "Containers" {
            include *
        }

        styles {
            element "Element" {
                color #0773af
                stroke #0773af
                strokeWidth 7
                shape roundedbox
            }
            element "Person" {
                shape person
            }
            element "Database" {
                shape cylinder
            }
            element "Storage" {
                shape cylinder
            }
            element "Boundary" {
                strokeWidth 5
            }
            relationship "Relationship" {
                thickness 4
            }
        }
    }

    configuration {
        scope softwaresystem
    }

}