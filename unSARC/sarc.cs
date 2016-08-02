using System;
using System.IO;

namespace unSARC
{
    public class sarc
    {
        private HSARC Header;
        private HSFAT Fat;
        private SFATN[] FatNode;
        private SFNT FNT;
        private string[] FNTNode;
        private SFNTN[] FNTNodeA;
        private string Paths;

        public sarc(string path)
        {
            Paths = path;
            ReadHeader(path);
        }

        private void ReadHeader(string path)
        {
            FileStream readStream = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(readStream);

            Header = new HSARC();

            Header.Magic = br.ReadUInt32();
            Header.Lenght = br.ReadUInt16();
            Header.BOM = br.ReadUInt16();
            Header.FileLenght = br.ReadUInt32();
            Header.DATOffset = br.ReadUInt32();
            Header.Unknown = br.ReadUInt32();

            Fat = new HSFAT();

            Fat.Magic = br.ReadUInt32();
            Fat.Lenght = br.ReadUInt16();
            Fat.NodeCount = br.ReadUInt16();
            Fat.FileNameHashMultiplier = br.ReadUInt32();

            FatNode = new SFATN[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                FatNode[i].FileNameHash = br.ReadUInt32();
                FatNode[i].FileNameTableEntry = BitConverter.ToUInt16(br.ReadBytes(2), 0);
                br.ReadBytes(1);
                FatNode[i].Unknown = br.ReadByte();
                FatNode[i].StartFileNode = br.ReadUInt32();
                FatNode[i].EndFileNode = br.ReadUInt32();
            }

            FNT = new SFNT();
            FNT.Magic = br.ReadUInt32();
            FNT.Lenght = br.ReadUInt16();
            br.ReadUInt16();

            FNTNode = new string[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                int asciilenghtcount = 0;
                bool sopread = false;

                do
                {
                    byte tempbyte = br.ReadByte();

                    if (tempbyte != 0)
                    {
                        FNTNode[i] += (char) tempbyte;
                    }
                    else
                    {
                        sopread = true;
                    }

                    asciilenghtcount++;
                } while (sopread == false);
                //Console.WriteLine(i + " " + asciilenghtcount + " " + recountlenght(asciilenghtcount));
                br.ReadBytes(recountlenght(asciilenghtcount));
            }

            FNTNodeA = new SFNTN[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                FNTNodeA[i].FileName = FNTNode[i];

                if (FNTNodeA[i].FileName == null)
                {
                    break;
                }

                UInt32 hash = GetHash(FNTNodeA[i].FileName, Fat.FileNameHashMultiplier);

                for (int j = 0; j < FatNode.Length; j++)
                {
                    if (FatNode[j].FileNameHash == hash)
                    {
                        //Console.WriteLine(FatNode[j].FileNameHash);
                        //Console.WriteLine(hash);
                        //Console.WriteLine(j);
                        //Console.WriteLine(FNTNodeA[i].FileName);
                        FNTNodeA[i].FatNodeNumber = j;
                        //Console.WriteLine("");
                        break;
                    }
                }
            }

            br.Close();
            readStream.Close();
        }


        public void Extract()
        {
            string ExcPath = Path.GetFileName(Paths).Replace(Path.GetExtension(Paths), "") + "_unpack";
            //Console.WriteLine(ExcPath);
            ExcPath = Path.GetFullPath(Paths).Replace(Path.GetFileName(Paths), "") + ExcPath;
            //Console.WriteLine(ExcPath);
            FileStream readStream = new FileStream(Paths, FileMode.Open);
            BinaryReader br = new BinaryReader(readStream);

            for (int i = 0; i < FatNode.Length; i++)
            {
                //Console.WriteLine((i+1)+" of "+ FatNode.Length);
                br.BaseStream.Position = Header.DATOffset + FatNode[(int) FNTNodeA[i].FatNodeNumber].StartFileNode;
                uint lenghtfile = FatNode[(int)FNTNodeA[i].FatNodeNumber].EndFileNode - FatNode[(int)FNTNodeA[i].FatNodeNumber].StartFileNode;

                if (FNTNodeA[i].FileName == null)
                {
                    break;
                }

                string tempath = ExcPath + @"\" + FNTNodeA[i].FileName.Replace("/", "\\");
                string tempath2 = Path.GetFileName(tempath);
                //Console.WriteLine(tempath);
                if (!Directory.Exists(tempath.Replace(tempath2,"")))
                {
                    Directory.CreateDirectory(tempath.Replace(tempath2, ""));
                }

                File.WriteAllBytes(tempath,br.ReadBytes((int)lenghtfile));
            }

            br.Close();
            readStream.Close();
        }

        static uint GetHash(string name, uint multiplier)
        {
            if (name.Length == 0)
            {
                return 0;
            }
            uint result = 0;
            for (int i = 0; i < name.Length; i++)
            {
                result = name[i] + result * multiplier;
            }
            return result;
        }

        private int recountlenght(int lenght)
        {
            var inp0 = lenght % 4;

            if (inp0 == 0)
            {
                inp0 = 4;
            }

            return 4 - inp0;
        }
    }

    public struct HSARC
    {
        // always 20 byte
        public UInt32 Magic;
        public UInt16 Lenght;
        public UInt16 BOM;          // 0xFFFE
        public UInt32 FileLenght;
        public UInt32 DATOffset; // absolute
        public UInt32 Unknown; //0x00000100
    }

    public struct HSFAT
    {
        // always 12 byte
        public UInt32 Magic;
        public UInt16 Lenght;
        public UInt16 NodeCount; 
        public UInt32 FileNameHashMultiplier;
    }

    public struct SFATN
    {
        public UInt32 FileNameHash;
        public UInt32 FileNameTableEntry; // relative offset after FAT Header div by 4
        public byte Unknown; // always 0x1
        public UInt32 StartFileNode;
        public UInt32 EndFileNode; // relative
    }

    public struct SFNT
    {
        public UInt32 Magic;
        public UInt16 Lenght;
    }

    public struct SFNTN
    {
        public int FatNodeNumber;
        public string FileName;
    }
}
