# Containers

The container diagram shows the internal structure of the Byakko platform.

- **Portal** — Blazor WebAssembly frontend for end users. Communicates with the Api.
- **Admin** — Blazor WebAssembly frontend for administrators. Communicates with the Api.
- **Api** — ASP.NET Core backend. Handles all business logic and delegates storage to the Database Schema and Minio.
- **KeyCloak** — Identity and access management server. Provides JWT-based authentication for the Api, Portal, and Admin.
- **Database Schema** — PostgreSQL database. Stores asset metadata and application state.
- **Minio** — S3-compatible object storage. Stores the binary content of uploaded assets.

![Containers](images/Containers.svg)