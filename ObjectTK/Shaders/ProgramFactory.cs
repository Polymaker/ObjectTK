//
// ProgramFactory.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using log4net;
using ObjectTK.Exceptions;
using ObjectTK.Shaders.Sources;

namespace ObjectTK.Shaders
{
    /// <summary>
    /// Contains methods to automatically initialize program objects.
    /// </summary>
    public static class ProgramFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProgramFactory));

        /// <summary>
        /// The base path used when looking for shader files.<br/>
        /// Default is: Data/Shaders/
        /// </summary>
        public static string BasePath { get; set; }
        
        /// <summary>
        /// Specifies the default extension appended to effect names.<br/>
        /// Default is: glsl
        /// </summary>
        public static string Extension { get; set; }

        static ProgramFactory()
        {
            BasePath = Path.Combine("Data", "Shaders");
            Extension = "glsl";
        }

        /// <summary>
        /// Initializes a program object using the shader sources tagged to the type with <see cref="ShaderSourceAttribute"/>.
        /// </summary>
        /// <typeparam name="T">Specifies the program type to create.</typeparam>
        /// <returns>A compiled and linked program.</returns>
        public static T Create<T>()
            where T : Program
        {
            // retrieve shader types and filenames from attributes
            var shaders = ShaderSourceAttribute.GetShaderSources(typeof(T));
            var shaderFiles = typeof(T).GetCustomAttributes<SourceFileAttribute>(false).ToList();

            if (shaders.Count == 0) throw new ObjectTKException("ShaderSourceAttribute(s) missing!");
            // create program instance
            var program = (T)Activator.CreateInstance(typeof(T));
            try
            {
                // compile and attach all shaders
                foreach (var attribute in shaders)
                {
                    // create a new shader of the appropriate type
                    using (var shader = new Shader(attribute.Type))
                    {
                        Logger.DebugFormat("Compiling {0}: {1}", attribute.Type, attribute.EffectKey);
                        // load the source from effect(s)
                        var included = new List<Effect.Section>();
                        //GetShaderSource(typeof(T), attribute);
                        var source = GetShaderSource(attribute.EffectKey, shaderFiles, included);
                        // assign source filenames for proper information log output
                        shader.SourceFiles = included.Select(_ => _.Effect.Path).ToList();
                        // compile shader source
                        shader.CompileSource(source);
                        // attach shader to the program
                        program.Attach(shader);
                    }
                }
                // link and return the program
                program.Link();
            }
            catch
            {
                program.Dispose();
                throw;
            }
            return program;
        }

        /// <summary>
        /// Load shader source file(s).<br/>
        /// Supports multiple source files via "#include xx" directives and corrects the line numbering by using the glsl standard #line directive.
        /// </summary>
        /// <param name="effectKey">Specifies the effect key to load.</param>
        /// <param name="included">Holds the effectKeys of all shaders already loaded to prevent multiple inclusions.</param>
        /// <returns>The fully assembled shader source.</returns>
        private static string GetShaderSource(string effectKey, List<SourceFileAttribute> files, List<Effect.Section> included = null)
        {
            if (included == null) included = new List<Effect.Section>();
            // retrieve effect section
            Effect.Section section;
            try
            {
                var directory = Path.GetDirectoryName(effectKey);
                var filename = Path.GetFileName(effectKey);
                var separator = filename.IndexOf('.');
                var sourceName = filename.Substring(0, separator);

                var effectPath = Path.ChangeExtension(Path.Combine(BasePath, directory, sourceName), Extension);

  
                var effectFile = files.FirstOrDefault(x => x.SourceName.ToUpper() == sourceName.ToUpper());

                if (effectFile != null)
                {
                    if (effectFile.Embedded)
                    {
                        //var stream = 
                    }
                    else
                    {
                        if (!File.Exists(effectFile.Filename))
                        {
                            var tmp = Path.Combine(BasePath, effectFile.Filename);
                            if (File.Exists(tmp))
                                effectPath = tmp;
                        }
                    }
                }
                
                var shaderKey = filename.Substring(separator + 1);
                section = Effect.GetSection(effectPath, shaderKey);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Invalid effect key: {0}", effectKey), ex);
            }
            if (section == null) throw new Exception(string.Format("Shader source not found: {0}", effectKey));
            // check for multiple includes of the same section
            if (included.Contains(section))
            {
                Logger.WarnFormat("Shader already included: {0}", effectKey);
                return "";
            }
            included.Add(section);
            // parse source for #include directives and insert proper #line annotations
            using (var reader = new StringReader(section.Source))
            {
                var source = new StringBuilder();
                var lineNumber = section.FirstLineNumber;
                var fileNumber = included.Count-1;
                var fixLine = true;
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    // check if it is an include statement
                    const string includeKeyword = "#include";
                    if (!line.StartsWith(includeKeyword))
                    {
                        // add correct line number offset to the corresponding section within the effect file
                        if (fixLine && !line.StartsWith("#version"))
                        {
                            source.AppendLine(string.Format("#line {0} {1}", lineNumber, fileNumber));
                            fixLine = false;
                        }
                        source.AppendLine(line);
                    }
                    else
                    {
                        // parse the included filename
                        var includedEffectKey = line.Remove(0, includeKeyword.Length).Trim();
                        // make the include path relative to the current file
                        includedEffectKey = Path.Combine(Path.GetDirectoryName(effectKey) ?? "", includedEffectKey);
                        // replace current line with the source of the included section
                        source.Append(GetShaderSource(includedEffectKey, files, included));
                        // remember to fix the line numbering on the next line
                        fixLine = true;
                    }
                    lineNumber++;
                }
                return source.ToString();
            }
        }
    }
}