using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectTK.Shaders.Variables
{
    public sealed class UniformStructMember
    {
        /// <summary>
        /// The struct's member name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The location of the uniform within the shader program.
        /// </summary>
        public int Location { get; internal set; }

        /// <summary>
        /// Specifies whether this variable is active.<br/>
        /// Unused variables are generally removed by OpenGL and cause them to be inactive.
        /// </summary>
        public bool Active => Location >= 0;

        internal FieldInfo FI { get; set; }

        internal UniformStructMember(FieldInfo fI)
        {
            FI = fI;
            Name = fI.Name;
            Location = -1;
        }
    }
}
