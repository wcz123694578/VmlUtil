using System;

namespace VmlUtil.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class VmlElementAttribute : Attribute
    {
        public string Name { get; }
        public VmlElementAttribute(string name = null)
        {
            this.Name = name;
        }
    }
}