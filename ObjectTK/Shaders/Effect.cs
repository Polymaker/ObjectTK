//
// Effect.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using ObjectTK.Shaders.Sources;

namespace ObjectTK.Shaders
{
    /// <summary>
    /// Represents an effect file which may contain several sections, each containing the source of a shader.<br/>
    /// Similar implementation to GLSW: http://prideout.net/blog/?p=11
    /// </summary>
    public class Effect
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Effect));

        private static readonly Dictionary<string, Effect> Cache = new Dictionary<string, Effect>();

        /// <summary>
        /// Specifies the Path to the effects source file.
        /// </summary>
        public string Path { get; private set; }

        public SourceFile Source { get; private set; }

        /// <summary>
        /// Holds all sections contained within this effect.
        /// </summary>
        private readonly Dictionary<string, Section> _sections;

        public IEnumerable<Section> Sections => _sections.Values;

        /// <summary>
        /// Represents a section within an effect file.
        /// </summary>
        public class Section
        {
            /// <summary>
            /// Holds a reference to the effect which contains this section.
            /// </summary>
            public Effect Effect;

            /// <summary>
            /// The shader key to this section.
            /// </summary>
            public string ShaderKey;

            /// <summary>
            /// The source within this section.
            /// </summary>
            public string Source;

            /// <summary>
            /// Specifies the first line number of this section within the effect file.
            /// </summary>
            public int FirstLineNumber;
        }

        private Effect()
        {
            _sections = new Dictionary<string, Section>();
        }

        private Effect(string path)
        {
            Path = path;
            _sections = new Dictionary<string, Section>();
        }

        private Effect(SourceFile source)
        {
            Source = source;
            _sections = new Dictionary<string, Section>();
        }

        /// <summary>
        /// Retrieves the best matching section to the given shader key.
        /// </summary>
        /// <param name="shaderKey">The shader key to find the best match for.</param>
        /// <returns>A section containing shader source.</returns>
        public Section GetMatchingSection(string shaderKey)
        {
            Section closestMatch = null;
            foreach (var section in _sections.Values)
            {
                // find longest matching section key
                if (shaderKey.ToLower().StartsWith(section.ShaderKey.ToLower()) && (closestMatch == null || section.ShaderKey.Length > closestMatch.ShaderKey.Length))
                {
                    closestMatch = section;
                }
            }
            return closestMatch;
        }

        /// <summary>
        /// Retrieves the best matching section from the effect file.
        /// </summary>
        /// <param name="effectPath">Specifies the path to the effect file.</param>
        /// <param name="shaderKey">The shader key to find the best match for.</param>
        /// <returns>A section containing shader source.</returns>
        public static Section GetSection(string effectPath, string shaderKey)
        {
            return LoadEffect(effectPath).GetMatchingSection(shaderKey);
        }

        public static Effect LoadFrom(SourceFile file)
        {
            // return cached effect if available
            if (Cache.ContainsKey(file.Path)) return Cache[file.Path];

            // otherwise load the whole effect file
            const string sectionSeparator = "--";
            try
            {
                var fileStream = file.GetStream();
                if (fileStream == null)
                {
                    if (file.Embedded)
                        throw new KeyNotFoundException($"The manifest resource name '{file.Path}' was not found in assembly '{file.Assembly.GetName().Name}'");
                    else
                        throw new FileNotFoundException("Effect source file not found", file.Path);
                }

                using (var reader = new StreamReader(fileStream))
                {
                    var effect = new Effect(file);
                    var source = new StringBuilder();
                    Section section = null;
                    var lineNumber = 1;
                    while (!reader.EndOfStream)
                    {
                        // read the file line by line
                        var line = reader.ReadLine();
                        if (line == null) break;
                        // count line number
                        lineNumber++;
                        // append code to current section until a section separator is reached
                        if (!line.StartsWith(sectionSeparator))
                        {
                            source.AppendLine(line);
                            continue;
                        }
                        // write source to current section
                        if (section != null) section.Source = source.ToString();
                        // start new section
                        section = new Section
                        {
                            Effect = effect,
                            ShaderKey = line.Substring(sectionSeparator.Length).Trim(),
                            FirstLineNumber = lineNumber
                        };
                        effect._sections.Add(section.ShaderKey, section);
                        source.Clear();
                    }
                    // make sure the last section is finished
                    if (section != null) section.Source = source.ToString();
                    // cache the effect
                    Cache.Add(file.Path, effect);
                    return effect;
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Effect while reading source file", ex);
                throw;
            }
        }

        public static Effect Parse(string effectSource)
        {
            const string sectionSeparator = "--";
            using (StringReader reader = new StringReader(effectSource))
            {
                var effect = new Effect();
                var source = new StringBuilder();
                Section section = null;
                var lineNumber = 1;
                while (reader.Peek() > 0)
                {
                    // read the file line by line
                    var line = reader.ReadLine();
                    if (line == null) break;
                    // count line number
                    lineNumber++;
                    // append code to current section until a section separator is reached
                    if (!line.StartsWith(sectionSeparator))
                    {
                        source.AppendLine(line);
                        continue;
                    }
                    // write source to current section
                    if (section != null) section.Source = source.ToString();
                    // start new section
                    section = new Section
                    {
                        Effect = effect,
                        ShaderKey = line.Substring(sectionSeparator.Length).Trim(),
                        FirstLineNumber = lineNumber
                    };
                    effect._sections.Add(section.ShaderKey, section);
                    source.Clear();
                }
                // make sure the last section is finished
                if (section != null) section.Source = source.ToString();
                return effect;
            }
        }

        /// <summary>
        /// Loads the the given effect file and parses its sections.
        /// </summary>
        /// <param name="path">Specifies the path to the effect file.</param>
        /// <returns>A new Effect object containing the parsed content from the file.</returns>
        public static Effect LoadEffect(string path)
        {
            // return cached effect if available
            if (Cache.ContainsKey(path)) return Cache[path];
            // otherwise load the whole effect file
            const string sectionSeparator = "--";
            try
            {
                using (var reader = new StreamReader(path))
                {
                    var effect = new Effect(path);
                    var source = new StringBuilder();
                    Section section = null;
                    var lineNumber = 1;
                    while (!reader.EndOfStream)
                    {
                        // read the file line by line
                        var line = reader.ReadLine();
                        if (line == null) break;
                        // count line number
                        lineNumber++;
                        // append code to current section until a section separator is reached
                        if (!line.StartsWith(sectionSeparator))
                        {
                            source.AppendLine(line);
                            continue;
                        }
                        // write source to current section
                        if (section != null) section.Source = source.ToString();
                        // start new section
                        section = new Section
                        {
                            Effect = effect,
                            ShaderKey = line.Substring(sectionSeparator.Length).Trim(),
                            FirstLineNumber = lineNumber
                        };
                        effect._sections.Add(section.ShaderKey, section);
                        source.Clear();
                    }
                    // make sure the last section is finished
                    if (section != null) section.Source = source.ToString();
                    // cache the effect
                    Cache.Add(path, effect);
                    return effect;
                }
            }
            catch (FileNotFoundException ex)
            {
                Logger.Error("Effect source file not found.", ex);
                throw;
            }
        }
    }
}