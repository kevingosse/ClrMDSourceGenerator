
namespace Generated.Helloo.TestChildProcess
{
    public class StructWithStructWithStructField
    {
        private ulong _address;

        public global::System.Int32 Field1 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field1");
public global::System.Int32 Field2 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field2");
public Generated.Helloo.TestChildProcess.StructWithStructField Value;


        public StructWithStructWithStructField(ulong address)
        {
            _address = address;
        }
    }
}
