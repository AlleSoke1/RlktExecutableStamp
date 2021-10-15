using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RLKTExecutableStamp
{
    class Program
    {
        static int ReservedBytes = 4 + 48; // header + SHA384 data + smallChecksum(Sha384)
        static UInt32 Header = 0x47655267; // gReG

        public static byte[] DoChecksum(byte[] data)
        {
            using (var algo = new SHA384Managed())
            {
                return algo.ComputeHash(data);
            }
        }

        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Invalid usage!");
                return;
            }
            string filename = args[0];

            if (File.Exists(filename))
            {
                //Get all bytes
                byte[] bytes = File.ReadAllBytes(filename);

                //Check if file is already packed by header
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
                {
                    reader.BaseStream.Seek(bytes.Length - ReservedBytes, SeekOrigin.Begin);
                    UInt32 curHeader = reader.ReadUInt32();
                    if (curHeader == Header)
                    {
                        Console.WriteLine("The file is already signed and checksumed!");
                        return;
                    }
                }

                //Get checksum
                Array.Resize(ref bytes, bytes.Length - ReservedBytes);
                byte[] checksum = DoChecksum(bytes);
                byte[] header = BitConverter.GetBytes(Header);

                //Write at the end of file
                FileStream fs = File.OpenWrite(filename);
                fs.Seek(bytes.Length, SeekOrigin.Begin);
                fs.Write(header, 0, header.Length);
                fs.Write(checksum, 0, checksum.Length);
                fs.Close();

                Console.WriteLine("File ["+filename+"] checksumed and saved!");
            }
            else
            {
                Console.WriteLine(filename + " does not exists!");
            }
        }
    }
}
