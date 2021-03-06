﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace BinStorage.Extensions
{
    public static class ObjectSerializer
    {
        public static Stream Serialize(this Object obj)
        {
            if (obj == null)
                return null;

            var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, obj);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public static Object Deserialize(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(stream);
            }
        }

        public static Stream Compress(this Stream stream)
        {
            MemoryStream result = new MemoryStream();
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    stream.CopyTo(zip);
                }
                byte[] resBytes = outputStream.ToArray();
                result.Write(resBytes, 0, resBytes.Length);
                result.Seek(0, SeekOrigin.Begin);
            }

            return result;
        }

        public static Stream Decompress(this Stream stream)
        {
            string tempFilePath = Path.GetTempFileName();
            FileStream result = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

            using (Stream clonedStream = new MemoryStream())
            {
                stream.CopyTo(clonedStream);
                stream.Seek(0, SeekOrigin.Begin);
                clonedStream.Seek(0, SeekOrigin.Begin);
                using (var zip = new GZipStream(clonedStream, CompressionMode.Decompress))
                {
                    zip.CopyTo(result);
                }
            }
            result.Seek(0, SeekOrigin.Begin);

            return result;
        }
    }
}
