﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Utilities;

namespace Compression.FileHandling
{
    public sealed class BgzipTextWriter : StreamWriter, IDisposable
    {
        readonly BlockGZipStream _bgzipStream;
        private readonly byte[] _buffer;
        private int _bufferIndex;
        private const int BufferSize = BlockGZipStream.BlockGZipFormatCommon.BlockSize;

        public long Position => _bgzipStream.Position + _bufferIndex;


        #region IDisposable
        bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //Console.WriteLine($"disposing bgzipTextWriter, total bytes written:{_totalBytesWritten}");
                Flush();
                _bgzipStream.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion

        public BgzipTextWriter(string path) : this(new BlockGZipStream(FileUtilities.GetCreateStream(path), CompressionMode.Compress))
        {
        }

        public BgzipTextWriter(BlockGZipStream bgzipStream):base(bgzipStream)
        {
            _buffer = new byte[BufferSize];//4kb blocks
            _bgzipStream = bgzipStream;
        }

        public override void Flush()
        {
            if (_bufferIndex == 0) return;
            _bgzipStream.Write(_buffer, 0, _bufferIndex);
            _bufferIndex = 0;
        }

        public override void WriteLine(string s)
        {
            Write(s+"\n");
        }

        public override void Write(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            var lineBytes = Encoding.UTF8.GetBytes(line);

            if (lineBytes.Length <= BufferSize - _bufferIndex)
            {
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, lineBytes.Length);
                _bufferIndex += lineBytes.Length;
            }
            else
            {
                //fill up the buffer
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, BufferSize - _bufferIndex);
                var lineIndex = BufferSize - _bufferIndex;

                //write it out to the stream
                _bgzipStream.Write(_buffer, 0, BufferSize);
                _bufferIndex = 0;

                while (lineIndex + BufferSize <= lineBytes.Length)
                {
                    _bgzipStream.Write(lineBytes, lineIndex, BufferSize);
                    lineIndex += BufferSize;
                }
                //the leftover bytes should be saved in buffer
                if (lineIndex < lineBytes.Length)
                {
                    Array.Copy(lineBytes, lineIndex, _buffer, 0, lineBytes.Length - lineIndex);
                    _bufferIndex = lineBytes.Length - lineIndex;
                }

            }
        }

    }
}