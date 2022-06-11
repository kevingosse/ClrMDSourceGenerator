using System;
using ClrMDSourceGenerator;

namespace ClrMDSourceGeneratorTest
{
    [MemoryDump(@"E:\DynaMD.TestChildProcess.exe_210123_143317.dmp", "Helloo.*")]
    internal partial class Program
    {
        static void Main(string[] args)
        {
            // This type only exist in the memory dump and is generated at compilation time
            var obj = new Generated.Helloo.TestChildProcess.ClassWithULongField(0x000001b1c1272f18);
            Console.WriteLine(Test.Result);

            
            // Reads the value of the field from the memory dump
            Console.WriteLine(obj.Value);
        }
    }
}
