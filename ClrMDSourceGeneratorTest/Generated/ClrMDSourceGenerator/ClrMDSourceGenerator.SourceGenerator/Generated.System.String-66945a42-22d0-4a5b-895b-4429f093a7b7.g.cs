
namespace Generated.System
{
    public class String
    {
        private ulong _address;

        public global::System.Int32 m_stringLength => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Int32>("m_stringLength");
public global::System.Char m_firstChar => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::System.Char>("m_firstChar");


        public String(ulong address)
        {
            _address = address;
        }
    }
}
