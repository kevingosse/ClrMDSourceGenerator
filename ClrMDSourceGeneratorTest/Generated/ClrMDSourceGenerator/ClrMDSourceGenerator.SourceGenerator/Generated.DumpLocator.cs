
using Microsoft.Diagnostics.Runtime;

namespace Generated
{
    public static class DumpLocator
    {
        private static ClrHeap _heap;

        public static ClrHeap GetHeap()
        {
            if (_heap == null)
            {
                var dataTarget = DataTarget.LoadDump(@"E:\DynaMD.TestChildProcess.exe_210123_143317.dmp");
                var runtime = dataTarget.ClrVersions[0].CreateRuntime();
                _heap = runtime.Heap;
            }

            return _heap;
        }
    }
}
