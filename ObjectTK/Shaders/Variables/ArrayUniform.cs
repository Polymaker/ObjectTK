using System;
using System.Reflection;
using log4net;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace ObjectTK.Shaders.Variables
{
    /// <summary>
    /// Represents a struct array uniform. <br/>
    /// Each member of each item are mapped to individual uniforms using the nomenclature: &quot;&lt;uniform name&gt;[index].&lt;member name&gt;&quot;
    /// </summary>
    /// <typeparam name="T">The struct type.</typeparam>
    public class ArrayUniform<T> : ProgramVariable where T : struct
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ArrayUniform<T>));

        private readonly UniformStructMember[] StructMembers;

        public int MaxSize { get; set; }

        /// <summary>
        /// The current value of the uniform.
        /// </summary>
        private T[] _value;

        /// <summary>
        /// Gets or sets the current value of the shader uniform.
        /// </summary>
        public T[] Value
        {
            get => _value;
            set => Set(value);
        }

        public ArrayUniform()
        {
            var fields = typeof(T).GetFields();
            StructMembers = new UniformStructMember[fields.Length];
            for (int i = 0; i < fields.Length; i++)
                StructMembers[i] = new UniformStructMember(fields[i]);
        }

        internal override void Initialize(Program program, PropertyInfo property)
        {
            base.Initialize(program, property);
            var sizeAttr = property.GetCustomAttributes<ArraySizeAttribute>(false).FirstOrDefault();
            if (sizeAttr != null)
                MaxSize = sizeAttr.MaxSize;
        }

        internal override void Initialize(Program program, PropertyInfo property)
        {
            base.Initialize(program, property);
        }

        internal override void OnLink()
        {
            Active = CheckIsActive(MaxSize > 0 ? MaxSize : 1);

            if (!Active) Logger.WarnFormat("Uniform is either not found, not an array or not used: {0}", Name);
        }

        public bool CheckIsActive(int maxSize)
        {
            for (int i = 0; i < maxSize; i++)
            {
                foreach (var field in StructMembers)
                {
                    string uniformName = $"{Name}[{i}].{field.Name}";
                    int fieldLoc = GL.GetUniformLocation(ProgramHandle, uniformName);
                    if (fieldLoc >= 0)
                    {
                        Active = true;
                        return Active;
                    }
                }
            }

            return Active;
        }

        /// <summary>
        /// Sets the given value to the program uniform.<br/>
        /// Must be called on an active program, i.e. after <see cref="Program"/>.<see cref="Program.Use()"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void Set(T[] value)
        {
            _value = value;
            for (int i = 0; i < value.Length; i++)
            {
                for (int j = 0; j < StructMembers.Length; j++)
                {
                    string uniformName = $"{Name}[{i}].{StructMembers[j].Name}";
                    int fieldLoc = GL.GetUniformLocation(ProgramHandle, uniformName);
                    if (fieldLoc >= 0)
                    {
                        object fieldValue = StructMembers[j].FI.GetValue(value[i]);
                        UniformSetter.SetGeneric(StructMembers[j].FI.FieldType, fieldLoc, fieldValue);
                    }
                }
            }
        }
    }
}
