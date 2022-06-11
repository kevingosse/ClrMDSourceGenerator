
namespace Generated.System
{
    public class DateTime
    {
        private ulong _address;

        public global::System.UInt64 dateData => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.UInt64>("dateData");


        public DateTime(ulong address)
        {
            _address = address;
        }
    }
}
