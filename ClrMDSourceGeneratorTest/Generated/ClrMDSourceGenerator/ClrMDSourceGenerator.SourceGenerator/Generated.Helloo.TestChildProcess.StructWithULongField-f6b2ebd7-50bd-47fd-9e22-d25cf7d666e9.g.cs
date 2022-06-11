
namespace Generated.Helloo.TestChildProcess
{
    public class StructWithULongField
    {
        private ulong _address;

        public global::System.Int32 Field => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field");
public global::System.Int32 Field2 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field2");
public global::System.Int32 Field3 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field3");
public global::System.Int32 Field4 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field4");
public global::System.Int32 Field5 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field5");
public global::System.UInt64 Value => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.UInt64>("Value");


        public StructWithULongField(ulong address)
        {
            _address = address;
        }
    }
}
