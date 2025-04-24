using System;
using System.Collections.Generic;

namespace VmlUtil.Core
{
    public class VmlElement : VmlNode
    {
        public string TagName { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public List<VmlNode> Children { get; set; } = new List<VmlNode>();

        public VmlElement()
        {
            
        }

        public VmlElement(string tagName)
        {
            this.TagName = tagName;
        }

        public override string ToXml()
        {
            string xml = "";
            xml += $"<{TagName}";
            foreach(var attr in Attributes)
            {
                xml += $" {attr.Key}=\"{attr.Value}\"";
            }
            if (Children.Count == 0)
            {
                xml += "/>";
                return xml;
            }
            xml += ">";
            foreach (var item in Children)
            {
                xml += item.ToXml();
            }
            xml += $"</{TagName}>";
            return xml;
        }
    }
}
