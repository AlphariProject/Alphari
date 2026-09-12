using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphari_v2.ATA
{
    public static class PCIUtils
    {
        public static PCIDevice FindFirstDrive()
        {
            foreach (PCIDevice dev in PCI.Devices)
            {
                if (dev.ClassCode == 0x01 && dev.Subclass == 0x01) { Console.WriteLine($"Found 0x01:0x01 !! {dev.BAR0:X8}"); return dev; }
                else continue;
            }
            return null;
        }

        public static (ushort BAR0, ushort BAR2) GetBars02(PCIDevice dev)
        {
            uint bar0 = dev.BAR0 & 0xFFFC;
            uint bar2 = dev.BaseAddressBar[2].BaseAddress & 0xFFFC;

            return ((ushort)bar0, (ushort)bar2);
        }

        public static (ushort BAR0, ushort BAR1) GetBars01(PCIDevice dev)
        {
            uint bar0 = dev.BAR0 & 0xFFFC;
            uint bar2 = dev.BaseAddressBar[1].BaseAddress & 0xFFFC;

            return ((ushort)bar0, (ushort)bar2);
        }

        public static uint[] GetBars(PCIDevice dev)
        {
            uint[] bars = new uint[dev.BaseAddressBar.Length];

            for (int i = 0; i < dev.BaseAddressBar.Length; i++)
            {
                bars[i] = dev.BaseAddressBar[i].BaseAddress & 0xFFFC;
            }
            
            return bars;
        }
    }
}
