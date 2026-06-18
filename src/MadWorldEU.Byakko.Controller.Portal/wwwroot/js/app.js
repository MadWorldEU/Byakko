window.initTooltips = function () {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        bootstrap.Tooltip.getOrCreateInstance(el);
    });
};

window.downloadFileWithPassword = async function (url, password, dotNetRef) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ password: password ?? null })
        });

        if (!response.ok) {
            const error = await response.json();
            if (error.code === 'Encryption.DecryptionFailed') {
                return 'The password is incorrect.';
            }
            return error.description ?? 'Download failed.';
        }

        const contentDisposition = response.headers.get('Content-Disposition');
        let fileName = 'download';
        if (contentDisposition) {
            const rfc5987 = contentDisposition.match(/filename\*\s*=\s*UTF-8''([^;\r\n]*)/i);
            if (rfc5987) {
                fileName = decodeURIComponent(rfc5987[1].trim());
            } else {
                const plain = contentDisposition.match(/filename\s*=\s*"([^"]+)"|filename\s*=\s*([^;\r\n]+)/i);
                if (plain) fileName = (plain[1] ?? plain[2]).trim();
            }
        }

        const contentLength = parseInt(response.headers.get('Content-Length') ?? '0', 10);
        const reader = response.body.getReader();
        const chunks = [];
        let received = 0;
        let lastPercent = -1;

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            chunks.push(value);
            received += value.length;
            if (contentLength > 0) {
                const percent = Math.min(99, Math.round((received / contentLength) * 100));
                if (percent !== lastPercent) {
                    lastPercent = percent;
                    await dotNetRef.invokeMethodAsync('UpdateProgress', percent);
                }
            }
        }

        await dotNetRef.invokeMethodAsync('UpdateProgress', 100);

        const blob = new Blob(chunks);
        const objectUrl = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = objectUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(objectUrl);

        return null;
    } catch {
        return 'Download failed.';
    }
};