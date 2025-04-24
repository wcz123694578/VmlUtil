using NUnit.Framework;
using System;
using System.Collections.Generic;
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

        [VmlElement(name: "UserTbl")]
        public class User
        {
            //[VmlIgnore]
            [VmlAttribute]
            public int Id { get; set; }
            [VmlAttribute]
            public string Name { get; set; }
            public Hobby Hobby { get; set; }
            [VmlAttribute]
            public DateTime CreateTime { get; set; }
        }

        public class Hobby
        {
            [VmlAttribute]
            public string Name { get; set; }
            public string Level { get; set; }
        }


        public class WebSite
        {
            public List<User> Users { get; set; } = new List<User>();
            public List<int> Scores { get; set; } = new List<int>();
        }

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

        [Test]
        public void TestSerializer()
        {
            User user = new User();
            user.Id = 1;
            user.Name = "test";
            user.Hobby = new Hobby() { Name = "swimming", Level = "good" };

            WebSite site = new WebSite();
            site.Users.Add(user);
            user.Id = 2;
            user.Name = "Jack";
            user.Hobby = new Hobby() { Name = "dancing", Level = "excellent" };

            site.Users.Add(user);

            site.Scores.Add(1);
            site.Scores.Add(2);
            site.Scores.Add(3);
            site.Scores.Add(4);
            VmlSerializer<WebSite> serializer = new VmlSerializer<WebSite>();
            var doc = serializer.Serialize(site);
            Debug.Print(doc.ToXml());
        }
        [Test]
        public void TestDeserializer()
        {
            User user = new User();
            user.Id = 1;
            user.Name = "test";
            user.Hobby = new Hobby() { Name = "swimming", Level = "good" };
            user.CreateTime = DateTime.Now;

            WebSite site = new WebSite();
            site.Users.Add(user);
            user.Id = 2;
            user.Name = "Jack";
            user.Hobby = new Hobby() { Name = "dancing", Level = "excellent" };
            user.CreateTime = DateTime.Now;

            site.Users.Add(user);

            site.Scores.Add(1);
            site.Scores.Add(2);
            site.Scores.Add(3);
            site.Scores.Add(4);

            var serializer = new VmlSerializer<WebSite>();
            var doc = serializer.Serialize(site);
            string xml = doc.ToXml();

            WebSite webSite = serializer.Deserialize(doc, typeof(WebSite));
        }
    }
}