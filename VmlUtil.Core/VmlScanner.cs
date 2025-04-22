using System;
using System.Collections.Generic;
using System.Text;

namespace VmlUtil.Core
{
    public enum TokenType
    {
        TagBegin,         // <
        TagEnd,           // >
        TagClose,         // /
        
        EqualSign,          // =
        NameString,
        AttributeString,    // a="b"

        TextString,         // <tag>text</text>

        EndOfSrc
    }

    public class VmlScanner : IScan
    {
        public VmlScanner(string source)
        {
            this._source = source;
            this._current = 0;
        }


        public TokenType TokenType { get; }

        public string GetValueString()
        {
            if (this._value_string == null)
            {
                throw new Exception("Invalid string.");
            }
            return this._value_string;
        }

        public TokenType Scan()
        {
            if (isAtEnd()) return TokenType.EndOfSrc;

            _historyPrev.Push(_current);
            char c = advance();

            _prev = _current;

            if (c == '<')
            {
                _currentState = currentState.isNameString;
                return TokenType.TagBegin;
            }
            else if (c == '>')
            {
                _currentState = currentState.isTextString;
                return TokenType.TagEnd;
            }
            else if (c == '/')
            {
                _currentState = currentState.isNameString;
                return TokenType.TagClose;
            }
            else if (Char.IsWhiteSpace(c))
            {
                return Scan();
            }
            else if (c == '\"')
            {
                scanAttributeString();
                return TokenType.AttributeString;
            }
            else if (Char.IsLetter(c))
            {
                return scanString();
            }
            else if (c == '=')
            {
                return TokenType.EqualSign;
            }
            else
            {
                throw new Exception($"Unexpected token: '{c}'");
            }
        }

        public void Rollback()
        {
            if (_historyPrev.Count == 0)
            {
                throw new Exception();
            }
            _current = _historyPrev.Pop();

        }

        #region private methods
        private bool isAtEnd()
        {
            return this._current >= _source.Length;
        }

        private char advance()
        {
            return _source[_current++];
        }

        private void scanAttributeString()
        {
            int pos = _current;
            
            while (peek() != '\"' && !isAtEnd())
            {
                advance();
            }

            if (isAtEnd())
            {
                throw new Exception("Expected the right '\"'");
            }

            advance();

            _value_string = _source.Substring(pos, _current - pos - 1);
        }

        private TokenType scanString()
        {
            _current--;
            if (_currentState == currentState.isTextString)
            {
                scanTextString();
                return TokenType.TextString;
            } 
            else
            {
                scanNameString();
                return TokenType.NameString;
            }
        }

        private void scanNameString()
        {
            int pos = _current;
            bool first = true;
            
            while (!isAtEnd())
            {
                char c = peek();

                if (Char.IsWhiteSpace(c) || c == '/' || c == '>' || c == '=')
                {
                    break;
                }

                if ((Char.IsNumber(c) || Char.IsSeparator(c)) && first)
                {
                    throw new Exception("Tag name or attribute name cannot start with a number or a separator");
                }

                first = false;

                if (c == '<')
                {
                    throw new Exception("Tag name cannot contain '<'");
                }


                advance();
            }

            _value_string = _source.Substring(pos, _current - pos);
        }

        private void scanTextString()
        {
            int pos = _current;

            while (peek() != '<' && !isAtEnd())
            {
                advance();
            }

            _value_string = _source.Substring(pos, _current - pos);
            _value_string = _value_string.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }
        #endregion

        char peek()
        {
            if (isAtEnd()) return '\0';
            return _source[_current];
        }

        #region private fields
        // 源文件/Vml字符串
        private string _source;
        // 属性的值，因为带有引号所以单独列出来判断了
        private string _value_string;
        private int _current = 0;
        private int _prev = 0;
        private enum currentState
        {
            isNameString,
            isTextString,
            none
        }
        private currentState _currentState;
        private Stack<int> _historyPrev = new Stack<int>();
        #endregion
    }
}
