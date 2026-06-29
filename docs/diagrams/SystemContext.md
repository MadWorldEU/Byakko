# System Context

The system context diagram shows the Byakko platform, the people who interact with it, and the external systems it depends on.

- **User** — accesses the platform via the Portal to upload, manage, and retrieve files.
- **Administrator** — manages the platform via the Admin interface.
- **OVHCloud** — external S3-compatible object storage provider. Byakko stores AES-256 encrypted file content here in production.
- **Proton Mail** — external SMTP mail provider. Byakko uses it to deliver feedback submissions from the Contact page to the administrator.

![System Context](images/SystemContext.svg)