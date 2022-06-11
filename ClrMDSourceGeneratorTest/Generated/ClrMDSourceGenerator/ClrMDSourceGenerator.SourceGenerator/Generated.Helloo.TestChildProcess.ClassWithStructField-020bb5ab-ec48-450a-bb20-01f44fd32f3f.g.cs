
namespace Generated.Helloo.TestChildProcess
{
    public class ClassWithStructField
    {
        private ulong _address;

        public global::System.Int32 Field => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field");
public global::System.Int32 Field2 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field2");
public Generated.Helloo.TestChildProcess.StructWithULongField Value;


        public ClassWithStructField(ulong address)
        {
            _address = address;
        }
    }
}
