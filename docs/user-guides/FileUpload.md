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

## Notes

- You must be signed in to upload. Downloading is public.
- Only the user who uploaded a file can re-upload its content (ownership is enforced server-side).
- The file name and content type set at upload time are fixed and cannot be changed afterwards.