using System;
using System.Collections.Generic;
using System.Text;

namespace VmlUtil.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class VmlAttributeAttribute : Attribute
    {
        public string Name { get; }
        public VmlAttributeAttribute(string name = null)
        {
            this.Name = name;
        }
    }
}
