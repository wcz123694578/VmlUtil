using System;
using System.Collections.Generic;
using System.Xml;

namespace VmlUtil.Core
{
    public class VmlParser : IParse
    {
        public VmlParser(VmlScanner scanner)
        {
            this._scanner = scanner;
        }

        /**
         * document ::= element
         * node ::= element | text
         * element ::= (begin_tag node* end_tag) | close_tag
         * begin_tag ::= '<' name_string attribute* '>'
         * end_tag ::= '<' '/' name_string '>'
         * close_tag ::= '<' name_string attribute* '/' '>'
         * attribute ::= name_string '=' attribute_string
         * text ::= text_string
         */

        public VmlElement Parse()
        {
            VmlElement element = new VmlElement();
            TokenType tokenType = this._scanner.Scan();

            if (tokenType == TokenType.EndOfSrc)
            {
                return null;
            }

            else if (tokenType != TokenType.TagBegin)
            {
                throw new Exception("Expected a '<'");
            }

            TokenType next = _scanner.Scan();

            if (next != TokenType.NameString)
            {
                throw new Exception("Expected a tag name.");
            }

            element.TagName = _scanner.GetValueString();

            element.Attributes = parseAttributes();

            next = _scanner.Scan();
            if (next == TokenType.TagClose)
            {
                next = _scanner.Scan();
                if (next != TokenType.TagEnd)
                {
                    throw new Exception("Missing end tag");
                }
                return element;
            }

            if (next != TokenType.TagEnd)
            {
                throw new Exception("Missing end tag");
            }

            element.Children = parseChildren(element.TagName);

            next = _scanner.Scan();
            if (next != TokenType.TagBegin)
            {
                throw new Exception("Expected a '<'");
            }

            next = _scanner.Scan();
            if (next != TokenType.TagClose)
            {
                throw new Exception("Expected a '/'");
            }

            next = _scanner.Scan();
            if (next != TokenType.NameString)
            {
                throw new Exception("Expected a tag name");
            }

            if (_scanner.GetValueString() != element.TagName)
            {
                throw new Exception($"Tag <{element.TagName}> not closed: expected </{element.TagName}>");
            }

            next = _scanner.Scan();
            if ( next != TokenType.TagEnd)
            {
                throw new Exception("Expected a '>'");
            }

            return element;
        }

        private Dictionary<string, string> parseAttributes()
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            var next = _scanner.Scan();
            if (next == TokenType.TagEnd || next == TokenType.TagClose)
            {
                _scanner.Rollback();
                return attributes;
            }

            _scanner.Rollback();
            while (true)
            {
                string key, value;
                next = _scanner.Scan();
                if (next == TokenType.TagEnd || next == TokenType.TagClose)
                {
                    _scanner.Rollback();
                    return attributes;
                }
                if (next != TokenType.NameString)
                {
                    throw new Exception("Expected a attribute name.");
                }

                key = _scanner.GetValueString();
                next =_scanner.Scan();
                if (next != TokenType.EqualSign)
                {
                    throw new Exception($"Attribute \"{key}\" expected a '='");
                }

                next = _scanner.Scan();
                if (next != TokenType.AttributeString)
                {
                    throw new Exception($"Attribute \"{key}\" expected a value");
                }

                value = _scanner.GetValueString();

                attributes[key] = value;
            }

            _scanner.Rollback();
            return attributes;
        }

        private List<VmlNode> parseChildren(string curTagName)
        {
            List<VmlNode> children = new List<VmlNode>();
            var next = _scanner.Scan();
            if (next == TokenType.TagBegin)
            {
                _scanner.Rollback(); return children;
            }

            _scanner.Rollback();

            while (true)
            {
                next = _scanner.Scan();
                if (next == TokenType.TextString)
                {
                    children.Add(new VmlText(_scanner.GetValueString()));
                } 
                else if (next == TokenType.TagBegin)
                {
                    next = _scanner.Scan();
                    if (next == TokenType.TagClose)
                    {
                        next = _scanner.Scan();
                        if (_scanner.GetValueString() == curTagName)
                        {
                            _scanner.Rollback();
                            _scanner.Rollback();
                            _scanner.Rollback();
                            break;
                        }
                    }

                    _scanner.Rollback();
                    _scanner.Rollback();
                    children.Add(Parse());
                }
                else
                {
                    throw new Exception("Invalid node");
                }
            }

            return children;
        }

        private VmlScanner _scanner;
    }
}
