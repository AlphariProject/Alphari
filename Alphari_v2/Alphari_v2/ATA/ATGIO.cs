using Cosmos.Core;

public class ATGIO
{
    public readonly ushort Data;
    public readonly ushort Error;
    public readonly ushort Features;
    public readonly ushort SectorCount;
    public readonly ushort LBA0;
    public readonly ushort LBA1;
    public readonly ushort LBA2;
    public readonly ushort DeviceSelect;
    public readonly ushort Status;
    public readonly ushort Command;
    public readonly ushort AlternateStatus;
    public readonly ushort Control;

    public ATGIO(uint bar0, uint bar2)
    {
        Data = (ushort)(bar0 + 0);
        Error = (ushort)(bar0 + 1);
        Features = (ushort)(bar0 + 1);
        SectorCount = (ushort)(bar0 + 2);
        LBA0 = (ushort)(bar0 + 3);
        LBA1 = (ushort)(bar0 + 4);
        LBA2 = (ushort)(bar0 + 5);
        DeviceSelect = (ushort)(bar0 + 6);
        Status = (ushort)(bar0 + 7);
        Command = (ushort)(bar0 + 7);

        AlternateStatus = (ushort)(bar2 + 2);
        Control = (ushort)(bar2 + 2);
    }
}