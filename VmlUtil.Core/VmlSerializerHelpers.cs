using System;
using System.Reflection;

namespace VmlUtil.Core
{
    public static class VmlSerializerHelpers
    {

        internal static void AddChildFromObj(VmlElement parent, object obj)
        {
            var child = new VmlElement(obj.GetType().Name);
            child.Children.Add(new VmlText(obj.ToString()));
            parent.Children.Add(child);
        }

        //internal static void AddChildFromSimpleType(VmlElement parent, object obj)
        //{
        //    var typeName = obj.GetType().Name;

        //    if ()

        //    var child = new VmlElement(obj.GetType().Name);
        //    child.Children.Add(new VmlText(obj.ToString()));
        //    parent.Children.Add(child);
        //}

        internal static bool isFundamental(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type.Equals(typeof(string)) || type.Equals(typeof(DateTime));
        }

        internal static bool isIgnored(PropertyInfo p)
        {
            return p.GetCustomAttribute<VmlIgnoreAttribute>() != null;
        }
    }
}