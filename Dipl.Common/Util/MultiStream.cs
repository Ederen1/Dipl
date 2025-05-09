using System.Security.Cryptography;

namespace Dipl.Common.Util;

/// <summary>
///     Concatenates multiple input streams into a single read-only stream.
///     Does not support seeking.
/// </summary>
public class MultiStream : Stream
{
    private readonly Stream[] _streams;
    private int _currentIndex; // Index of the current stream in the _streams array being read from.

    public MultiStream(params Stream[] streams)
    {
        if (streams == null || streams.Length == 0)
            throw new ArgumentException("At least one stream must be provided", nameof(streams));

        _streams = streams;
        _currentIndex = 0;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;

    // Length is not supported on a non-seekable concatenated stream.
    public override long Length => throw new NotSupportedException("The stream does not support seeking.");

    public override long Position
    {
        get => throw new NotSupportedException("The stream does not support seeking.");
        set => throw new NotSupportedException("The stream does not support seeking.");
    }

    public override void Flush()
    {
        /* no-op */
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var totalRead = 0;

        // Loop until we've filled the buffer or exhausted all streams.
        while (_currentIndex < _streams.Length && totalRead < buffer.Length)
        {
            var current = _streams[_currentIndex];
            var bytesRead = await current.ReadAsync(buffer[totalRead..], cancellationToken);

            if (bytesRead > 0)
                totalRead += bytesRead;
            else
                // Move to the next stream when the current one is exhausted.
                _currentIndex++;
        }

        return totalRead;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await ReadAsync(buffer.AsMemory().Slice(offset, count), cancellationToken);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            foreach (var s in _streams)
                try
                {
                    s.Dispose();
                }
                catch (NotSupportedException e) when (s is CryptoStream)
                {
                    // Ignore CryptoStream trying to flush a non-seekable stream
                }

        base.Dispose(disposing);
    }
}