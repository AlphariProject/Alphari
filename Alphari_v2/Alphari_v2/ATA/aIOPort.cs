using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphari_v2.ATA
{
    public class aIOPort : IOPort
    {
        public aIOPort(ushort aPort) : base(aPort)
        {
        }

        public static byte Rd8(ushort aPort)
        {
            return Read8(aPort);
        }

        public static byte Wr8(ushort aPort, byte aData)
        {
            try
            {
                Write8(aPort, aData);
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public static ushort Rd16(ushort aPort)
        {
            return Read16(aPort);
        }

        public static byte Wr16(ushort aPort, ushort aData)
        {
            try
            {
                Write16(aPort, aData);
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
