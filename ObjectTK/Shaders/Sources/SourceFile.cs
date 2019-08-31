using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectTK.Shaders.Sources
{
    public class SourceFile
    {
        public string Path { get; private set; }
        public string SourceName { get; private set; }
        public Assembly Assembly { get; private set; }
        public bool Embedded => Assembly != null;

        public SourceFile(string path, string sourceName = null)
        {
            Path = path;
            SourceName = sourceName;

            if (string.IsNullOrEmpty(SourceName))
                SourceName = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public SourceFile(Assembly assembly, string path, string sourceName)
        {
            Assembly = assembly;
            Path = path;
            SourceName = sourceName;

            if (string.IsNullOrEmpty(SourceName))
            {
                SourceName = path;
                if (SourceName.EndsWith(".glsl"))
                    SourceName = SourceName.Substring(0, SourceName.Length - 5);
            }
        }

        public Stream GetStream()
        {
            if (Embedded)
                return Assembly.GetManifestResourceStream(Path);

            return File.OpenRead(Path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
