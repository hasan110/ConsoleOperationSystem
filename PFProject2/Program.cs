using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PFProject2
{
    class Program
    {
        static string[] VALID_COMMANDS = {
                                             "ls", "dir", // List Directory
                                             "touch", "mkfile", // Create File
                                             "edit", // Edit/Update file's data
                                             "view", // View file data
                                             "mkdir", // Make Dir
                                             "cd", // goto directory
                                             "cd ..", // 1 step back
                                             "copy", // Copy file to another directory
                                             "cut", // Cut file to another directory
                                             "rename", // Rename file or folder 
                                             "del", // Delete file
                                             "rmdir", // Remove Directory
                                             "exit", // Exit DOS
                                             "cls", // Clear the screen
                                             "help" // List all commands
                                         };

        static int  MSG_TYPE_SUCCESS = 2,
                    MSG_TYPE_ERROR = 3, 
                    MSG_TYPE_WARNING = 4, 
                    MSG_TYPE_NORMAL = 1;

        static string[] FILE_DATA = {};

        static string home_dir_name = "home";
        static string current_dir = "/" + home_dir_name + "/";

        static void Main(string[] args)
        {
            StartTheOS();
        }

        static void StartTheOS () {
            LoadKernel();
            SetTheTitle();
            
            // Start the process
            string input = "";
            string[] cmds = { }; // cmds = commands
            while (true)
            {
                input = UserInput();
                cmds = input.Split(' ');

                if (input == "")
                { /* silence */ }
                else if (input == "exit")
                    break;
                else
                    HandleCommands(cmds);
            }
            Console.WriteLine("Program is exiting...");
        }

        static void HandleCommands(string[] cmds)
        {
            switch (cmds[0].ToLower())
            {
                case "ls":
                case "dir":
                    Handle_LS_DIR();
                    break;

                case "cd":
                    Handle_CD(cmds);
                    break;

                case "touch":
                case "mkfile":
                    Handle_TOUCH(cmds);
                    break;
  
                case "rename":
                    Handle_RENAME(cmds);
                    break;

                case "cut":
                    Handle_CUT(cmds);
                    break;

                case "copy":
                    Handle_COPY(cmds);
                    break;


                case "view":
                    Handle_VIEW(cmds);
                    break;

                case "mkdir":
                    Handle_MKDIR(cmds);
                    break;

                case "del":
                    Handle_DEL(cmds);
                    break;

                case "rmdir":
                    Handle_RMDIR(cmds);
                    break;

                case "help":
                    Handle_HELP(cmds);
                    break;

                case "cls":
                    Console.Clear();
                    break;
                    
                default:
                    Console.WriteLine("Invalid Command: " + cmds[0]);
                    Console.WriteLine("Type help to get list of valid commands");
                    break;
            }
            // 
        }

        // Command Handlers

        public static void Handle_CUT(string[] cmds)
        {
            if (cmds.Length > 2)
            {
                Handle_COPY(cmds);
                int index = GetIndex(cmds[1]);
                FILE_DATA = RemoveIndexFromArray(FILE_DATA, index);
                SaveToFile();
            }
        }

        public static void Handle_COPY(string[] cmds)
        {
            if (cmds.Length > 2)
            {
                int index = GetIndex(cmds[1]);
                if (index > -1)
                {
                    string to_path = cmds[2];
                    bool no_exist = true;
                    if (to_path.StartsWith("/"))
                    {
                        string filedata = GetFileData(cmds[1]);
                        for (int i = 0; i < FILE_DATA.Length; i++)
                        {
                            string this_path = FILE_DATA[i];
                            if (to_path.Length > 0)
                                if (to_path[to_path.Length - 1] != '/')
                                    to_path += "/";

                            if (this_path == to_path)
                            {
                                string file = RemoveStart(FILE_DATA[i], current_dir);
                                FILE_DATA = AddToArray(FILE_DATA, to_path + cmds[1] + ":" + filedata);
                                no_exist = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        string filedata = GetFileData(cmds[1]);
                        for (int i = 0; i < FILE_DATA.Length; i++)
                        {
                            string this_path = RemoveStart(FILE_DATA[i], current_dir);
                            if (to_path.Length > 0)
                                if (to_path[to_path.Length - 1] != '/')
                                    to_path += "/";
                            if (this_path == to_path)
                            {
                                string file = RemoveStart(FILE_DATA[i], current_dir);
                                FILE_DATA = AddToArray(FILE_DATA, current_dir + to_path + cmds[1] + ":" + filedata);
                                no_exist = false;
                                break;
                            }
                        }
                    }
                    if (no_exist) {
                        WriteConsole("Destination path not found...", MSG_TYPE_ERROR);
                    }
                    SaveToFile();
                }
                else {
                    WriteConsole("File '"+cmds[1]+"' does not exist", MSG_TYPE_ERROR);
                }
            }
        }

        public static void Handle_RENAME(string[] cmds)
        {
            string name = SkipAndJoin(cmds, 1);
            if (DirExists(name))
            {
                int index = GetIndex(name);
                string newName = UserInput("Enter new name for this folder >");
                for (int i = 0; i < FILE_DATA.Length; i++)
                {
                    if (FILE_DATA[i].Replace(current_dir, "").StartsWith(name))
                    {
                        FILE_DATA[i] = FILE_DATA[i].Replace(current_dir, "").Replace(name, current_dir + newName);
                    }
                }
                SaveToFile();
                WriteConsole("Folder renamed successfully", MSG_TYPE_SUCCESS);
            }
            else WriteConsole("No folder exists with the given name", MSG_TYPE_ERROR);
            if(FileExists(name)) {
                int index = GetIndex(name);
                if (index > -1)
                {
                    string newName = UserInput("Enter new name for this file >");
                    if (!newName.Contains(" "))
                    {
                        string data = FILE_DATA[index].Split(':')[1];
                        FILE_DATA[index] = current_dir + newName + ":" + data;
                        SaveToFile();
                        WriteConsole("Folder renamed successfully", MSG_TYPE_SUCCESS);
                    }
                    else WriteConsole("No spaces allowed in file's names", MSG_TYPE_WARNING);
                }
                else WriteConsole("No file exists with the given name", MSG_TYPE_ERROR);
            }
        }

        public static void Handle_HELP(string[] cmds)
        {
            Console.WriteLine("Valid Commands: \n---------------\n");
            foreach (var item in VALID_COMMANDS)
                Console.WriteLine(item.ToUpper());
            Console.WriteLine();
        }

        public static void Handle_RMDIR(string[] cmds)
        {
            string dirname = SkipAndJoin(cmds, 1);
            if (DirExists(dirname))
            {
                if (Confirm("Do you really want to delete this folder? "))
                {
                    int[] indexes = new int[FILE_DATA.Length];
                    int size = 0;
                    for (int i = 0; i < FILE_DATA.Length; i++)
                    {
                        string str1 = RemoveStart(FILE_DATA[i], current_dir);
                        if (str1.StartsWith(dirname))
                        {
                            indexes[size] = i;
                            size++;
                        }
                    }
                    FILE_DATA = RemoveIndexesFromArray(FILE_DATA, indexes, size + 1);
                    SaveToFile();
                }
            }
            else WriteConsole("Folder with the given name '" + dirname + "' does not exists", MSG_TYPE_ERROR);
        }

        public static void Handle_DEL(string[] cmds)
        {
            if (cmds.Length > 1)
            {
                int index = GetIndex(cmds[1]);
                if (index > -1)
                {
                    if (Confirm("Do you really want to delete this file? "))
                    {
                        FILE_DATA = RemoveIndexFromArray(FILE_DATA, index);
                        SaveToFile();
                    }
                }
                else WriteConsole("File with the given name '" + cmds[1] + "' does not exists", MSG_TYPE_ERROR);
            }
        }

        public static void Handle_MKDIR(string[] cmds)
        {
            if (cmds.Length > 1 && cmds[1].Trim() != "")
            {
                string name = SkipAndJoin(cmds, 1);
                if (!DirExists(name))
                {
                    FILE_DATA = AddToArray(FILE_DATA, current_dir + name + "/");
                    SaveToFile();
                    WriteConsole("Folder created successfully", MSG_TYPE_SUCCESS);
                }
                else WriteConsole("Folder with the given name is already exist..", MSG_TYPE_ERROR);
            }
        }

        public static void Handle_VIEW(string[] cmds)
        {
            if (cmds.Length > 1 && cmds[1].Trim() != "")
            {
                if (FileExists(cmds[1]))
                {
                    string filedata = GetFileData(cmds[1]);
                    WriteConsole(filedata, MSG_TYPE_NORMAL);
                }
                else WriteConsole("File with the given name does not exists ", MSG_TYPE_ERROR);
            }
        }

        public static void Handle_TOUCH(string[] cmds)
        {
            if (cmds.Length > 1 && cmds[1].Trim() != "")
            {
                if (!FileExists(cmds[1]))
                {
                    string data = cmds.Length > 2 ? SkipAndJoin(cmds, 2) : "";
                    FILE_DATA = AddToArray(FILE_DATA, current_dir + cmds[1] + ":" + data);
                    SaveToFile();
                    Console.WriteLine("File created successfully");
                }
                else Console.WriteLine("File with the given name is already exist..");
            }
        }

        /*
         * Handling CD (Change Directory) Command
         */ 
        public static void Handle_CD(string[] cmds)
        {
            if (cmds.Length > 1 && cmds[1].Trim() != "")
            {
                if (cmds[1] != "..")
                {
                    string dir_name = SkipAndJoin(cmds, 1);
                    if (DirExists(dir_name))
                        current_dir += dir_name + "/";
                    else WriteConsole("No Directory exists with the given name '" + dir_name + "'", 3);
                }
                else
                {
                    if (current_dir.Substring(1, current_dir.Length-1) != home_dir_name) // Check if current directory is root
                    {
                        string curr = current_dir.Split('/')[current_dir.Split('/').Length - 2];
                        current_dir = current_dir.Replace(curr + "/", "");
                    }
                }
            }
        }
        
        /*
         * Handling LS or DIR Commands
         */ 
        public static void Handle_LS_DIR()
        {
            for (int i = 0; i < FILE_DATA.Length; i++)
            {
                string str1 = FILE_DATA[i].Replace(current_dir, "");
                if (str1 != "")
                {
                    if (str1.Split('/').Length > 1 && str1.Split('/')[1] == "")
                        Console.WriteLine("\t <DIR> \t\t" + str1);
                    else if (!str1.Contains('/'))
                        Console.WriteLine("\t <FILE> \t" + str1.Split(':')[0]);
                }
            } 
        }


        // Helper Functions/Method ---------------------------------------------------------------------------

        public static void SetTheTitle()
        {
            Console.Title = "Operating System 3.0 - PAK KIET";
            Console.WriteLine("Programing Fundamentals Operating System 3.0");
            Console.WriteLine("(c) 2019 PAK KIET University. All right reserved");
            Console.WriteLine();
        }

        /// <summary>
        /// Function for getting user input prefixing current path to the Console
        /// </summary>
        /// <param name="preMsg"></param>
        /// <returns></returns>
        public static string UserInput(string preMsg = "") {
            if (preMsg == "")
                Console.Write(current_dir.Substring(0, current_dir.Length - 1) + ">");
            else
                Console.Write(preMsg);
            return Console.ReadLine();
        }

        static int GetIndex(string name) {
            int index = -1;
            for (int i = 0; i < FILE_DATA.Length; i++)
            {
                string item = FILE_DATA[i];
                string str1 = item.Replace(current_dir, "");
                if (str1 != "")
                {
                    if (str1.Split('/').Length > 1 && str1.Split('/')[1] == "")
                    {
                        if (str1.Replace('/', ' ').Trim() == name)
                            index = i;
                    }
                    else if (!str1.Contains('/'))
                    {
                        if (str1.Trim().Split(':')[0] == name)
                            index = i;
                    }
                }

            }

            return index;
        }

        static int[] GetDirIndexes(string name) {
            int len = 0;
            string indexes = "";
            for (int i = 0; i < FILE_DATA.Length; i++)
            {
                string str1 = FILE_DATA[i].Replace(current_dir, "");
                if (str1.StartsWith(name))
                {
                    len++;
                    indexes += (i + ",");
                }
            }
            indexes = indexes.Substring(0, indexes.Length - 1);
            int[] arr = new int[len];

            return arr;
        }

        static bool DirExists(string name) {
            bool yes = false;
            for (int i = 0; i < FILE_DATA.Length; i++)
            {
                string str1 = FILE_DATA[i].Replace(current_dir, "");
                if (str1 != "" && str1.Replace('/', ' ').Trim() == name)
                {
                    yes = true;
                    break;
                }
            }
            return yes;
        }

        static bool FileExists(string name)
        {
            bool yes = false;
            for (int i = 0; i < FILE_DATA.Length; i++)
            {
                string str1 = FILE_DATA[i].Replace(current_dir, "");
                if (str1 != "" && !str1.Contains('/'))
                {
                    yes = str1.Split(':')[0] == name;
                }
            }
            return yes;
        }

        static string GetFileData(string filename) {
            string data = "";
            foreach (var item in FILE_DATA)
            {
                string str1 = item.Replace(current_dir, "");
                if (str1 != "" && !str1.Contains('/'))
                {
                    if (str1.Split(':')[0] == filename)
                    {
                        data = str1.Split(':')[1];
                        break;
                    }
                }
            }
            return data;
        }

        static bool Confirm(string msg) {
            Console.WriteLine(msg);
            Console.WriteLine("Type Y to confirm, or Type any other key to abort");
            string str = Console.ReadLine();

            return str.ToUpper() == "Y";
        }

        /*
         * Write a line to Console
         * @type = 1 = normal, 2 = success, 3 = error, 4 = warning
         */
        static void WriteConsole(string message, int type = 1) {
            if (type == 2) {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (type == 3) {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (type == 4) {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // Filing --------------------------------------------------------------------------------------------

        static void SaveToFile() {
            FileStream fs = new FileStream("datafile.db", FileMode.Truncate, FileAccess.ReadWrite, FileShare.None);
            string alldata = "";
            for (int i = 0; i < FILE_DATA.Length; i++)
                alldata += FILE_DATA[i] + "\n";
            alldata = alldata.Substring(0, alldata.Length - 1);
            byte[] byteArr = new UTF8Encoding(true).GetBytes(alldata);
            fs.Write(byteArr, 0, byteArr.Length);

            fs.Close();
        }

        public static void LoadKernel()
        {
            string file_data_str = "";
            current_dir = "/" + home_dir_name + "/";
            bool does_file_already_exists = File.Exists("datafile.db");
            FileStream fs = new FileStream("datafile.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (!does_file_already_exists)
            {
                byte[] newdata = new UTF8Encoding(true).GetBytes(current_dir);
                fs.Write(newdata, 0, newdata.Length);
            }
            else {
                for (int i = 0; i < fs.Length; i++)
                    file_data_str += (char)fs.ReadByte();
                FILE_DATA = file_data_str.Split('\n');
                current_dir = FILE_DATA[0];
                home_dir_name = current_dir.Substring(1, current_dir.Length - 1);
            }

            fs.Close();
        }

        // Utility Properties ---------------------------------------------------------------------------------

        /*
         * Remove and element from an array
         */
        static string[] RemoveIndexFromArray(string[] arr, int index)
        {
            string[] newArr = new string[arr.Length - 1];
            int na_i = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (i != index)
                {
                    newArr[na_i] = arr[i];
                    na_i++;
                }
            }
            return newArr;
        }

        static string[] RemoveIndexesFromArray(string[] arr, int[] indexes, int size)
        {
            string[] newArr = new string[arr.Length - size];
            int na_i = 0;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                bool met = false;
                for (int j = 0; j < size; j++)
                {
                    if (indexes[j] > 0 && i == indexes[j])
                        met = true;
                }
                if (!met)
                {
                    newArr[na_i] = arr[i];
                    na_i++;
                }

            }
            return newArr;
        }

        /*
         * Similar to Array.Concat(newArray);
         */
        static string[] AddToArray(string[] arr, string str)
        {
            string[] newArr = new string[arr.Length + 1];
            for (int i = 0; i < arr.Length; i++)
                newArr[i] = arr[i];
            newArr[newArr.Length - 1] = str;
            return newArr;
        }

        /*
         * Similar to string.Join(" ", str.Skip(num))
         */
        static string SkipAndJoin(string[] arr, int num)
        {
            string joined = "";
            for (int i = 0; i < arr.Length; i++)
            {
                if (i + 1 > num)
                    joined += arr[i] + " ";
            }
            return joined.Trim();
        }

        static string RemoveStart(string str, string to_rep)
        {
            if (str.Length < to_rep.Length)
                return str;
            return str.Substring(to_rep.Length);
        }


    }
}
