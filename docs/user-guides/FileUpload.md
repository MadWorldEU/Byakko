# File Upload & Download

Byakko lets you upload a file and share it with anyone via a link — no account required to download.

## Uploading a file

1. Sign in to the Portal.
2. Navigate to **Storage → Upload** (`/storage/upload`).
3. Click the file picker and select a file (maximum 1000 MB).
4. Optionally, click **Advanced options** to set a password (see below).
5. Click **Upload**.
6. Once the upload completes, a shareable link appears. Click **Copy link** to copy it to your clipboard.

The link looks like:

```
https://<your-domain>/storage/download/<file-id>
```

Share this link with anyone — they do not need an account to download the file.

## Password-protecting a file

You can restrict a file so that only recipients who know the password can download it.

1. Before uploading, click **Advanced options** to expand the password section.
2. Either type your own password or click **Generate** to create a cryptographically secure 20-character password automatically.
3. Use the eye icon to show or hide the password field.
4. Proceed with the upload as normal.

> **Important:** the password is not stored or recoverable by Byakko. If you lose it the file cannot be decrypted. Make sure to share the password with your recipient through a separate, trusted channel.

## Downloading a file

Open the shareable link in any browser. The download page shows the file name, content type, and expiry date.

- If the file has **no password**, leave the password field empty and click **Download**.
- If the file is **password-protected**, enter the password in the field before clicking **Download**. An incorrect password will be rejected with an error.

## Security

Byakko applies several layers of protection to your uploaded files.

**Encryption at rest** — every file is encrypted with AES-256 before it is written to object storage. The encryption key is never stored alongside the file. The file is decrypted on the fly when you download it and is never written to disk in plaintext.

**Password protection** — if you set a password at upload time, the AES-256 key is derived from a combination of the server key and your password using PBKDF2 (600 000 iterations, SHA-256) with a unique per-file salt. Without the correct password the file cannot be decrypted, even by the server. Byakko does not store or transmit your password after the upload completes.

**Ownership enforcement** — only the user who created the upload record can supply the actual file content. Any attempt by a different user to upload content for the same record is rejected with a `403 Forbidden` error.

**Expiry** — every upload has a validity period (default 30 days). Once a file expires it can no longer be downloaded, and its content is permanently deleted from storage during the next scheduled cleanup.

**Authentication** — uploading requires a valid account and a signed JWT token issued by the identity server. Downloading is intentionally public so recipients do not need an account.

**Rate limiting** — upload and download endpoints apply a stricter request limit (default 20 requests per minute per user) on top of the global API limit (default 100 requests per minute) to prevent abuse.

**Transport security** — all traffic between the client, the API, and object storage is encrypted in transit via HTTPS/TLS.

## Notes

- You must be signed in to upload. Downloading is public.
- Only the user who uploaded a file can re-upload its content (ownership is enforced server-side).
- The file name and content type set at upload time are fixed and cannot be changed afterwards.