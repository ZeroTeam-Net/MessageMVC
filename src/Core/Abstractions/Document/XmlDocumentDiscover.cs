using Agebull.Common.Reflection;
using Agebull.EntityModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ZeroTeam.MessageMVC.Documents;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// XML文档解析
    /// </summary>
    public static class XmlDocumentDiscover
    {
        const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        #region XML文档

        static void RegistDocument()
        {
            //foreach (var serviceInfo in StationInfo.Values)
            //{
            //    if (serviceInfo.Aips.Count == 0)
            //        continue;
            //    if (!ZeroAppOption.Instance.Documents.TryGetValue(serviceInfo.Name, out var doc))
            //    {
            //        ZeroAppOption.Instance.Documents.Add(serviceInfo.Name, serviceInfo);
            //        continue;
            //    }
            //    foreach (var api in serviceInfo.Aips)
            //    {
            //        if (!doc.Aips.ContainsKey(api.Key))
            //        {
            //            doc.Aips.Add(api.Key, api.Value);
            //        }
            //        else
            //        {
            //            doc.Aips[api.Key] = api.Value;
            //        }
            //    }
            //}
        }

        static bool IsLetter(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

        static readonly Dictionary<Type, TypeDocument> _typeDocs = new();

        static readonly Dictionary<Type, TypeDocument> _lock = new();

        /// <summary>
        /// 读取类型说明
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TypeDocument ReadEntity(Type type, string name)
        {
            var typeDocument = new TypeDocument
            {
                Name = name,
                TypeName = ReflectionHelper.GetTypeName(type),
                ClassName = ReflectionHelper.GetTypeName(type),
            };

            ReadEntity(ref typeDocument, type);
            return typeDocument;
        }
        static void ReadEntity(ref TypeDocument typeDocument, Type type)
        {
            if (type == null || type.IsAutoClass || !IsLetter(type.Name[0]) ||
                type.IsInterface || type.IsMarshalByRef || type.IsCOMObject ||
                type == typeof(object) || type == typeof(void) ||
                type == typeof(ValueType) || type == typeof(Type) || type == typeof(Enum))
            {
                return;
            }

            if (type.IsEnum)
            {
                typeDocument = new TypeDocument
                {
                    IsEnum = true,
                    IsBaseType = true,
                    Name = type.Name,
                    ClassName = type.GetFullTypeName()
                };
            }
            else if (type == typeof(string))
            {
                typeDocument = new TypeDocument
                {
                    IsBaseType = true,
                    Name = type.Name,
                    ClassName = type.GetFullTypeName()
                };
                return;
            }
            else if (type.IsBaseType())
            {
                typeDocument = new TypeDocument
                {
                    IsBaseType = true,
                    Name = type.Name,
                    ClassName = type.GetFullTypeName()
                };
                return;
            }
            if (type.Namespace == "System" || type.Namespace?.Contains("System.") == true)
            {
                return;
            }

            if (_typeDocs.TryGetValue(type, out var doc))
            {
                foreach (var field in doc.Fields)
                {
                    if (typeDocument.Fields.ContainsKey(field.Key))
                    {
                        typeDocument.Fields[field.Key] = field.Value;
                    }
                    else
                    {
                        typeDocument.Fields.Add(field.Key, field.Value);
                    }
                }
                return;
            }
            if (_lock.TryGetValue(type, out var doc2))
            {
                typeDocument = doc2;
                return;
            }
            _lock.Add(type, typeDocument);
            if (type.IsArray)
            {
                ReadEntity(ref typeDocument, type.Assembly.GetType(type.FullName.Split('[')[0]));
                typeDocument.IsArray = true;
                return;
            }
            if (type.IsGenericType && !type.IsValueType &&
                type.GetGenericTypeDefinition().GetInterface(typeof(IEnumerable<>).FullName) != null)
            {
                ReadEntity(ref typeDocument, type.GetGenericArguments().Last());
                typeDocument.IsArray = true;
                return;
            }

            XmlMember.Find(type);
            if (type.IsEnum)
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (field.IsSpecialName)
                    {
                        continue;
                    }
                    var info = CheckMember(typeDocument, type, field, field.FieldType, false, false, false);
                    if (info != null)
                    {
                        info.TypeName = "int";
                        info.Example = ((int)field.GetValue(null)).ToString();
                        info.JsonName = null;
                    }
                }
                _typeDocs.Add(type, new TypeDocument
                {
                    Fields = typeDocument.Fields?.ToDictionary(p => p.Key, p => p.Value)
                });
                typeDocument.Copy(XmlMember.Find(type));
                return;
            }

            var dc = GetCustomAttribute<DataContractAttribute>(type);
            var jo = GetCustomAttribute<JsonObjectAttribute>(type);

            foreach (var property in type.GetProperties(AllFlags))
            {
                if (property.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, property, property.PropertyType, jo != null, dc != null);
            }
            foreach (var field in type.GetFields(AllFlags))
            {
                if (!char.IsLetter(field.Name[0]) || field.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, field, field.FieldType, jo != null, dc != null);
            }

            _typeDocs.Add(type, new TypeDocument
            {
                Fields = typeDocument.Fields?.ToDictionary(p => p.Key, p => p.Value)
            });
            typeDocument.Copy(XmlMember.Find(type));
        }

        static TypeDocument CheckMember(TypeDocument document, Type parent, MemberInfo member, Type memType, bool json, bool dc, bool checkBase = true)
        {
            if (document.Fields.ContainsKey(member.Name))
            {
                return null;
            }

            var jp1 = GetCustomAttribute<JsonPropertyAttribute>(member);
            var jp2 = GetCustomAttribute<JsonPropertyNameAttribute>(member);
            var dm = GetCustomAttribute<DataMemberAttribute>(member);
            if (json)
            {
                var ji1 = GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>(member);
                var ji2 = GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>(member);
                if (ji1 != null || ji2 != null)
                {
                    return null;
                }
            }
            else if (dc)
            {
                var id = GetCustomAttribute<IgnoreDataMemberAttribute>(member);
                if (id != null)
                {
                    return null;
                }
            }

            var field = new TypeDocument();
            var doc = XmlMember.Find(parent, member.Name);
            field.Copy(doc);
            var isArray = false;
            var isDictionary = false;
            try
            {
                var type = memType;
                if (memType.IsArray)
                {
                    isArray = true;
                    type = type.Assembly.GetType(type.FullName.Split('[')[0]);
                }
                else if (type.IsGenericType)
                {
                    if (memType.GetGenericTypeDefinition().IsSupperInterface(typeof(IEnumerable<>)))
                    {
                        isArray = true;
                        type = type.GetGenericArguments()[0];
                    }
                    else if (memType.GetGenericTypeDefinition().IsSupperInterface(typeof(ICollection<>)))
                    {
                        isArray = true;
                        type = type.GetGenericArguments()[0];
                    }
                    else if (memType.GetGenericTypeDefinition().IsSupperInterface(typeof(IDictionary<,>)))
                    {
                        var fields = type.GetGenericArguments();
                        field.Fields.Add("Key", ReadEntity(fields[0], "Key"));
                        field.Fields.Add("Value", ReadEntity(fields[1], "Value"));
                        isDictionary = true;
                        checkBase = false;
                    }
                }
                if (type.IsEnum)
                {
                    if (checkBase)
                    {
                        field = ReadEntity(type, member.Name);
                    }

                    field.IsBaseType = true;
                    field.IsEnum = true;
                }
                else if (type.IsBaseType())
                {
                    field.IsBaseType = true;
                }
                else if (!isDictionary)
                {
                    if (checkBase)
                    {
                        field = ReadEntity(type, member.Name);
                    }
                    field.IsBaseType = false;
                }
                field.TypeName = ReflectionHelper.GetTypeName(type);
            }
            catch
            {
                field.TypeName = "object";
            }
            if (isArray)
            {
                field.TypeName += "[]";
                field.IsArray = true;
            }
            else if (isDictionary)
            {
                field.TypeName = "Dictionary";
                field.IsArray = false;
                field.IsBaseType = false;
            }

            field.Name = member.Name;
            field.JsonName = jp1?.PropertyName ?? jp2?.Name ?? dm?.Name ?? member.Name;
            field.ClassName = ReflectionHelper.GetTypeName(memType);


            var rule = GetCustomAttribute<DataRuleAttribute>(member);
            if (rule != null)
            {
                field.CanNull = rule.CanNull;
                field.Regex = rule.Regex;
                field.Min = rule.Min;
                field.Max = rule.Max;
            }
            document.Fields.Add(member.Name, field);

            return field;
        }
        static T GetCustomAttribute<T>(Type type) where T : Attribute => type.GetCustomAttributes<T>().FirstOrDefault();
        static T GetCustomAttribute<T>(MemberInfo type) where T : Attribute => type.GetCustomAttributes<T>().FirstOrDefault();

        #endregion
    }
}