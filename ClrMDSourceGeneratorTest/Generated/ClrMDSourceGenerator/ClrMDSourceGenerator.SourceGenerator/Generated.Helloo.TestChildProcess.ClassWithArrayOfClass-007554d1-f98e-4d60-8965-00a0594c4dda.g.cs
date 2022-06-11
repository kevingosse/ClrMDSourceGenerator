
namespace Generated.Helloo.TestChildProcess
{
    public class ClassWithArrayOfClass
    {
        private ulong _address;

        public global::System.Int32 Field => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field");
public global::System.Int32 Field2 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field2");
public global::System.Int32 Field3 => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("Field3");


        public ClassWithArrayOfClass(ulong address)
        {
            _address = address;
        }
    }
}
