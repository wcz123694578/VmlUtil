using System;
using System.Collections.Generic;
using System.Text;

namespace VmlUtil.Core
{
    public class VmlText : VmlNode
    {
        public VmlText(string content)
        {
            this.Content = content;
        }
        public string Content { get; set; }

        public override string ToXml()
        {
            return Content;
        }
    }
}
