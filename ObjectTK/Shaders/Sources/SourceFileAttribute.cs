using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTK.Shaders.Sources
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SourceFileAttribute : Attribute
    {
        public string Filename { get; set; }
        public string SourceName { get; set; }
        public bool Embedded { get; set; }

        public SourceFileAttribute(string filename)
        {
            Filename = filename;
        }

        public SourceFileAttribute(string filename, string sourceName) : this(filename)
        {
            SourceName = sourceName;
        }
    }
}
