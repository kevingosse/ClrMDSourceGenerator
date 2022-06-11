
using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class MemoryDumpAttribute : Attribute
    {
        public MemoryDumpAttribute(string source, params string[] filters)
        {
            Source = source;
            Filters = filters;
        }

        public string Source { get; set; }

        public string[] Filters { get; set; }
    }
