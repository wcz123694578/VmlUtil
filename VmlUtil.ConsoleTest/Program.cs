using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VmlUtil.ConsoleTest
{
    public  class Program
    {
        public class Hobby
        {
            public string Name { get; set; }
            public string Level { get; set; }
        }

        public class User
        {
            [XmlAttribute]
            public int Id { get; set; }
            [XmlAttribute]
            public string Name { get; set; }
            public Hobby Hobby { get; set; }
        }



        public class WebSite
        {
            public List<User> Users { get; set; } = new List<User>();
            [XmlAttribute]
            public List<int> Score { get; set; } = new List<int>();
        }
        public static void Main(string[] args)
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
            site.Score.Add(1);
            site.Score.Add(2);
            site.Score.Add(3);
            site.Score.Add(4);
            XmlSerializer serializer = new XmlSerializer(typeof(WebSite));


            string xml = "";
            using (TextWriter tw = new StringWriter())
            {
                serializer.Serialize(tw, site);
                Console.WriteLine(tw.ToString());
            }
        }
    }
}
