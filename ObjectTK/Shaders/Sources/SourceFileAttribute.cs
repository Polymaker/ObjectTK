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
            SourceName = System.IO.Path.GetFileNameWithoutExtension(filename);
        }

        public SourceFileAttribute(string filename, string sourceName) : this(filename)
        {
            SourceName = sourceName;
        }

        public static List<SourceFile> GetShaderSourceFiles(Type programType, List<ShaderSourceAttribute> shaders)
        {
            var fileAttrs = programType.GetCustomAttributes<SourceFileAttribute>(true).ToList();
            var files = new List<SourceFile>();

            foreach (var attr in fileAttrs)
            {
                if (attr.Embedded)
                    files.Add(new SourceFile(programType.Assembly, attr.Filename, attr.SourceName));
                else
                    files.Add(new SourceFile(attr.SourceName, attr.SourceName));
            }

            foreach (var attr in shaders)
            {
                var sourceName = attr.GetSourceName();

                if (!files.Any(x => x.SourceName.Equals(sourceName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    string filePath = System.IO.Path.ChangeExtension(
                        System.IO.Path.Combine(ProgramFactory.BasePath, 
                        attr.GetDirectoryName(), sourceName), 
                        ProgramFactory.Extension);

                    files.Add(new SourceFile(filePath, sourceName));
                }
            }

            return files;
        }
    }
}
