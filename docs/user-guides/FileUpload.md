# File Upload & Download

Byakko lets you upload a file and share it with anyone via a link — no account required to download.

## Uploading a file

1. Sign in to the Portal.
2. Navigate to **Storage → Upload** (`/storage/upload`).
3. Click the file picker and select a file (maximum 100 MB).
4. Click **Upload**.
5. Once the upload completes, a shareable link appears. Click **Copy link** to copy it to your clipboard.

The link looks like:

```
https://<your-domain>/storage/download/<file-id>
```

Share this link with anyone — they do not need an account to download the file.

## Downloading a file

Open the shareable link in any browser. The download page shows the file name and type. Click **Download** to save the file.

## Security

Byakko applies several layers of protection to your uploaded files.

**Encryption at rest** — every file is encrypted with AES-256 before it is written to object storage. The encryption key is never stored alongside the file. The file is decrypted on the fly when you download it and is never written to disk in plaintext.

**Ownership enforcement** — only the user who created the upload record can supply the actual file content. Any attempt by a different user to upload content for the same record is rejected with a `403 Forbidden` error.

**Expiry** — every upload has a validity period (default 30 days). Once a file expires it can no longer be downloaded, and its content is permanently deleted from storage during the next scheduled cleanup.

**Authentication** — uploading requires a valid account and a signed JWT token issued by the identity server. Downloading is intentionally public so recipients do not need an account.

**Rate limiting** — upload and download endpoints apply a stricter request limit (default 20 requests per minute per user) on top of the global API limit (default 100 requests per minute) to prevent abuse.

**Transport security** — all traffic between the client, the API, and object storage is encrypted in transit via HTTPS/TLS.

## Notes

- You must be signed in to upload. Downloading is public.
- Only the user who uploaded a file can re-upload its content (ownership is enforced server-side).
- The file name and content type set at upload time are fixed and cannot be changed afterwards.