using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroTeam.MessageMVC.Documents;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// API扩展功能
    /// </summary>
    public static class ApiJson
    {
        /// <summary>
        /// 转为Json内容
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="isRoot">是否顶级</param>
        /// <returns></returns>
        public static string JsonExample(this TypeDocument type, bool isRoot = true)
        {
            return JsonExample(type, new HashSet<TypeDocument>(), isRoot);
        }

        static string JsonExample(TypeDocument type, HashSet<TypeDocument> hashSet, bool isRoot)
        {
            if (hashSet.Contains(type))
                return null;
            hashSet.Add(type);
            var code = new StringBuilder();
            if (type.IsArray)
                code.Append("[");
            if (type.IsBaseType)
            {
                code.Append(type.DocExample);
            }
            else
            {
                code.Append("{");
                bool first = true;
                foreach (var field in type.Fields.Values)
                {
                    if (first)
                        first = false;
                    else
                        code.Append(",");

                    code.AppendLine();
                    code.Append($"    '{field.DocName}' : {JsonExample(field, hashSet, false)}");
                }
                code.AppendLine();
                code.Append("}");
            }
            if (type.IsArray)
                code.Append("]");

            var json = code.ToString();
            return isRoot ? json : json.SpaceLine(4).Trim();
        }

        /// <summary>
        /// 参数的Json
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ArgumentJson(this ApiDocument type)
        {
            if (!type.HaseArgument)
                return string.Empty;
            var code = new StringBuilder();
            if (type.Arguments.Count == 1 && !type.Arguments.Values.First().IsBaseType)
            {
                var arg = type.Arguments.Values.First();

                code.Append(JsonExample(arg, new HashSet<TypeDocument>(), false));
            }
            else
            {
                code.AppendLine("{");
                bool first = true;
                foreach (var arg in type.Arguments.Values)
                {
                    if (first)
                        first = false;
                    else
                        code.AppendLine(",");

                    code.Append($"    '{ arg.Name }' : {JsonExample(arg, new HashSet<TypeDocument>(), false)}");
                }
                code.AppendLine();
                code.Append('}');
            }
            return code.ToString();
        }

    }
}