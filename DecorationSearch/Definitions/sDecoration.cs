using System.Runtime.InteropServices;

namespace DecorationSearch.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 24)]
    public struct sDecoration
    {
        public int id;
        public int unk;
        public int equippedGear;
        public int equippedSlot;
        public int Quantity;
        public int unk1;
    }
}
