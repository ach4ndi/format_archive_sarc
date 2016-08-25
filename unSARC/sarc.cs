using System;
using System.IO;
using System.Windows.Forms;

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
        private bool isSARC;

        public sarc(string path)
        {
            Paths = path;
            ReadHeader(path);
        }

        public sarc(string path, TextBox box)
        {
            Paths = path;
            ReadHeader(path,box);
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

            if (!(new string(_Header.Magic)).Equals("SARC"))
            {
                isSARC = false;

                br.Close();
                readStream.Close();
                return;
            }
            else
            {
                isSARC = true;
            }

            _Header.Lenght = br.ReadUInt16();
            _Header.BOM = br.ReadUInt16();
            _Header.FileLenght = br.ReadUInt32();
            _Header.DATOffset = br.ReadUInt32();
            _Header.Version = br.ReadUInt16();
            _Header.Unknown = br.ReadUInt16();

            Fat = new HSFAT();

            Fat.Magic = br.ReadChars(4);
            Fat.Lenght = br.ReadUInt16();
            Fat.NodeCount = br.ReadUInt16(); // max 0x3fff ~ ‭16383‬
            Fat.HashKey = br.ReadUInt32();

            FatNode = new SFATN[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                FatNode[i].FileNameHash = br.ReadUInt32();
                FatNode[i].FileNameOffsetEntry = BitConverter.ToUInt32(new byte[] {br.ReadByte(), br.ReadByte(), br.ReadByte(), 0x0}, 0);
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
                        FNTNodeA[i].FatNodeNumber = j;
                        break;
                    }
                }
            }

            br.Close();
            readStream.Close();
        }

        private void ReadHeader(string path, TextBox box)
        {
            FileStream readStream = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(readStream);

            _Header = new HSARC();

            _Header.Magic = br.ReadChars(4);

            if (!(new string(_Header.Magic)).Equals("SARC"))
            {
                isSARC = false;

                box.BeginInvoke(new Action(() =>
                {
                    box.Text += path;
                    box.Text += Environment.NewLine;
                    box.Text += "	";

                    box.Text += "Not SARC File";
                    box.Text += Environment.NewLine;
                }));

                return;
            }
            else
            {
                isSARC = true;
            }

            _Header.Lenght = br.ReadUInt16();
            _Header.BOM = br.ReadUInt16();
            _Header.FileLenght = br.ReadUInt32();
            _Header.DATOffset = br.ReadUInt32();
            _Header.Version = br.ReadUInt16();
            _Header.Unknown = br.ReadUInt16();

            string paddo = "        ";

            box.BeginInvoke(new Action(() =>
            {
                box.Text += path;
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "// Header";
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "Lenght : " + _Header.Lenght;
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "BOM : " + _Header.BOM.ToString("x");
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "FileLenght : " + _Header.FileLenght +" byte";
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "DAT Offset : " + _Header.DATOffset;
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "Version : " + _Header.Version;
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "? : " + _Header.Unknown;
                box.Text += Environment.NewLine;
            }));

            Fat = new HSFAT();

            Fat.Magic = br.ReadChars(4);
            Fat.Lenght = br.ReadUInt16();
            Fat.NodeCount = br.ReadUInt16(); // max 0x3fff ~ ‭16383‬
            Fat.HashKey = br.ReadUInt32();

            box.BeginInvoke(new Action(() =>
            {
                box.Text += paddo;

                box.Text += "// FAT Header";
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "Lenght : " + Fat.Lenght;
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "Node Count : " + Fat.NodeCount;
                box.Text += Environment.NewLine;
                box.Text += paddo;

                box.Text += "HashKey : " + Fat.HashKey;
                box.Text += Environment.NewLine;
            }));


            FatNode = new SFATN[Fat.NodeCount];

            for (int i = 0; i < FatNode.Length; i++)
            {
                FatNode[i].FileNameHash = br.ReadUInt32();
                FatNode[i].FileNameOffsetEntry = BitConverter.ToUInt32(new byte[] { br.ReadByte(), br.ReadByte(), br.ReadByte(), 0x0 }, 0);
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
                        FNTNode[i] += (char)tempbyte;
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
                        FNTNodeA[i].FatNodeNumber = j;
                        break;
                    }
                }
            }

            br.Close();
            readStream.Close();
        }

        public void Extract()
        {
            if (!isSARC) return;

            string excPath = Path.GetFileName(Paths).Replace(Path.GetExtension(Paths), "") + "_unpack";
            //Console.WriteLine(ExcPath);
            excPath = Path.GetFullPath(Paths).Replace(Path.GetFileName(Paths), "") + excPath;
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

                    tempath = excPath + @"\" + FNTNodeA[i].FileName.Replace("/", "\\");
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

                    tempath = excPath + @"\" + i+ ".bin";
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
