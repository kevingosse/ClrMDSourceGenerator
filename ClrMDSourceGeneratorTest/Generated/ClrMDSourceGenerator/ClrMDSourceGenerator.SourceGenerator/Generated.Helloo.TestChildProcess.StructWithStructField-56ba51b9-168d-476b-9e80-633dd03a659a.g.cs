
namespace Generated.Helloo.TestChildProcess
{
    public class StructWithStructField
    {
        private ulong _address;

        public global::System.Int32 Field1 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field1");
public global::System.Int32 Field2 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field2");
public Generated.Helloo.TestChildProcess.StructWithULongField Value;


        public StructWithStructField(ulong address)
        {
            _address = address;
        }
    }
}
