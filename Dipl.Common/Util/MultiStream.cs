namespace Dipl.Common.Util
{
    /// <summary>
    /// Concatenates multiple input streams into a single read-only stream.
    /// Does not support seeking.
    /// </summary>
    public class MultiStream : Stream
    {
        private readonly Stream[] _streams;
        private int _currentIndex;

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

        // Length is not supported on a non-seekable concatenated stream
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { /* no-op */ }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(buffer));

            var totalRead = 0;

            // Loop until we've read the requested count or no more data
            while (count > 0 && _currentIndex < _streams.Length)
            {
                var current = _streams[_currentIndex];
                var bytesRead = current.Read(buffer, offset, count);

                if (bytesRead > 0)
                {
                    totalRead += bytesRead;
                    offset += bytesRead;
                    count -= bytesRead;
                }
                else
                {
                    // Move to next stream when current is exhausted
                    _currentIndex++;
                }
            }

            return totalRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Optionally dispose inner streams
                foreach (var s in _streams)
                    s.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
