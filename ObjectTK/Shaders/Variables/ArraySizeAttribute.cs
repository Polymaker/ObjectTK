using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTK.Shaders.Variables
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ArraySizeAttribute : Attribute
    {
        public int MaxSize { get; set; }

        public ArraySizeAttribute(int maxSize)
        {
            MaxSize = maxSize;
        }
    }
}
