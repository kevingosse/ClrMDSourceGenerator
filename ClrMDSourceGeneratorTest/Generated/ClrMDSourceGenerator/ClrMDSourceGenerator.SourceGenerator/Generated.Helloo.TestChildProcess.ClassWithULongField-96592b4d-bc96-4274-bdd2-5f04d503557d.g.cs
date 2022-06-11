
namespace Generated.Helloo.TestChildProcess
{
    public class ClassWithULongField
    {
        private ulong _address;

        public global::System.UInt64 Value => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.UInt64>("Value");


        public ClassWithULongField(ulong address)
        {
            _address = address;
        }
    }
}
