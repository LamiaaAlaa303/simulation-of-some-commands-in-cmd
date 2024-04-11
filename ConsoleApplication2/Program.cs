using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace ConsoleApp6Edited
{
    class VirsualDisk
    {
        public static FileStream file;
        /* public static void CREATE_Disk(string path)
         {
             file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
             file.Close();
         }


         public static int getFreeSpace()
         {
             return (1024 * 1024) - (int)file.Length;
         }*/


        public static void initalize(string path)
        {
            if (!File.Exists(path))
            {
                file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);


                for (int i = 0; i < 1024; i++)
                {
                    file.WriteByte(0);
                    //Console.Write("0");
                }
                for (int i = 1024; i < ((4 * 1024) + 1024); i++)
                {
                    file.WriteByte((byte)'*');
                    // Console.Write("*");
                }
                for (int i = ((4 * 1024) + 1024); i < 1048576; i++)
                {
                    file.WriteByte((byte)'#');
                    //Console.Write("#");
                }




                //Fat_table f = new Fat_table();
                Fat_table.initialize();
                Directory root = new Directory("D:\\>", 0x10, 5, 0, null);
                file.Close();
                root.write_directory();
                // Fat_table.Set_Next(5, -1);
                Program.currentDirectory = root;


                Fat_table.write_Fat_Table();
            }
            else
            {
                Fat_table.Get_Fat_Table();
                Directory root = new Directory("M:\\>", 0x10, 5, 0, null);
                root.Read_Directory();
                Program.currentDirectory = root;
            }
        }
        /* static public void initialize()
         {
             file = new FileStream("D:\\taskOs.txt", FileMode.Create, FileAccess.Write);
             using var sr = new StreamWriter(file);
             for (int i = 0; i < 1024; i++)
             {
                 sr.Write("0");
                 Console.Write("0");
             }
             for (int i = 1024; i < ((4 * 1024) + 1024); i++)
             {
                 sr.Write("*");
                 Console.Write("*");
             }
             for (int i = ((4 * 1024) + 1024); i < 1048576; i++)
             {
                 sr.Write("#");
                 Console.Write("#");
             } 
             //file.Close();
         }*/
        public static void Write_Block(byte[] data, int index, int offset = 0, int count = 1024)
        {
            file = new FileStream("D:\\taskOs.txt", FileMode.Open, FileAccess.Write);
            file.Seek(index * 1024, SeekOrigin.Begin);
            file.Write(data, offset, count);
            file.Flush();
            file.Close();
        }
        public static byte[] Get_Block(int index)
        {
            string path = "D:\\taskOs.txt";
            file = new FileStream("D:\\taskOs.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            file.Seek(index * 1024, SeekOrigin.Begin);
            byte[] data = new byte[1024];
            file.Read(data, 0, 1024);
            file.Close();
            return data;
        }
    }
    public static class Fat_table
    {
        static int[] FatTable;
        static byte[] b = new byte[4096];
        static int index;


        public static void initialize()
        {
            FatTable = new int[1024];
            for (int i = 0; i < 1024; i++)
            {
                if (i < 5)
                {
                    FatTable[i] = -1;
                }
            }
        }
        public static void write_Fat_Table()
        {
            VirsualDisk.file = new FileStream("D:\\taskOs.txt", FileMode.OpenOrCreate, FileAccess.Write);
            VirsualDisk.file.Seek(1024, SeekOrigin.Begin);
            Buffer.BlockCopy(FatTable, 0, b, 0, b.Length);
            VirsualDisk.file.Write(b, 0, b.Length);


            VirsualDisk.file.Close();
        }
        public static void Get_Fat_Table()
        {
            Fat_table.initialize();


            VirsualDisk.file = new FileStream("D:\\taskOs.txt", FileMode.OpenOrCreate, FileAccess.Read);
            VirsualDisk.file.Seek(1024, SeekOrigin.Begin);
            VirsualDisk.file.Read(b, 0, b.Length);
            Buffer.BlockCopy(b, 0, FatTable, 0, b.Length);
            VirsualDisk.file.Close();


        }


        //Get_Fat_Table();
        //for (int i = 0; i < FatTable.Length; i++)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("[" + i +1+ "]" + " [" + FatTable[i] + "]");
        //}


        public static int Get_Available_Block()
        {
            int freeIndex = -1;
            for (int i = 0; i < 1024; i++)
            {
                if (FatTable[i] == 0)
                {
                    freeIndex = i;
                    break;
                }
            }
            return freeIndex;
        }
        public static int Get_Next(int index)
        {
            return (FatTable[index]);
        }
        public static void Set_Next(int index, int Value)
        {
            FatTable[index] = Value;
        }
        public static int Get_Available_Blocks()
        {
            int counter = 0;
            for (int i = 0; i < FatTable.Length; i++)
            {
                if (FatTable[i] == 0)
                {
                    counter++;
                }
            }
            return counter;
        }
        public static int Get_Free_Space()
        {
            return Get_Available_Blocks() * 1024;
        }
    }
    class Directory_Entry
    {
        public char[] file_Name = new char[11];
        public byte file_attribute; //0x0 (file) or 0x10 (folder)
        public byte[] file_empty = new byte[12]; //zeros
        public int file_size;
        public int first_cluster;
        public Directory_Entry()
        { }
        public Directory_Entry(string fileName, byte fileAttribute, int firstCluster, int FileSize)
        {
            if (fileName.Length < 11)
            {
                for (int i = fileName.Length; i < 11; i++)
                    fileName += " ";


            }
            this.file_Name = fileName.ToCharArray();
            file_attribute = fileAttribute;
            /*if (file_attribute == 0x0)
            {
                string[] filename = new string(fileName).Split('.');
                assignFileName(filename[0].ToCharArray(), filename[1].ToCharArray());
            }
            else
            {
                assignDIRName(fileName);
            } */
            first_cluster = firstCluster;
            file_size = FileSize;
        }
        /*public void assignFileName(char[] name, char[] extension)
        {
            if (name.Length <= 7 && extension.Length == 3)
            {
                int j = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    j++;
                    this.file_Name[i] = name[i];
                }
                j++;
                this.file_Name[j] = '.';
                for (int i = 0; i < extension.Length; i++)
                {
                    j++;
                    this.file_Name[j] = extension[i];
                }
                for (int i = ++j; i < file_Name.Length; i++)
                {
                    this.file_Name[i] = ' ';
                }
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    this.file_Name[i] = name[i];
                }
                this.file_Name[7] = '.';
                for (int i = 0, j = 8; i < extension.Length; j++, i++)
                {
                    this.file_Name[j] = extension[i];
                }
            }
        }


        public void assignDIRName(char[] name)
        {
            if (name.Length <= 11)
            {
                int j = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    j++;
                    this.file_Name[i] = name[i];
                }
                for (int i = ++j; i < file_Name.Length; i++)
                {
                    this.file_Name[i] = ' ';
                }
            }
            else
            {
                int j = 0;
                for (int i = 0; i < 11; i++)
                {
                    j++;
                    this.file_Name[i] = name[i];
                }
            }
        } */
        public byte[] GetBytes()
        {
            byte[] b = new byte[32];
            byte[] name = new byte[11];
            name = Encoding.ASCII.GetBytes(file_Name);
            for (int i = 0; i < 11; i++)
            {
                if (i < name.Length)
                    b[i] = name[i];
                else
                    b[i] = (byte)' ';


            }
            b[11] = file_attribute;


            for (int i = 0; i < 12; i++)
            {
                b[i + 12] = 0;
            }


            byte[] fc = new byte[4];
            fc = BitConverter.GetBytes(first_cluster);
            for (int i = 0; i < 4; i++)
                b[i + 24] = fc[i];


            byte[] fs = new byte[4];
            fs = BitConverter.GetBytes(file_size);
            for (int i = 0; i < 4; i++)
                b[i + 28] = fs[i];


            return b;


        }


        public Directory_Entry GetDirectoryEntry(byte[] b)
        {
            Directory_Entry d = new Directory_Entry();
            for (int i = 0; i < 11; i++)
            {
                d.file_Name[i] = (char)b[i];
            }


            d.file_attribute = b[11];


            for (int i = 0; i < 12; i++)
            {
                d.file_empty[i] = 0;
            }
            byte[] fc = new byte[4];
            for (int i = 24; i < 28; i++)
            {
                fc[i - 24] = b[i];
            }
            d.first_cluster = BitConverter.ToInt32(fc, 0);
            byte[] fs = new byte[4];
            for (int i = 28; i < 32; i++)
            {
                fs[i - 28] = b[i];
            }
            d.file_size = BitConverter.ToInt32(fs, 0);


            //  Directory_Entry d1 = new Directory_Entry(new string (file_Name), file_attribute, first_cluster, file_size);
            return d;
        }




    }
    class Directory : Directory_Entry
    {
        public List<Directory_Entry> directory_Table;


        public Directory parent = null;
        public Directory(string fileName, byte fileattribute, int firstcluster, int file_Size, Directory parent1)
            : base(fileName, fileattribute, firstcluster, file_Size)
        {
            directory_Table = new List<Directory_Entry>();
            if (parent != null)
            {
                parent1 = parent;
            }
        }
        public Directory_Entry GetDirectory_Entry()
        {
            Directory_Entry d = new Directory_Entry(new string(file_Name), file_attribute, first_cluster, file_size);
            return d;
        }
        public void write_directory()
        {
            int fattIndex, lastInbex = -1;


            byte[] DirectoryEntryByte = new byte[32];
            byte[] DiretcoryTableByte = new byte[32 * directory_Table.Count];
            for (int i = 0; i < directory_Table.Count; i++)
            {
                DirectoryEntryByte = directory_Table[i].GetBytes();
                for (int j = i * 32, c = 0; c < 32; c++, j++)
                {
                    DiretcoryTableByte[j] = DirectoryEntryByte[c];
                }
            }
            double numOfReqBlock = Math.Ceiling(DiretcoryTableByte.Length / 1024.0);
            int fullSize = DiretcoryTableByte.Length / 1024;
            int reminder = (DiretcoryTableByte.Length % 1024);
            if (numOfReqBlock <= Fat_table.Get_Available_Blocks())
            {
                if (this.first_cluster != 0) // fe el root bs (5)
                {
                    fattIndex = this.first_cluster;
                }
                else
                {
                    fattIndex = Fat_table.Get_Available_Block();
                    this.first_cluster = fattIndex;
                }
                List<byte[]> Blocks = new List<byte[]>();
                byte[] block = new byte[1024];
                for (int j = 0; j < fullSize; j++)
                {
                    for (int i = j * 1024, c = 0; c < 1024; c++, i++)
                    {
                        block[c] = DiretcoryTableByte[j];




                    }
                    Blocks.Add(block);


                }
                if (reminder > 0)
                {
                    block = new byte[1024];
                    int start = fullSize * 1024;
                    for (int i = 0; i < (start + reminder); i++)
                    {
                        block[i % 1024] = DiretcoryTableByte[i];


                    }
                    Blocks.Add(block);




                }
                for (int i = 0; i < Blocks.Count; i++)
                {
                    VirsualDisk.Write_Block(Blocks[i], fattIndex);
                    Fat_table.Set_Next(fattIndex, -1);
                    if (lastInbex != -1)
                    {
                        // lastInbex = fattIndex;
                        Fat_table.Set_Next(lastInbex, fattIndex);
                    }
                    else
                    {
                        lastInbex = fattIndex;
                        fattIndex = Fat_table.Get_Available_Block();


                    }
                }
            }
            Fat_table.write_Fat_Table();
        }
        public void Read_Directory()
        {
            directory_Table = new List<Directory_Entry>();
            if (first_cluster != 0 && Fat_table.Get_Next(first_cluster) != 0)
            {
                int FatIndex = this.first_cluster;
                int next = Fat_table.Get_Next(FatIndex);
                List<byte> ls = new List<byte>();
                //List<Directory_Entry> dt = new List<Directory_Entry>();


                do
                {
                    ls.AddRange(VirsualDisk.Get_Block(FatIndex));
                    FatIndex = next;
                    if (FatIndex != -1)
                    {
                        next = Fat_table.Get_Next(FatIndex);
                    }
                } while (next != -1);
                byte[] b = new byte[32];


                for (int i = 0; i < ls.Count; i++)
                {
                    b[i % 32] = ls[i];
                    if ((i + 1) % 32 == 0)
                    {
                        Directory_Entry d = GetDirectoryEntry(b);
                        if (d.file_Name[0] != '\0')
                            directory_Table.Add(d);


                    }




                }
            }
        }
        public int searchdirectory(string name)
        {
            Read_Directory();
            if (name.Length < 11)
            {
                for (int i = name.Length; i < 11; i++)
                    name += " ";


            }
            for (int i = 0; i < directory_Table.Count; i++)
            {
                string n = new string(this.directory_Table[i].file_Name);
                if (n == name)
                {
                    return i;
                }
            }
            return -1;
        }
        /* public int searchdirectory(string name)
         {


             char[] filename = name.ToCharArray();
             for (int i = 0; i < directory_Table.Count; i++)
             {
                 if (directory_Table[i].file_Name == filename)
                 {
                     return i;
                 }
             }
             return -1;
         }*/
        public void updateContent(Directory_Entry d)
        {
            Read_Directory();
            int index;
            index = searchdirectory(new string(d.file_Name));
            if (index != -1)
            {
                directory_Table.RemoveAt(index);
                directory_Table.Insert(index, d);
            }
            write_directory();
        }
        public void deletedirectory()
        {
            int next, index;


            if (this.first_cluster != 0)
            {
                index = this.first_cluster;
                next = -1;


                do
                {
                    Fat_table.Set_Next(index, 0);
                    next = index;
                    if (index != -1)
                    {
                        index = Fat_table.Get_Next(index);
                    }
                }
                while (index != -1);
                if (this.parent != null)
                {
                    parent.Read_Directory();
                    index = parent.searchdirectory(new string(file_Name));
                }
                if (index != -1)
                {
                    this.parent.directory_Table.RemoveAt(index);
                    this.parent.write_directory();
                }
                // Fat_table.write_Fat_Table();
            }
        }
    }
    class File_Entry : Directory_Entry
    {
        public string content;
        public Directory parent;


        public File_Entry(string filenName, byte fileattribute, int firstcluster, int size, string Content, Directory parent1)
            : base(filenName, fileattribute, firstcluster, size)
        {
            content = Content;
            if (parent != null)
            {
                parent = parent1;


            }
        }
        public void write_File()
        {
            int fattIndex, lastInbex = -1;


            byte[] b = Encoding.ASCII.GetBytes(content);
            double numOfReqBlock = Math.Ceiling(b.Length / 1024.0);
            int fullSize = b.Length / 1024;
            int reminder = (b.Length % 1024);
            if (numOfReqBlock <= Fat_table.Get_Available_Blocks())
            {
                if (this.first_cluster != 0) // fe el root bs (5)
                {
                    fattIndex = this.first_cluster;
                }
                else
                {
                    fattIndex = Fat_table.Get_Available_Block();
                    this.first_cluster = fattIndex;
                }
                List<byte[]> Blocks = new List<byte[]>();
                byte[] block = new byte[1024];
                for (int j = 0; j < fullSize; j++)
                {
                    for (int i = j * 1024, c = 0; c < 1024; c++, i++)
                    {
                        block[c] = b[j];




                    }
                    Blocks.Add(block);


                }
                if (reminder > 0)
                {
                    block = new byte[1024];
                    int start = fullSize * 1024;
                    for (int i = 0; i < (start + reminder); i++)
                    {
                        block[i % 1024] = b[i];


                    }
                    Blocks.Add(block);




                }
                for (int i = 0; i < Blocks.Count; i++)
                {
                    VirsualDisk.Write_Block(Blocks[i], fattIndex);
                    Fat_table.Set_Next(fattIndex, -1);
                    if (lastInbex != -1)
                    {
                        // lastInbex = fattIndex;
                        Fat_table.Set_Next(lastInbex, fattIndex);
                    }
                    else
                    {
                        lastInbex = fattIndex;
                        fattIndex = Fat_table.Get_Available_Block();


                    }
                }
            }
            Fat_table.write_Fat_Table();


        }
        List<byte> ls;
        public void Read_File()
        {
            if (this.first_cluster != 0)
            {
                content = string.Empty;
                int first = this.first_cluster;
                int next = Fat_table.Get_Next(first);
                List<byte> ls = new List<byte>();
                do
                {
                    ls.AddRange(VirsualDisk.Get_Block(first));
                    first = next;
                    if (first != -1)
                    {
                        next = Fat_table.Get_Next(first);
                    }
                } while (next != -1);


                string s = string.Empty;
                for (int i = 0; i < ls.Count; i++)
                {
                    if (Convert.ToChar(ls[i]) != '\0')
                        s += Convert.ToChar(ls[i]);
                }
                content = s;
            }
        }
        public void deleteFile()
        {
            int next, fs_cluxter;


            if (first_cluster != 0)
            {
                fs_cluxter = first_cluster;
                next = Fat_table.Get_Next(fs_cluxter);
                do
                {
                    Fat_table.Set_Next(fs_cluxter, 0);
                    fs_cluxter = next;
                    if (first_cluster != -1)
                    {
                        next = Fat_table.Get_Next(fs_cluxter);
                    }
                }
                while (fs_cluxter != -1);
                if (parent != null)
                {
                    // parent.Read_Directory();
                    int index = parent.searchdirectory(new string(file_Name));
                    if (index != -1)
                    {
                        parent.directory_Table.RemoveAt(index);
                        parent.write_directory();
                        Fat_table.write_Fat_Table();
                    }
                }
            }
        }
    }
    class cmd
    {
        public static void help()
        {
            Console.WriteLine("CD      Change the current default directory to. ");
            Console.WriteLine("CLS     Clear the screen. ");
            Console.WriteLine("DIR     List the contents of directory . ");
            Console.WriteLine("QUIT    Quit the shell. ");
            Console.WriteLine("COPY    Copies one or more files to another location. ");
            Console.WriteLine("DEL     Deletes one or more files. ");
            Console.WriteLine("HELP    Provides Help information for commands. ");
            Console.WriteLine("MD      Creates a directory. ");
            Console.WriteLine("RD      Removes a directory. ");
            Console.WriteLine("RENAME  Renames a file. ");
            Console.WriteLine("TYPE    Displays the contents of a text file. ");
            Console.WriteLine("IMPORT  Import text file(s) from your computer. ");
            Console.WriteLine("EXPORT  Export text file(s) to your computer. ");
        }
        public static void cls()
        {
            Console.Clear();
        }
        public static void quit()
        {
            System.Environment.Exit(1);
        }
        public static void rd(string name)
        {
            int index = Program.currentDirectory.searchdirectory(name);
            if (index != -1)
            {
                int firstCluster = Program.currentDirectory.directory_Table[index].first_cluster;
                Directory d1 = new Directory(name, 0x10, firstCluster, 0, Program.currentDirectory);
                d1.deletedirectory();
                Program.currentPath = new string(Program.currentDirectory.file_Name).Trim();
            }
            else
            {
                Console.WriteLine("The system cannot find the path specified.");
            }
        }
        public static void cd(string name)
        {
            int index = Program.currentDirectory.searchdirectory(name);


            if (index != -1)
            {
                int firstCluster = Program.currentDirectory.directory_Table[index].first_cluster;
                Directory d1 = new Directory(name, 0x10, firstCluster, 0, Program.currentDirectory);
                Program.currentPath = new string(Program.currentDirectory.file_Name).Trim()
                    + "\\" + new string(d1.file_Name).Trim();
                Program.currentDirectory.Read_Directory();
            }
            else
            {
                Console.WriteLine("The system cannot find the path specified.");
            }
        }
        public static void md(string name)
        {
            if (Program.currentDirectory.searchdirectory(name) == -1)
            {
                Directory_Entry newdirectory = new Directory_Entry(name, 0x10, 0, 0);
                Program.currentDirectory.directory_Table.Add(newdirectory);
                Program.currentDirectory.write_directory();
                if (Program.currentDirectory.parent != null)
                {
                    Program.currentDirectory.parent.updateContent(Program.currentDirectory.parent);
                    Program.currentDirectory.parent.write_directory();
                }
            }
            else
            {
                Console.WriteLine("A subdirectory or file " + name + " already exists.");
            }
        }
        public static void dir()
        {
            int counterDirectory = 0, counterfiles = 0, counter_size_of_files = 0;
            Console.WriteLine("Directory of " + Program.currentPath);
            for (int i = 0; i < Program.currentDirectory.directory_Table.Count; i++)
            {
                if (Program.currentDirectory.directory_Table[i].file_attribute == 0x0)
                {
                    Console.WriteLine(Program.currentDirectory.directory_Table[i].file_size +
                        "  " + new string(Program.currentDirectory.directory_Table[i].file_Name));
                    counterfiles++;
                    counter_size_of_files += Program.currentDirectory.directory_Table[i].file_size;
                }
                else
                {
                    Console.Write("<dir>" + "      ");
                    Console.WriteLine(Program.currentDirectory.directory_Table[i].file_Name);
                    counterDirectory++;
                }
            }
            Console.WriteLine(counterfiles + " File(s)       " + counter_size_of_files + " bytes");
            Console.WriteLine(counterDirectory + " Dir(s)   " + Fat_table.Get_Free_Space() + "  bytes Free");
        }
        public static void import(string file_path)
        {
            if (File.Exists(file_path))
            {
                string content = File.ReadAllText(file_path);
                int size = content.Length;
                int startname = file_path.LastIndexOf("\\");
                string name;
                name = file_path.Substring(startname + 1);
                int index = Program.currentDirectory.searchdirectory(name);
                if (index == -1)
                {
                    int fisrtCluster;
                    if (size > 0)
                    {
                        fisrtCluster = Fat_table.Get_Available_Block();
                    }
                    else
                    {
                        fisrtCluster = 0;
                    }
                    File_Entry f = new File_Entry(name, 0x0, fisrtCluster, size, content, Program.currentDirectory);
                    f.write_File();
                    Directory_Entry d = new Directory_Entry(name, 0x0, fisrtCluster, size);
                    Program.currentDirectory.directory_Table.Add(d);
                    Program.currentDirectory.write_directory();


                }
                else
                {
                    Console.WriteLine("file already Exist");
                }
            }
            else
            {
                Console.WriteLine("this file Not Exists ");
            }
        }
        public static void type(string name)
        {
            int index = Program.currentDirectory.searchdirectory(name);
            if (index != -1)
            {
                int first_cluster = Program.currentDirectory.directory_Table[index].first_cluster;
                int filesize = Program.currentDirectory.directory_Table[index].file_size;
                string content = null;
                File_Entry f = new File_Entry(name, 0x0, first_cluster, filesize, content, Program.currentDirectory);


                f.Read_File();
                Console.WriteLine(f.content);
            }
            else
            {
                Console.WriteLine("The system can't find the file ");
            }
        }
        public static void export(string source, string destination)
        {
            int index = Program.currentDirectory.searchdirectory(source);
            if (index != -1)
            {
                if (System.IO.Directory.Exists(destination))
                {
                    int first_cluster = Program.currentDirectory.directory_Table[index].first_cluster;
                    int filesize = Program.currentDirectory.directory_Table[index].file_size;
                    string temp = null;
                    File_Entry f = new File_Entry(source, 0x0, first_cluster, filesize, temp, Program.currentDirectory);
                    f.Read_File();


                    StreamWriter sw = new StreamWriter(destination + "\\" + source);
                    sw.Write(f.content);
                    sw.Flush();
                    sw.Close();
                }
                else
                {
                    Console.WriteLine("the system can't find this path in computer Disk");
                }
            }
            else
            {
                Console.WriteLine("This file doesn't exist");
            }
        }
        public static void rename(string oldname, string newname)
        {
            int index = Program.currentDirectory.searchdirectory(oldname);
            if (index != -1)
            {
                int n = Program.currentDirectory.searchdirectory(newname);
                if (n == -1)
                {
                    Directory_Entry d = Program.currentDirectory.directory_Table[index];
                    d.file_Name = newname.ToCharArray();
                    Program.currentDirectory.directory_Table.RemoveAt(index);
                    Program.currentDirectory.directory_Table.Insert(n + 1, d);
                    // Program.currentDirectory.write_directory();
                }
                else
                {
                    Console.WriteLine("A duplicate file name exists, or the file cannot be found.");
                }


            }
            else
            {
                Console.WriteLine(" The system cannot find the file specified.");
            }


        }
        public static void del(string name)
        {
            int index = Program.currentDirectory.searchdirectory(name);
            if (index != -1)
            {
                int f = Program.currentDirectory.directory_Table[index].file_attribute;
                if (f == 0x0)
                {
                    int first_cluster = Program.currentDirectory.directory_Table[index].first_cluster;
                    int file_size = Program.currentDirectory.directory_Table[index].file_size;
                    File_Entry d = new File_Entry(name, 0x0, first_cluster, 0, null, Program.currentDirectory);
                    d.deleteFile();
                }
                else
                {
                    Console.WriteLine(" The system can't find the file specified. ");
                }
            }
            else
            {
                Console.WriteLine(" The system can't find the file specified. ");
            }
        }
        public static void copy(string num1, string num2)
        {
            int index1 = Program.currentDirectory.searchdirectory(num1);
            if (index1 != -1)
            {
                int start_index = num2.LastIndexOf("\\");
                string name = num2.Substring(start_index + 1);


                int index_destenation = Program.currentDirectory.searchdirectory(name);
                if (index_destenation == -1)
                {
                    if (num2 != Program.currentDirectory.file_Name.ToString())
                    {
                        int f_cluster = Program.currentDirectory.directory_Table[index1].first_cluster;
                        int f_size = Program.currentDirectory.directory_Table[index1].file_size;
                        Directory_Entry entry = new Directory_Entry(num1, 0x0, f_cluster, f_size);
                        Directory dir = new Directory(num2, 0x10, 0, 0, Program.currentDirectory.parent);
                        dir.directory_Table.Add(entry);
                    }
                    else Console.WriteLine("not fff");
                }
            }
        }
    }
    class Program
    {
        /* static void Main(string[] args)
         {
             VirsualDisk.initalize("taskOs.txt");
             //VirsualDisk.initialize();
             Fat_table.initialize();
             Fat_table.write_Fat_Table();
             Fat_table.Get_Fat_Table();
             // Fat_table.Print_Fat_Table();
             Fat_table.Get_Available_Block();
             //Directory_Entry de= new Directory_Entry();
             
                Console.ReadKey();
            }
         }       */
        public static Directory currentDirectory;
        public static string currentPath;
        static void Main(string[] args)
        {
            VirsualDisk.initalize("D:\\taskOs.txt");
            // Fat_table.initialize();
            // Fat_table.Print_Fat_Table();


            //string currentdir = Environment.CurrentDirectory;            


            currentPath = new string(currentDirectory.file_Name);
            while (true)
            {
                Console.Write(currentPath.Trim());
                string inputuser = Console.ReadLine();


                /* if (inputuser.Length == 0)
                 {


                 }
                 else */
                if (!inputuser.Contains(" "))
                {
                    if (inputuser.ToLower() == "help")
                    {
                        cmd.help();
                    }
                    else if (inputuser.ToLower() == "quit")
                    {
                        cmd.quit();
                    }
                    else if (inputuser.ToLower() == "cls")
                    {
                        cmd.cls();
                    }
                    else if (inputuser.ToLower() == "md")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }
                    else if (inputuser.ToLower() == "rd")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }
                    else if (inputuser.ToLower() == "dir")
                    {
                        cmd.dir();
                    }
                    else if (inputuser.ToLower() == "import")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }
                    else if (inputuser.ToLower() == "type")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }
                    else if (inputuser.ToLower() == "export")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }
                    else if (inputuser.ToLower() == "del")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }
                    else if (inputuser.ToLower() == "copy")
                    {
                        Console.WriteLine("The syntax of the command is incorrect.");
                    }


                }
                else if (inputuser.Contains(" "))
                {
                    string[] arrInput = inputuser.Split();
                    if (arrInput[0] == "md")
                    {
                        cmd.md(arrInput[1]);
                    }
                    else if (arrInput[0] == "rd")
                    {
                        cmd.rd(arrInput[1]);
                    }
                    else if (arrInput[0] == "cd")
                    {
                        cmd.cd(arrInput[1]);
                    }
                    else if (arrInput[0] == "import")
                    {
                        cmd.import(arrInput[1]);
                    }
                    else if (arrInput[0] == "rename")
                    {
                        cmd.rename(arrInput[1], arrInput[2]);
                    }
                    else if (arrInput[0] == "type")
                    {
                        cmd.type(arrInput[1]);
                    }
                    else if (arrInput[0] == "export")
                    {
                        cmd.export(arrInput[1], arrInput[2]);
                    }
                    else if (arrInput[0] == "del")
                    {
                        cmd.del(arrInput[1]);
                    }
                    else if (arrInput[0] == "copy")
                    {
                        cmd.copy(arrInput[1], arrInput[1]);
                    }
                }


            }




        }
    }
}