using System;
using System.IO;

namespace unSARC
{
    public class sarc
    {
        private HSARC _Header;
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

        public HSARC Header
        {
            get { return _Header; }
        }

        private void ReadHeader(string path)
        {
            FileStream readStream = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(readStream);

            _Header = new HSARC();

            _Header.Magic = br.ReadChars(4);
            _Header.Lenght = br.ReadUInt16();
            _Header.BOM = br.ReadUInt16();
            _Header.FileLenght = br.ReadUInt32();
            _Header.DATOffset = br.ReadUInt32();
            _Header.Version = br.ReadUInt32();
            _Header.Unknown = br.ReadUInt32();

            Fat = new HSFAT();

            Fat.Magic = br.ReadChars(4);
            Fat.Lenght = br.ReadUInt16();
            Fat.NodeCount = br.ReadUInt16(); // max 0x3fff ~ ‭16383‬
            Fat.HashKey = br.ReadUInt32();

            FatNode = new SFATN[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                FatNode[i].FileNameHash = br.ReadUInt32();
                FatNode[i].FileNameOffsetEntry = BitConverter.ToUInt16(br.ReadBytes(2), 0);
                br.ReadBytes(1);
                FatNode[i].FileNameFlag = br.ReadByte();
                FatNode[i].StartFileNode = br.ReadUInt32();
                FatNode[i].EndFileNode = br.ReadUInt32();
            }

            FNT = new SFNT();
            FNT.Magic = br.ReadChars(4);
            FNT.Lenght = br.ReadUInt16();
            FNT.Unknown = br.ReadUInt16();

            FNTNode = new string[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                int asciilenghtcount = 0;
                bool sopread = false;

                //256 character
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

                UInt32 hash = GetHash(FNTNodeA[i].FileName, Fat.HashKey);

                for (int j = 0; j < FatNode.Length; j++)
                {
                    if (FatNode[j].FileNameHash == hash && FatNode[i].FileNameFlag == 1)
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
                br.BaseStream.Position = _Header.DATOffset + FatNode[(int) FNTNodeA[i].FatNodeNumber].StartFileNode;
                uint lenghtfile = FatNode[(int)FNTNodeA[i].FatNodeNumber].EndFileNode - FatNode[(int)FNTNodeA[i].FatNodeNumber].StartFileNode;

                string tempath = "";
                string tempath2 = "";

                if (FatNode[i].FileNameFlag == 1)
                {
                    if (FNTNodeA[i].FileName == null)
                    {
                        break;
                    }

                    tempath = ExcPath + @"\" + FNTNodeA[i].FileName.Replace("/", "\\");
                    tempath2 = Path.GetFileName(tempath);

                    //Console.WriteLine(tempath);
                    if (!Directory.Exists(tempath.Replace(tempath2, "")))
                    {
                        Directory.CreateDirectory(tempath.Replace(tempath2, ""));
                    }

                    File.WriteAllBytes(tempath, br.ReadBytes((int)lenghtfile));
                }
                else
                {
                    br.BaseStream.Position = _Header.DATOffset + FatNode[i].StartFileNode;
                    lenghtfile = FatNode[i].EndFileNode - FatNode[i].StartFileNode;

                    tempath = ExcPath + @"\" + i+ ".bin";
                    tempath2 = Path.GetFileName(tempath);

                    //Console.WriteLine(tempath);
                    if (!Directory.Exists(tempath.Replace(tempath2, "")))
                    {
                        Directory.CreateDirectory(tempath.Replace(tempath2, ""));
                    }

                    File.WriteAllBytes(tempath, br.ReadBytes((int)lenghtfile));
                }
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
        public char[] Magic;
        public UInt16 Lenght;
        public UInt16 BOM;          // 0xFFFE
        public UInt32 FileLenght;
        public UInt32 DATOffset; // absolute
        public UInt32 Version;
        public UInt32 Unknown; //0x00000100
    }

    public struct HSFAT
    {
        // always 12 byte
        public char[] Magic;
        public UInt16 Lenght;
        public UInt16 NodeCount; 
        public UInt32 HashKey;
    }

    public struct SFATN
    {
        public UInt32 FileNameHash;
        public UInt32 FileNameOffsetEntry; // relative offset after FAT Header div by 4
        public byte FileNameFlag; // always 0x1
        public UInt32 StartFileNode;
        public UInt32 EndFileNode; // relative
    }

    public struct SFNT
    {
        public char[] Magic;
        public UInt16 Lenght;
        public UInt16 Unknown;
    }

    public struct SFNTN
    {
        public int FatNodeNumber;
        public string FileName;
    }
}
