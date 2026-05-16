workspace "Byakko" "A file sharing platform for uploading, managing, and retrieving digital assets." {

    !identifiers hierarchical

    model {
        user = person "User"
        administrator = person "Administrator"
        system = softwareSystem "Byakko" {
            api = container "Api"
            admin = container "Admin"
            portal = container "Portal"
            keycloak = container "KeyCloak"
            db = container "Database Schema" {
                tags "Database"
            }
            minio = container "Minio" {
                tags "Storage"
            }
        }

        user -> system.portal "Uses"
        administrator -> system.admin "Uses"
        system.portal -> system.api "Uses"
        system.admin -> system.api "Uses"
        system.api -> system.db "Reads from and writes to"
        system.api -> system.minio "Downloads from and uploads to"
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