namespace MadWorldEU.Byakko.Streams;

/// <summary>Wraps a stream and reports upload progress as a percentage via a callback.</summary>
internal sealed class ProgressStream(Stream inner, long totalBytes, Func<int, Task> onProgress) : Stream
{
    private long _bytesRead;
    private int _lastReportedPercent = -1;

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await inner.ReadAsync(buffer, cancellationToken);
        if (read > 0)
        {
            _bytesRead += read;
            var percent = totalBytes > 0 ? (int)(_bytesRead * 100 / totalBytes) : 0;
            if (percent != _lastReportedPercent)
            {
                _lastReportedPercent = percent;
                await onProgress(percent);
            }
        }
        return read;
    }

    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => inner.Length;
    public override long Position { get => inner.Position; set => inner.Position = value; }
    public override void Flush() => inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    public override void SetLength(long value) => inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            inner.Dispose();
        }
        base.Dispose(disposing);
    }
}