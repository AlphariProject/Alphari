using Cosmos.Core.Memory;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphari_v2
{
    public class AlphaFS
    {
        CosmosVFS fs;

        public string root; // Root in DOS Path (dev1 -> p1 -> rootpath)
        public string home; // Home dir in UnixPath (root -> home)
        public string rhome; // Home dir for root user in UnixPath
        public string cwd;

        Disk sda;
        ManagedPartition sdap1;

        public AlphaFS(CosmosVFS basefs) 
        {
            Console.WriteLine("Set Base");
            fs = basefs;
            Console.WriteLine("fs is null?");
            if (fs == null) { Console.WriteLine("Yes"); throw new Exception("Invalid FS"); }
            try { Console.WriteLine("CheckReg"); VFSManager.ThrowIfNotRegistered(); }
            catch { throw new Exception("Invalid FS"); }

            Console.WriteLine("Creating directory");
            //fs.CreateDirectory(@"0:\proc");
            //fs.CreateDirectory(@"0:\sys");
            //fs.CreateDirectory(@"0:\dev");

            Console.WriteLine("SDA");
            sda = Kernel.disk;
            Console.WriteLine("SDAP1");
            sdap1 = Kernel.root;
            Console.WriteLine("ROOT");
            root = sdap1.MountedFS.RootPath;

            Console.WriteLine("HOMES");
            home = "/home"; rhome = "/root";
            Console.WriteLine("cwd");
            cwd = home;
        }

        public void CreateFile(string path)
        {
            fs.CreateFile(FromUnix(path));
        }

        public void CreateDirectory(string path)
        {
            fs.CreateDirectory(FromUnix(path));
        }

        public void DeleteFile(string path)
        {
            File.Delete(FromUnix(path));
        }

        public void DeleteDirectory(string path, bool recursive = true)
        {
            Directory.Delete(FromUnix(path), recursive);
        }

        public void List(string path)
        {
            string dosPath = FromUnix(path);

            // Виртуальные файлы для /proc
            if (dosPath.EndsWith(@"\proc") || dosPath == @"0:\proc")
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("sysrq");
                Console.WriteLine("oops_panic");
                Console.WriteLine("cpuinfo");
                Console.WriteLine("meminfo");
                Console.WriteLine("cpuspeed");
                Console.WriteLine("uptime");
                Console.ResetColor();
                return;
            }

            // Виртуальные файлы для /dev
            if (dosPath.EndsWith(@"\dev") || dosPath == @"0:\dev")
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("mem");
                Console.WriteLine("sda");
                Console.WriteLine("vga");
                Console.ResetColor();
                return;
            }

            // Виртуальные файлы для /sys
            if (dosPath.EndsWith(@"\sys") || dosPath == @"0:\sys")
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("devinfo");
                Console.WriteLine("rminfo");
                Console.WriteLine("helplist");
                Console.WriteLine("rootpath");
                Console.WriteLine("kernelinfo");
                Console.ResetColor();
                return;
            }

            // Стандартный вывод реальной файловой системы с вашей цветовой схемой
            try
            {
                foreach (var d in fs.GetDirectoryListing(dosPath))
                {
                    // Директория или файл? В CosmosFS у элементов есть признак типа (например, mEntryType == Directory)
                    // Проверяем расширения и тип
                    if (d.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    else if (d.mName.EndsWith(".sys"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (d.mName.EndsWith(".tsk"))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    else if (d.mName.EndsWith(".hdll"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray; // Цвет по умолчанию для обычных файлов
                    }

                    Console.WriteLine(d.mName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ResetColor();
        }

        public string[] ReadFileLines(string path)
        {
            string dosPath = FromUnix(path);

            if (System.IO.File.Exists(dosPath))
            {
                return System.IO.File.ReadAllLines(dosPath);
            }

            return new string[] { "File not found." };

        }
        public string ReadFile(string path)
        {
            string dosPath = FromUnix(path);
            string fileName = System.IO.Path.GetFileName(dosPath);
            string parentDir = System.IO.Path.GetDirectoryName(dosPath);

            // Обработка /proc
            if (parentDir != null && (parentDir.EndsWith(@"\proc") || dosPath.Contains(@"\proc")))
            {
                switch (fileName.ToLower())
                {
                    case "sysrq":
                        return "SysRq Help - c = crash, p = processes, i = info";
                    case "oops_panic":
                        // Замените на вашу переменную/свойство паники
                        return "false";
                    case "cpuinfo":
                        return $"{Cosmos.Core.CPU.GetCPUVendorName}\n{Cosmos.Core.CPU.GetCPUBrandString()}";
                    case "meminfo":
                        return "Total: " + Cosmos.Core.CPU.GetAmountOfRAM() + " MB";
                    case "cpuspeed":
                        return Cosmos.Core.CPU.GetCPUCycleSpeed().ToString();
                    case "uptime":
                        return Cosmos.Core.CPU.GetCPUUptime().ToString();
                    default:
                        if (fileName.StartsWith("task") && fileName.EndsWith(".tsk"))
                        {
                            return "Task Info Placeholder";
                        }
                        break;
                }
            }

            // Обработка /dev (поддержка sda, sdb и т.д. через sdX)
            if (parentDir != null && (parentDir.EndsWith(@"\dev") || dosPath.Contains(@"\dev")))
            {
                if (fileName.ToLower() == "mem")
                {
                    return "Memory Pointer";
                }
                else if (fileName.ToLower().StartsWith("sd") && fileName.Length == 3 && char.IsLetter(fileName[2]))
                {
                    char diskLetter = char.ToLower(fileName[2]); // 'a' -> 0, 'b' -> 1 и т.д.
                    int diskIndex = diskLetter - 'a';

                    try
                    {
                        var disks = fs.GetDisks();
                        if (disks != null && diskIndex >= 0 && diskIndex < disks.Count)
                        {
                            var targetDisk = disks[diskIndex];
                            byte[] firstSector = new byte[512];
                            targetDisk.Host.ReadBlock(0, 1, ref firstSector);
                            return Encoding.ASCII.GetString(firstSector);
                        }
                    }
                    catch
                    {
                        return "Error reading disk sector";
                    }
                    return "Disk not found";
                }
                else if (fileName.ToLower() == "vga")
                {
                    return "VGA Pointer: 0xB8000";
                }
            }



            // Обработка /sys
            if (parentDir != null && (parentDir.EndsWith(@"\sys") || dosPath.Contains(@"\sys")))
            {
                switch (fileName.ToLower())
                {
                    case "devinfo":
                        return "mem\nsda\nvga";
                    case "rminfo":
                        return "Total: " + Cosmos.Core.CPU.GetAmountOfRAM() + " MB";
                    case "helplist":
                        return "help, write, ls, cat, clear";
                    case "rootpath":
                        return root;
                    case "kernelinfo":
                        return "AlphaKernel (based on cosmos) v17.0";
                }
            }

            // Чтение обычных файлов через стандартную ФС
            if (System.IO.File.Exists(dosPath))
            {
                return System.IO.File.ReadAllText(dosPath);
            }

            return "File not found.";
        }

        public void WriteFile(string path, string content)
        {
            string dosPath = FromUnix(path);
            string fileName = System.IO.Path.GetFileName(dosPath);
            string parentDir = System.IO.Path.GetDirectoryName(dosPath);

            // Обработка /proc
            if (parentDir != null && (parentDir.EndsWith(@"\proc") || dosPath.Contains(@"\proc")))
            {
                switch (fileName.ToLower())
                {
                    case "sysrq":
                        // Обработка команд SysRq на лету при записи
                        char cmd = content.Trim().Length > 0 ? content.Trim()[0] : '\0';
                        if (cmd == 'c')
                        {
                            throw new Exception("sysrq_handle: 0x00@systrigger");
                        }
                        else if (cmd == 'i')
                        {
                            Console.WriteLine($"C - Crash kernel (trigger panic)\nP - show all processes\nI - this info");
                        }
                        return;
                    case "oops_panic":
                        // Меняем значение флага паники в ядре
                        if (bool.TryParse(content.Trim(), out bool panicVal))
                        {
                            // Kernel.oops = panicVal; (подставьте вашу логику или статическое поле)
                        }
                        return;
                }
                return; // Остальные файлы в /proc виртуальные и только для чтения
            }

            // Обработка /dev
            if (parentDir != null && (parentDir.EndsWith(@"\dev") || dosPath.Contains(@"\dev")))
            {
                if (fileName.ToLower() == "mem")
                {
                    // Пример записи по указателю RAT.RamStart (безопасный/небезопасный блок)
                    // unsafe { byte* ptr = (byte*)RAT.RamStart; ... }
                    return;
                }
                else if (fileName.ToLower().StartsWith("sd") && fileName.Length == 3 && char.IsLetter(fileName[2]))
                {
                    char diskLetter = char.ToLower(fileName[2]);
                    int diskIndex = diskLetter - 'a';

                    try
                    {
                        var disks = fs.GetDisks();
                        if (disks != null && diskIndex >= 0 && diskIndex < disks.Count)
                        {
                            var targetDisk = disks[diskIndex];
                            byte[] dataBytes = Encoding.ASCII.GetBytes(content);
                            targetDisk.Host.WriteBlock(0, 1, ref dataBytes);
                        }
                    }
                    catch
                    {
                        // Ошибка записи сектора
                    }
                    return;
                }
                return; // Остальные файлы /dev
            }

            // Обработка /sys (виртуальные, запись обычно не поддерживается)
            if (parentDir != null && (parentDir.EndsWith(@"\sys") || dosPath.Contains(@"\sys")))
            {
                return;
            }

            // Запись в обычные файлы
            System.IO.File.WriteAllText(dosPath, content);
        }

        public bool FileExist(string path)
        {
            if (File.Exists(FromUnix(path)))
            {
                return true;
            }
            return false;
        }

        public bool DirExist(string path)
        {
            if (Directory.Exists(FromUnix(path)))
            {
                return true;
            }
            return false;
        }

        public string ToUnix(string path)
        {
            if (string.IsNullOrEmpty(path)) return "/";

            if (path.StartsWith(root))
            {
                string subPath = path.Substring(root.Length).Replace('\\', '/');
                return "/" + subPath.TrimStart('/');
            }

            string unixPath = path.Replace('\\', '/');
            if (unixPath.Length >= 2 && unixPath[1] == ':')
            {
                char diskNum = unixPath[0];
                unixPath = "/disk" + diskNum + unixPath.Substring(2);
            }

            return unixPath;
        }

        private string FromUnix(string path)
        {
            if (string.IsNullOrEmpty(path)) return root;

            string dosPath = path.Replace('/', '\\');

            if (dosPath.StartsWith("\\disk"))
            {
                dosPath = dosPath.Substring(5);
                if (dosPath.Length > 0)
                {
                    char diskNum = dosPath[0];
                    dosPath = diskNum + ":" + dosPath.Substring(1);
                }
            }
            else if (dosPath.StartsWith("\\"))
            {
                dosPath = root.TrimEnd('\\') + dosPath;
            }

            return dosPath;
        }
    }
}
