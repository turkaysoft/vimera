using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Vimera{
    public static class TSCrcChecksumModule{
        internal static readonly uint[] Crc32Table;
        internal static readonly ulong[] Crc64Table;
        static TSCrcChecksumModule(){
            // CRC32 Table Create (ISO 3309 / Ethernet)
            uint poly32 = 0xEDB88320;
            Crc32Table = new uint[256];
            for (uint i = 0; i < 256; i++){
                uint temp = i;
                for (int j = 8; j > 0; j--){
                    if ((temp & 1) == 1)
                        temp = (temp >> 1) ^ poly32;
                    else
                        temp >>= 1;
                }
                Crc32Table[i] = temp;
            }
            // CRC64 Table Create (ECMA-182)
            ulong poly64 = 0x42F0E1EBA9EA3693;
            Crc64Table = new ulong[256];
            for (ulong i = 0; i < 256; i++){
                ulong temp = i;
                for (int j = 0; j < 8; j++){
                    if ((temp & 1) == 1)
                        temp = (temp >> 1) ^ poly64;
                    else
                        temp >>= 1;
                }
                Crc64Table[i] = temp;
            }
        }
        // Instead of creating a MemoryStream, we return the byte array directly
        public static uint CalculateCrc32(string input){
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < bytes.Length; i++){
                crc = (crc >> 8) ^ Crc32Table[(crc & 0xFF) ^ bytes[i]];
            }
            return ~crc;
        }
        public static ulong CalculateCrc64(string input){
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            ulong crc = 0;
            for (int i = 0; i < bytes.Length; i++){
                crc = (crc >> 8) ^ Crc64Table[(crc & 0xFF) ^ bytes[i]];
            }
            return crc;
        }
        public static uint ComputeCrc32(Stream stream){
            uint crc = 0xFFFFFFFF;
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0){
                for (int i = 0; i < bytesRead; i++){
                    crc = (crc >> 8) ^ Crc32Table[(crc & 0xFF) ^ buffer[i]];
                }
            }
            return ~crc;
        }
        public static ulong ComputeCrc64(Stream stream){
            ulong crc = 0;
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0){
                for (int i = 0; i < bytesRead; i++){
                    crc = (crc >> 8) ^ Crc64Table[(crc & 0xFF) ^ buffer[i]];
                }
            }
            return crc;
        }
    }
    // CRC32 Class - Now sharing the table from TSCrcChecksumModule
    public class TSCrc32 : HashAlgorithm{
        private uint crc;
        public TSCrc32(){
            HashSizeValue = 32;
            Initialize();
        }
        public override void Initialize(){
            crc = 0xFFFFFFFF;
        }
        protected override void HashCore(byte[] array, int ibStart, int cbSize){
            for (int i = 0; i < cbSize; i++){
                byte b = array[ibStart + i];
                crc = (crc >> 8) ^ TSCrcChecksumModule.Crc32Table[(crc & 0xFF) ^ b];
            }
        }
        protected override byte[] HashFinal(){
            uint finalCrc = ~crc;
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(finalCrc);
            bytes[1] = (byte)(finalCrc >> 8);
            bytes[2] = (byte)(finalCrc >> 16);
            bytes[3] = (byte)(finalCrc >> 24);
            return bytes;
        }
    }
    // CRC64 Class - Now sharing the table from TSCrcChecksumModule
    public class TSCrc64 : HashAlgorithm{
        private ulong crc;
        public TSCrc64(){
            HashSizeValue = 64;
            Initialize();
        }
        public override void Initialize(){
            crc = 0;
        }
        protected override void HashCore(byte[] array, int ibStart, int cbSize){
            for (int i = 0; i < cbSize; i++){
                byte b = array[ibStart + i];
                crc = (crc >> 8) ^ TSCrcChecksumModule.Crc64Table[(crc & 0xFF) ^ b];
            }
        }
        protected override byte[] HashFinal(){
            byte[] bytes = new byte[8];
            bytes[0] = (byte)(crc);
            bytes[1] = (byte)(crc >> 8);
            bytes[2] = (byte)(crc >> 16);
            bytes[3] = (byte)(crc >> 24);
            bytes[4] = (byte)(crc >> 32);
            bytes[5] = (byte)(crc >> 40);
            bytes[6] = (byte)(crc >> 48);
            bytes[7] = (byte)(crc >> 56);
            return bytes;
        }
    }
}