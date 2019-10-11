using System;
using System.Reflection;
using log4net;
using OpenTK.Graphics.OpenGL;

namespace ObjectTK.Shaders.Variables
{
    public class StructUniform<T> : ProgramVariable where T : struct
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ArrayUniform<T>));

        private readonly StructFieldInfo[] StructMembers;

        private class StructFieldInfo
        {
            public string Name { get; set; }
            public int BaseLocation { get; set; }
            public FieldInfo FI { get; set; }
            public bool Active => BaseLocation >= 0;
            public StructFieldInfo(FieldInfo fI)
            {
                FI = fI;
                Name = fI.Name;
                BaseLocation = 0;
            }
        }

        /// <summary>
        /// The current value of the uniform.
        /// </summary>
        private T _value;

        /// <summary>
        /// Gets or sets the current value of the shader uniform.
        /// </summary>
        public T Value
        {
            get => _value;
            set => Set(value);
        }

        public StructUniform()
        {
            var fields = typeof(T).GetFields();
            StructMembers = new StructFieldInfo[fields.Length];

            for (int i = 0; i < fields.Length; i++)
                StructMembers[i] = new StructFieldInfo(fields[i]);
        }

        internal override void OnLink()
        {
            bool anyActive = false;
            for (int i = 0; i < StructMembers.Length; i++)
            {
                string uniformName = $"{Name}.{StructMembers[i].Name}";
                int fieldLoc = GL.GetUniformLocation(ProgramHandle, uniformName);
                StructMembers[i].BaseLocation = fieldLoc;
                anyActive |= StructMembers[i].Active;
            }

            Active = anyActive;

            if (!Active) Logger.WarnFormat("Uniform is either not found, not a struct or not used: {0}", Name);
        }

        /// <summary>
        /// Sets the given value to the program uniform.<br/>
        /// Must be called on an active program, i.e. after <see cref="Program"/>.<see cref="Program.Use()"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void Set(T value)
        {
            for (int i = 0; i < StructMembers.Length; i++)
            {
                if (!StructMembers[i].Active)
                    continue;

                //string uniformName = $"{Name}.{StructMembers[i].Name}";
                //int fieldLoc = GL.GetUniformLocation(ProgramHandle, uniformName);

                int fieldLoc = StructMembers[i].BaseLocation;
                object fieldValue = StructMembers[i].FI.GetValue(value);
                UniformSetter.SetGeneric(StructMembers[i].FI.FieldType, fieldLoc, fieldValue);
            }
        }
    }
}
