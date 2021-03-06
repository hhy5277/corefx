// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace System.IO.IsolatedStorage
{
    public class IsolatedStorageFileStream : FileStream
    {
        private const string BackSlash = "\\";
        private const int DefaultBufferSize = 1024;

        private FileStream _fs;
        private IsolatedStorageFile _isf;
        private string _givenPath;
        private string _fullPath;

        public IsolatedStorageFileStream(string path, FileMode mode)
            : this(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None, null)
        {
        }

        public IsolatedStorageFileStream(string path, FileMode mode, IsolatedStorageFile isf)
            : this(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None, isf)
        {
        }

        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access)
            : this(path, mode, access, access == FileAccess.Read ? FileShare.Read : FileShare.None, DefaultBufferSize, null)
        {
        }

        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
            : this(path, mode, access, access == FileAccess.Read ? FileShare.Read : FileShare.None, DefaultBufferSize, isf)
        {
        }

        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, DefaultBufferSize, null)
        {
        }

        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
            : this(path, mode, access, share, DefaultBufferSize, isf)
        {
        }

        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
            : this(path, mode, access, share, bufferSize, null)
        {
        }

        // If the isolated storage file is null, then we default to using a file
        // that is scoped by user, appdomain, and assembly.
        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, IsolatedStorageFile isf) :
            base (path, mode, access, share)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if ((path.Length == 0) || path.Equals(BackSlash))
                throw new ArgumentException(
                   SR.IsolatedStorage_Path);

            if (isf == null)
            {
                throw new ArgumentNullException(nameof(isf));
            }

            if (isf.Disposed)
                throw new ObjectDisposedException(null, SR.IsolatedStorage_StoreNotOpen);

            switch (mode)
            {
                case FileMode.CreateNew:        // Assume new file
                case FileMode.Create:           // Check for New file & Unreserve
                case FileMode.OpenOrCreate:     // Check for new file
                case FileMode.Truncate:         // Unreserve old file size
                case FileMode.Append:           // Check for new file
                case FileMode.Open:             // Open existing, else exception
                    break;

                default:
                    throw new ArgumentException(SR.IsolatedStorage_FileOpenMode);
            }

            _isf = isf;
            _givenPath = path;
            _fullPath = _isf.GetFullPath(_givenPath);

            try
            {
                _fs = new
                   FileStream(_fullPath, mode, access, share, bufferSize,
                       FileOptions.None);
            }
            catch (Exception e)
            {
                // Exception message might leak the IsolatedStorage path. The desktop prevented this by calling an
                // internal API which made sure that the exception message was scrubbed. However since the innerException
                // is never returned to the user(GetIsolatedStorageException() does not populate the innerexception
                // in retail bits we leak the path only under the debugger via IsolatedStorageException._underlyingException which
                // they can any way look at via IsolatedStorageFile instance as well.
                throw IsolatedStorageFile.GetIsolatedStorageException("IsolatedStorage_Operation_ISFS", e);
            }
        }

        public override bool CanRead
        {
            get
            {
                return _fs.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _fs.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _fs.CanSeek;
            }
        }

        public override long Length
        {
            get
            {
                return _fs.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _fs.Position;
            }
            set
            {
                _fs.Position = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_fs != null)
                        _fs.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            _fs.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _fs.FlushAsync();
        }

        public override void SetLength(long value)
        {
            _fs.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _fs.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, Threading.CancellationToken cancellationToken)
        {
            return _fs.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return _fs.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // Desktop implementation of IsolatedStorage ensures that in case the size is increased the new memory is zero'ed out.
            // However in this implementation we simply call the FileStream.Seek APIs which have an undefined behavior.
            return _fs.Seek(offset, origin);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _fs.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _fs.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            _fs.WriteByte(value);
        }

        public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            return _fs.BeginRead(array, offset, numBytes, userCallback, stateObject);
        }

        public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            return _fs.BeginWrite(array, offset, numBytes, userCallback, stateObject);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _fs.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _fs.EndWrite(asyncResult);
        }

        public override IntPtr Handle
        {
            get { return _fs.Handle; }
        }

        public override void Unlock(long position, long length)
        {
            _fs.Unlock(position, length);
        }

        public override void Lock(long position, long length)
        {
            _fs.Lock(position, length);
        }
    }
}
