using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GovanifY.Utility;
using System.IO;

namespace XBB_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("XBB_Tool\nProgrammed by GovanifY for ChrisX930\n\n1) Extract 2) Create\n");
            string choice = Console.ReadLine();
            if (choice == "1")
            {
                Console.WriteLine("\n\nPlease enter the name of the file to extract: ");
                string arg = Console.ReadLine();
            if (File.Exists(arg))
            {

                BinaryStream input = new BinaryStream(File.Open(arg, FileMode.Open));
                UInt32 magic = input.ReadUInt32();
                if (magic != 0x01424258) { Console.WriteLine("INCORRECT MAGIC!\nExiting..."); return; }
                UInt32 count = input.ReadUInt32();
                input.Seek(0x20, SeekOrigin.Begin);//Padding...?
                string dirname = "@" + arg + "/";
                #region Dir creation
                try
                {
                    Directory.CreateDirectory(dirname);
                }
                catch (IOException e)
                {
                    Console.Write("Failed creating directory: {0}", e.Message);
                }
                #endregion
                for (int i = 0; i < count; i++)
                {
                    UInt32 offset = input.ReadUInt32();
                    UInt32 size = input.ReadUInt32();
                    UInt32 nameoffset = input.ReadUInt32();
                    UInt32 ID = input.ReadUInt32();

                    long tmp = input.Tell();
                    input.Seek(nameoffset, SeekOrigin.Begin);
                    byte[] namet = new byte[0];
                    //Reads name until 0
                    while(true)
                    {
                        byte test = input.ReadByte();
                        if (test == 0){goto next;}
                        byte[] tmpnamet = new byte[namet.Length + 1];
                        namet.CopyTo(tmpnamet, 1);
                        tmpnamet[0] = test;
                        namet = tmpnamet;
                    }
                next:
                    Array.Reverse(namet, 0, namet.Length);
                  string name = dirname + Encoding.ASCII.GetString(namet);
                Console.WriteLine("Extracting...: {0}", name);
                    input.Seek(offset, SeekOrigin.Begin);
                    
            byte[] PAPAtmp = input.ReadBytes((int)size);
            var PAPAfs = new FileStream(name, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            PAPAfs.Write(PAPAtmp, 0, PAPAtmp.Length);
            input.Seek(tmp, SeekOrigin.Begin);


                }
        }
            else
            {
                Console.WriteLine("Cannot open file!");
            }
            }
            else
            {
                if (choice == "2")
                {
                    Console.WriteLine("\n\nPlease enter the name of the file to create: ");
                    string arg = Console.ReadLine();
                    BinaryWriter output = new BinaryWriter(File.Open(Path.GetFileNameWithoutExtension(arg) + "Modded" + Path.GetExtension(arg), FileMode.Create));
                   string dirname = "@" + arg + "/";
                        UInt32 IDCustom = 0x90000000;
                        output.Write((uint)0x01424258);
                        string[] files = Directory.GetFiles(dirname);
                        output.Write((uint)files.Length);
                        output.Write((uint)0);//Padding
                        output.Write((uint)0);//Padding
                        output.Write((uint)0);//Padding
                        output.Write((uint)0);//Padding
                        output.Write((uint)0);//Padding
                        output.Write((uint)0);//Padding
                        BinaryStream input = new BinaryStream(File.Open(arg, FileMode.Open));
                        UInt32 magic = input.ReadUInt32();
                        if (magic != 0x01424258) { Console.WriteLine("INCORRECT MAGIC!\nExiting..."); return; }
                        UInt32 count = input.ReadUInt32();
                        input.Seek(0x20, SeekOrigin.Begin);//Padding...?
                        int disposername = (files.Length * 4 * 4) + 0x20 + 8 + (files.Length * 4 * 2);
                        int disposer2LBA = (files.Length * 4 * 4) + 0x20 + 8;
                        /*long tmp = output.BaseStream.Position;
                        output.Seek(disposername - 8, SeekOrigin.Begin);
                        output.Write((UInt32)0);//Garbage, dunno what those 2 bytes are
                        output.Write((UInt32)0);
                        output.Seek((int)tmp, SeekOrigin.Begin);*/
                        int disposer = (files.Length * 4 * 4) + 0x20 + 8 + 2 + (files.Length * 4 * 2);
                        foreach (string name in files)
                        {
                            disposer += Path.GetFileName(name).Length + 1;
                        }
                        foreach (string name in files)
                        {
                            UInt32 ID;
                            UInt32 unk1;
                            UInt32 unk2;
                            try
                            {
                            input.ReadUInt32();
                            input.ReadUInt32();
                            input.ReadUInt32();
                            ID = input.ReadUInt32();
                            long tmp = input.Tell();
                            input.Seek(disposer2LBA, SeekOrigin.Begin);
                            unk1 = input.ReadUInt32();
                            unk2 = input.ReadUInt32();
                            input.Seek(tmp, SeekOrigin.Begin);
                            }
                            catch
                            {
                               unk1 = 0;
                               unk2 = 0;
                               ID = IDCustom;//custom ID
                            }
                            byte[] file = File.ReadAllBytes(name);
                            output.Write((UInt32)disposer);
                            output.Write((UInt32)file.Length);//Size
                            output.Write((UInt32)disposername);
                            output.Write((UInt32)ID);
                            Console.WriteLine("Adding: {0}, using ID {1}",name, ID);
                            IDCustom++;
                            long tmp2 = output.BaseStream.Position;
                            output.Seek(disposer2LBA, SeekOrigin.Begin);
                            output.Write(unk1);
                            output.Write(unk2);//Unknowns to figure out!!!
                            disposer2LBA += 8;
                            output.Seek(disposername, SeekOrigin.Begin);
                            disposername += Path.GetFileName(name).Length + 1;
                            byte[] tmp3 = System.Text.Encoding.ASCII.GetBytes(Path.GetFileName(name));
                            byte[] tmp4 = new byte[] { 0x00 };
                            output.Write(tmp3);
                            output.Write(tmp4);
                            output.Seek(disposer, SeekOrigin.Begin);
                            output.Write(file);
                            disposer += file.Length;
                            output.Seek((int)tmp2, SeekOrigin.Begin);
                        }
                }
                else
                {
                    Console.WriteLine("Please enter a correct option!");
                }
            }

    }
    }
}
