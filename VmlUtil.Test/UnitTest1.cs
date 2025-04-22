using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using VmlUtil.Core;

namespace VmlUtil.Test
{
    public class Tests
    {
        string xmlString;
        VmlScanner scanner;
        VmlParser parser;

        [SetUp]
        public void Setup()
        {
            using (StreamReader sr = new StreamReader("test.xml"))
            {
                xmlString = sr.ReadToEnd();
            }

            scanner = new VmlScanner(xmlString);
            parser = new VmlParser(scanner);
        }

        [Test]
        public void TestScanner()
        {
            TokenType tokenType;

            while ((tokenType = scanner.Scan()) != TokenType.EndOfSrc)
            {
                Debug.Print("=================");
                Debug.Print($"TokenType: {tokenType.ToString()}");
                if (tokenType == TokenType.NameString || tokenType == TokenType.TextString || tokenType == TokenType.AttributeString)
                {
                    Debug.Print("ValueString: ");
                    Debug.Print(scanner.GetValueString());
                }
            }
        }

        [Test]
        public void TestParser()
        {
            VmlDocument vmlDocument = new VmlDocument();
            vmlDocument.Root = parser.Parse();
            Debug.Print(vmlDocument.ToXml());
        }
    }
}