using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace VmlUtil.Core
{
    public class VmlSerializer<T> : ISerialize<T>
    {
        public T Deserialize(VmlDocument vml, Type type)
        {
            if (vml == null) throw new ArgumentNullException(nameof(vml));
            if (vml.Root == null) return default;
            return (T)convertToObject(vml.Root, type);
        }


        public VmlDocument Serialize(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var document = new VmlDocument();
            document.Root = convertToVmlElement(obj);
            return document;
        }

        private VmlElement convertToVmlElement(object obj)
        {
            Type type = obj.GetType();

            string elementName = "";
            var typeAttributes = type.GetCustomAttribute<VmlElementAttribute>();
            elementName = typeAttributes?.Name ?? type.Name;

            var element = new VmlElement(elementName)
            {
                Attributes = new Dictionary<string, string>(),
                Children = new List<VmlNode>()
            };

            var properties = type.GetProperties().Where(p => !VmlSerializerHelpers.isIgnored(p));
            foreach (var property in properties)
            {
                if (VmlSerializerHelpers.isFundamental(property.PropertyType))
                {
                    // 如果getvalue为null
                    var value = property.GetValue(obj)?.ToString() ?? string.Empty;
                    if (property.GetCustomAttribute<VmlAttributeAttribute>() != null)
                    {
                        element.Attributes[property.Name] = value;
                    }
                    else
                    {
                        var child = new VmlElement(property.Name ?? property.PropertyType.Name);
                        child.Children.Add(new VmlText(value));
                        element.Children.Add(child);
                    }
                }
                else
                {
                    var child = property.GetValue(obj);
                    if (child != null)
                    {
                        // 判断是否继承自IEnumerable
                        if (typeof(IEnumerable).IsAssignableFrom(child.GetType()) && property.PropertyType != typeof(string))
                        {
                            AddEnumerableChildren(element, property, child);
                        }
                        else
                        {
                            element.Children.Add(convertToVmlElement(child));
                        }
                    }
                }
            }

            return element;
        }

        private void AddEnumerableChildren(VmlElement parent, PropertyInfo property, object child)
        {
            var childElem = new VmlElement(property.Name);
            foreach (var item in (IEnumerable)child)
            {
                if (VmlSerializerHelpers.isFundamental(item.GetType()))
                {
                    VmlSerializerHelpers.AddChildFromObj(childElem, item);
                }
                else
                {
                    childElem.Children.Add(convertToVmlElement(item));
                }
            }
            parent.Children.Add(childElem);
        }

        private object convertToObject(VmlElement element, Type type)
        {
            // activator 根据type构造对象
            var obj = Activator.CreateInstance(type);

            if (VmlSerializerHelpers.isFundamental(type))
            {
                var value = element.Children[0] as VmlText;
                obj = Convert.ChangeType(value.Content, type);
                return obj;
            }

            var properties = type.GetProperties()
                .Where(p => p.CanWrite && p.GetCustomAttribute<VmlIgnoreAttribute>() == null)
                .ToDictionary(p => GetAttributeName(p), p => p);

            // 设置简单类型的属性
            foreach (var attr in element.Attributes)
            {
                if (properties.TryGetValue(attr.Key, out var prop))
                {
                    var value = ParseValue(attr.Value, prop.PropertyType);
                    prop.SetValue(obj, value);
                }
            }

            // 处理复杂类型的属性
            var childGroups = element.Children
                .OfType<VmlElement>()
                .GroupBy(e => e.TagName);

            foreach (var childGroup in childGroups)
            {
                var prop = properties.Values.FirstOrDefault(p => GetElementName(p) == childGroup.Key);
                if (prop == null) continue;

                if (VmlSerializerHelpers.isFundamental(prop.PropertyType))
                {
                    string value = "";
                    foreach (var elem in childGroup)
                    {
                        foreach (var item in elem.Children)
                        {
                            value = (item as VmlText).Content;
                        }
                    }
                    prop.SetValue(obj, value);
                }
                else
                {
                    if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                    {
                        var itemType = prop.PropertyType.GetGenericArguments()[0];
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

                        foreach (var childElement in childGroup)
                        {
                            foreach (var item in childElement.Children)
                            {
                                //list.Add(convertToObject(item, itemType));
                                if (item is VmlText)
                                {
                                    list.Add((item as VmlText).Content);
                                }
                                else if (item is VmlElement)
                                {
                                    list.Add(convertToObject((VmlElement)item, itemType));
                                }
                            }
                        }

                        prop.SetValue(obj, list);
                    }
                    else
                    {
                        // 单个复杂对象
                        var childElement = childGroup.FirstOrDefault();
                        if (childElement != null)
                        {
                            var value = convertToObject(childElement, prop.PropertyType);
                            prop.SetValue(obj, value);
                        }
                    }

                }
            }

            return obj;
        }

        private string GetElementName(PropertyInfo property)
        {
            var attr = property.PropertyType.GetCustomAttribute<VmlElementAttribute>();
            return attr?.Name ?? property.Name;
        }

        private object ParseValue(string value, Type propertyType)
        {
            if (string.IsNullOrEmpty(value)) return null;

            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (propertyType == typeof(string)) return value;
            if (propertyType == typeof(int)) return int.Parse(value);
            if (propertyType == typeof(bool)) return bool.Parse(value);
            if (propertyType == typeof(DateTime)) return DateTime.Parse(value);
            if (propertyType == typeof(decimal)) return decimal.Parse(value);
            if (propertyType == typeof(double)) return double.Parse(value);
            if (propertyType == typeof(Guid)) return Guid.Parse(value);

            throw new NotSupportedException($"Not a support type convert: {propertyType.Name}");
        }

        private string GetAttributeName(PropertyInfo p)
        {
            var attr = p.GetCustomAttribute<VmlAttributeAttribute>();
            return attr?.Name ?? p.Name;
        }
    }
}
