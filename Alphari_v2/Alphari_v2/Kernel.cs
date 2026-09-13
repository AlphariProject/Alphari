using Alphari_v2.ATA;

using Cosmos.Core;

using Cosmos.Core.IOGroup;

using Cosmos.HAL;

using Cosmos.HAL.BlockDevice;

using Cosmos.System.FileSystem;

using Cosmos.System.FileSystem.VFS;

using Microsoft.CSharp.RuntimeBinder;

using System;

using System.IO;

using System.Threading;

using ATA_PIO = Alphari_v2.ATA.ATA_PIO;

using Sys = Cosmos.System;

namespace Alphari_v2
{
    public class Kernel : Sys.Kernel
    {
        Sys.Console Service;
        public static Disk disk;
        public static ManagedPartition root;
        CosmosVFS vfs;
        //AlphaFS fs;
        BlockDevice rawbd;
        ATA_PIO ataDevice;
        BetterConsole Console;
        bool init = false;
        string cwd = "1:\\";
        protected override void BeforeRun()
        {
            
        }

        protected override void Run()
        {
            if (!init)
            {
                System.Console.WriteLine("Starting preinitializating triggers !!");
                System.Console.WriteLine("  :: Finding PCI-Drive 01:01");
                Console = new BetterConsole();
                PCIDevice drive = PCIUtils.FindFirstDrive();
                if (drive != null)
                {
                    Console.WriteLine("    PCI@Found Drive 01:01 (0,0)");
                }
                else
                {
                    Console.WriteLine("    PCI@None!");
                    Console.WriteLine(@"Kernel Panic | ALPHARI_ERROR
\g@RDump!!
SYS@\nKERNEL
\n:: \wKernel: PCIDevice.Check();
\n:: \wPCI: Not found 01:01
PCI@\rERROR:
\n:: \wNot found any drive device
\n:: \wIDE: Not found 0x01 in PCI
Kernel Panic: Not syncing: \rNo Drive Found
0x0000001C
\nHint: \cTry switch SATA Mode / Drive mode in BIOS to IDE from AHCI.
Halting CPU!
\n:: \wGLOBAL: CPU.Halt();");
                    while (true) CPU.Halt();
                }
                Console.WriteLine(@"\w[ \gOK \w] Getting BARs (0,1)");
                (uint BAR0, uint BAR1) = PCIUtils.GetBars01(drive);

                Console.WriteLine($@"\n  ::\w BAR0: {BAR0}");
                Console.WriteLine($@"\n  ::\w BAR1: {BAR1}");
                Console.WriteLine(@"\w[ \gOK \w] Creating ATA");
                ATGIO ata = new ATGIO((ushort)(BAR0 & 0xFFFC), (ushort)(BAR1 & 0xFFFC));
                Console.WriteLine(@"\w[ \gOK \w] Creating ATA_DRIVE");
                ataDevice = new ATA_PIO(ata, Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Master);
                Console.WriteLine(@"\w[ \gOK \w] Reached target: CHECK_DISK_UNIT");
                if (ataDevice.DriveType != ATA_PIO.SpecLevel.Null)
                {
                    Console.WriteLine(@"\w[\g OK \w] Registered and initializated drive 0");
                    Console.WriteLine($@"\n  ::\w Device name: {ataDevice.ModelNo}");
                    Console.WriteLine($@"\n  ::\w Serial: {ataDevice.SerialNo}");
                }
                else
                {
                    Console.WriteLine(@"Kernel Panic | ALPHARI_ERROR
\g@RDump!!
SYS@\nKERNEL:
\n:: \wKernel: PCIDevice.Check();
\n:: \wKernel: RegDrive!
\n:: \wDRIVE: Error! Drive is NULL
PCI@\rERROR:
\n:: \wKernel: Error in DriveModule (insmod drive)
\n:: \wIDE: Not found valid drive

Kernel Panic: Not syncing: \rNo valid disk found
0x0000001C
\nHint: \cTry disconecting all other drives
Halting CPU!
\n:: \wGLOBAL: CPU.Halt();");
                    while (true) CPU.Halt();
                }
                Console.WriteLine(@"[ \gOK \w] Registering BlockDevice");
                BlockDevice.Devices.Add(ataDevice);
                Console.WriteLine(@"[ \gOK \w] Collecting RawBlockDevice");
                rawbd = BlockDevice.Devices[0];
                Console.WriteLine(@"[ \gOK \w] Initializating Disk as ManagedDrive");
                disk = new Disk(BlockDevice.Devices[BlockDevice.Devices.Count - 1]);
                Console.WriteLine(@"[ \gOK \w] Creating new vFS at 0,0");
                vfs = new CosmosVFS();
                Console.WriteLine(@"[ \gOK \w] Registering vFAT32 vfs at 0,0");
                VFSManager.RegisterVFS(vfs);
                Console.WriteLine(@"[ \gOK \w] Founding normal partition at 0");
                disk.Mount();
                if (disk.Partitions.Count != 0)
                {
                    disk.MountPartition(0);
                    Console.WriteLine($@"  \n:: Found partition {disk.Partitions[0].RootPath}");
                }
                else
                {
                    Console.WriteLine(@"Kernel Panic | ALPHARI_ERROR
\g@RDump!!
SYS@\nKERNEL:
\n:: \wKernel: PCIDevice.Check();
\n:: \wKernel: RegDrive!
\n:: \wDRIVE: Error! No Valid Partition Found
PCI@\rERROR:
\n:: \wKernel: Error in DriveModule (insmod drive)
\n:: \wIDE: Not found valid partition
Kernel Panic: Not syncing: \rNo valid partition found
0x0000001C
\nHint: \cInsert a VALID drive with 1 FAT32 partition.
Halting CPU!
\n:: \wGLOBAL: CPU.Halt();");
                    while (true) CPU.Halt();
                }
                Console.WriteLine(@"[ \gOK \w] Registering partition at 0");
                root = disk.Partitions[0];
                //Console.WriteLine(@"[ \gOK \w] Registering aFS");
                //fs = new AlphaFS(vfs);
                Console.WriteLine(@"[ \gOK \w] Reached target: Shell Init");
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine($@"
\c                      #\b##                  \wOS Name - \cAlphari
\c                     ###\b##                 \wHost    - \yalphari
\c          ###   ##  #+# #++  ++   ++\b+      \wUser    - \yuser
\c         ##### ###+##    +++++++++++\b++     \wKernel  - \cAlpha \gv2.0.0
\c       ###   #+#  #+      +++  +++   \b++#   \wVersion - \cAlphari \gv1.5.23
\c      ##      #####        +++++      \b#++  \wSoftRev - \m12092026
\c     ##         ##          +++         \b+# \wDfwRev  - \m{ataDevice.FirmwareRev}\w

Welcome to Alphari! Type \y'help'\w for more info!
");
                init = true;
            }
            try
            {
                Console.Write(@$"\galphari@user \c{cwd} \g$ \y");
                var input = Console.ReadLine();
                string[] parts = input.Split(' ');
                switch (parts[0])
                {
                    case "help":
                        Console.WriteLine(@"\gAvailiable commands:
help     - show this info
fetch    - show info about OS
poweroff - shutdown the system
reboot   - reset power
touch    - create new file
mkdir    - create new directory
rm       - remove file / catalog recursive [-r]
rmdir    - remove blank catalog
ls       - list entries
cd       - go to directory
cat      - read file
apnd     - append text
host     - print hostname
whoami   - print username
ver      - version of kernel / system");
                        break;
                    case "fetch":
                        Console.WriteLine($@"
\c                      #\b##                  \wOS Name - \cAlphari
\c                     ###\b##                 \wHost    - \yalphari
\c          ###   ##  #+# #++  ++   ++\b+      \wUser    - \yuser
\c         ##### ###+##    +++++++++++\b++     \wKernel  - \cAlpha \gv2.0.0
\c       ###   #+#  #+      +++  +++   \b++#   \wVersion - \cAlphari \gv1.5.23
\c      ##      #####        +++++      \b#++  \wSoftRev - \m12092026
\c     ##         ##          +++         \b+# \wDfwRev  - \m{ataDevice.FirmwareRev}\w
");
                        break;
                    case "": break;
                    case "poweroff": Sys.Power.Shutdown(); break;
                    case "reboot": Sys.Power.Reboot(); break;
                    case "touch": if (parts.Length != 1) vfs.CreateFile(parts[1]); break;
                    case "mkdir": if (parts.Length != 1) vfs.CreateDirectory(parts[1]); break;
                    case "get":
                        switch (parts[1])
                        {
                            case "root":
                                Console.WriteLine(root.MountedFS.RootPath);
                                break;
                            case "drive":
                                Console.WriteLine(@$"REV: {ataDevice.FirmwareRev}
SERIAL: {ataDevice.SerialNo}
MODEL: {ataDevice.ModelNo}
type: {(int)ataDevice.Type}");
                                break;
                        }
                        break;
                    case "rm":
                        if (parts.Length > 1
                        {
                            switch (parts[1])
                            {
                                case "-r":
                                    Directory.Delete(parts[1], true);
                                    break;
                                default:
                                    File.Delete(parts[1]);
                                    break;
                            }
                        }
                        break;
                    case "rmdir": if (parts.Length != 1) Directory.Delete(parts[1], false); break;
                    case "ls":
                        foreach (var e in vfs.GetDirectoryListing(cwd))
                        {
                            Console.WriteLine(e.mEntryType == Sys.FileSystem.Listing.DirectoryEntryTypeEnum.Directory ? "\\m" + e.mName : e.mName);
                        }
                        break;
                    case "cd": if (parts.Length != 1) cwd = parts[1]; break;
                    case "cat": foreach (var l in File.ReadAllLines(parts[1])) { Console.WriteLine(l); } break;
                    case "apnd": if (parts.Length > 2) { File.AppendAllText(parts[1], parts[2]); } break;
                    case "host": Console.WriteLine("alphari"); break;
                    cse "whoami": Console.WriteLine("user"); break;
                    case "ver": Console.WriteLine("Alphari v1.5.23"); break;
                    default:
                        Console.WriteLine($"cobalt: Not found: {parts[0]}");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

}
