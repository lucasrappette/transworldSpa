using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpaFramework.App.Utilities
{
    public static class ConventionUtilities
    {
        private static List<Type> _dataModelTypes { get; set; }
        private static List<Type> _dtoTypes { get; set; }
        private static List<Type> _bigQueryDtoTypes { get; set; }
        private static Dictionary<string, XElement> _docs { get; set; }

        private const string _dataModelTypeNamespace = "SpaFramework.App.Models.Data";
        private const string _dtoTypeNamespace = "SpaFramework.DTO";
        private const string _bigQueryDtoTypeNamespace = "SpaFramework.App.Models.BigQuery";

        // DTOs shouldn't have these properties that data models do
        private static string[] _expectedMissingDtoProperties = new string[] {
            "ConcurrencyTimestamp",
            "LoggableName",
            "TrackedChanges"
        };

        private static string[] _expectedMissingBigQueryDtoProperties = new string[]
        {
            "ConcurrencyCheck",
            "ConcurrencyTimestamp",
            "LoggableName",
            "TrackedChanges"
        };

        static ConventionUtilities()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.FullName.StartsWith("SpaFramework."));

            Type dataModelType = Type.GetType("SpaFramework.App.Models.Data.IEntity, SpaFramework.App");
            Type dtoType = Type.GetType("SpaFramework.DTO.IDTO, SpaFramework.DTO");

            _dataModelTypes = types.Where(x => x.GetInterfaces().Contains(dataModelType)).ToList();
            _dtoTypes = types.Where(x => x.GetInterfaces().Contains(dtoType)).ToList();

            _docs = new Dictionary<string, XElement>();

            string[] xmlFiles = new string[]
            {
                "SpaFramework.App.xml",
                "SpaFramework.Core.xml",
                "SpaFramework.Web.xml"
            };

            foreach (string xmlFile in xmlFiles)
            {
                XElement doc = XElement.Load(Path.Combine(AppContext.BaseDirectory, xmlFile));
                foreach (XElement member in doc.Element("members").Elements("member"))
                {
                    string name = member.Attribute("name")?.Value;
                    if (name != null)
                        _docs.Add(name, member);
                }
            }

        }

        public static List<Type> DataModelTypes { get { return _dataModelTypes; } }
        public static List<Type> DtoTypes { get { return _dtoTypes; } }

        private static string GetDataModelName(this Type type)
        {
            return type.FullName.Replace(_dataModelTypeNamespace + ".", "");
        }

        private static string GetDtoModelName(this Type type)
        {
            return type.FullName.Replace(_dtoTypeNamespace + ".", "").Replace("DTO", "");
        }

        private static string GetTypeName(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type).Name + "?";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return "List&lt;" + type.GetGenericArguments().First().Name + "&gt;";
            else
                return type.Name;
        }

        public static string GetSummary()
        {
            Dictionary<string, TypeSet> typesets = new Dictionary<string, TypeSet>();

            foreach (var type in DataModelTypes)
            {
                string name = type.GetDataModelName();
                if (!typesets.ContainsKey(name))
                    typesets[name] = new TypeSet();

                typesets[name].DataModel = type;
            }

            foreach (var type in DtoTypes)
            {
                string name = type.GetDtoModelName();
                if (!typesets.ContainsKey(name))
                    typesets[name] = new TypeSet();

                typesets[name].Dto = type;
            }

            StringBuilder results = new StringBuilder();

            results.Append(@"<html>
<head>
<style>
body { font-family: Arial; }
.class { font-size: 11pt; }
.member { font-size: 9pt; }
.member .name { padding-left: 15px; }
.red { background-color: #CC0000; width: 100px; }
.orange { background-color: #CCCC00; width: 100px; }
.green { background-color: #00CC00; width: 100px; text-align: center; }
.class { background-color: #E9E9E9; }
</style>
<script>
function toggleRows(name) {
    var els = document.getElementsByClassName(name);
    for (var i = 0; i < els.length; ++i) {
        els[i].style.display = els[i].style.display == 'none' ? 'table-row' : 'none';
    }
}
</script>
</head>
<body>
");
            results.AppendLine(@"<table><thead><tr><th>Name</th><th>Data</th><th>API DTO</th><th>BigQuery DTO</th></tr></thead><tbody>");

            foreach (var pair in typesets.OrderBy(x => x.Value.SortOrder).ThenBy(x => x.Key))
                AppendTypeset(results, pair.Key, pair.Value);

            results.AppendLine("</tbody></table>");
            results.AppendLine(@"
</body>
</html>");

            return results.ToString();
        }

        private static void AppendTypeset(StringBuilder results, string name, TypeSet typeset)
        {
            string expandName = name.Replace(".", "_");

            results.Append($"<tr class=\"class\" onclick=\"toggleRows('{expandName}')\"><td class=\"name\">{name}</td>");
            results.Append(typeset.DataModel == null ? "<td class=\"red\"></td>" : "<td class=\"green\">Y</td>");
            results.Append(typeset.Dto == null ? "<td class=\"red\"></td>" : "<td class=\"green\">Y</td>");

            string typeSetSummary = string.Empty;
            if (typeset.DataModel != null)
            {
                string docName = "T:" + typeset.DataModel.FullName;
                if (_docs.ContainsKey(docName))
                    typeSetSummary = _docs[docName].Element("summary")?.Value ?? "";
            }
            results.AppendLine("<td class=\"summary\">" + typeSetSummary + "</td></tr>");

            Dictionary<string, MemberSet> memberSets = typeset.GetMemberSets();
            foreach (var pair in memberSets.OrderBy(x => x.Key))
            {
                bool isIdentified = memberSets.ContainsKey(pair.Key + "Id");
                bool isList = pair.Value.DataModel != null && pair.Value.DataModel.PropertyType.IsGenericType && pair.Value.DataModel.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                bool isNavigableDto = pair.Value.Dto != null && pair.Value.Dto.PropertyType.Name.EndsWith("DTO");

                results.Append($"<tr class=\"member {expandName}\" style=\"display: none;\"><td class=\"name\">{pair.Key}</td>");
                results.Append(pair.Value.DataModel == null ? "<td class=\"red\"></td>" : "<td class=\"green\">" + pair.Value.DataModel.PropertyType.GetTypeName() + "</td>");
                results.Append(pair.Value.Dto == null ? (_expectedMissingDtoProperties.Contains(pair.Key) ? "<td class=\"orange\"></td>" : "<td class=\"red\"></td>") : "<td class=\"green\">" + pair.Value.Dto.PropertyType.GetTypeName() + "</td>");

                string memberSummary = string.Empty;
                if (pair.Value.DataModel != null)
                {
                    string docName = "P:" + typeset.DataModel.FullName + "." + pair.Value.DataModel.Name;
                    if (_docs.ContainsKey(docName))
                        memberSummary = _docs[docName].Element("summary")?.Value ?? "";
                }
                results.AppendLine("<td class=\"summary\">" + memberSummary + "</td></tr>");
            }
        }

        private class MemberSet
        {
            public PropertyInfo DataModel { get; set; }
            public PropertyInfo Dto { get; set; }
        }

        private class TypeSet
        {
            public Type DataModel { get; set; }
            public Type Dto { get; set; }

            public int SortOrder
            {
                get
                {
                    if (DataModel != null && Dto != null)
                        return 1;
                    else if (DataModel != null && Dto != null)
                        return 2;
                    else if (DataModel != null)
                        return 3;
                    else if (Dto != null)
                        return 4;
                    else if (DataModel != null)
                        return 5;
                    else if (Dto != null)
                        return 6;

                    return 0;
                }
            }

            public Dictionary<string, MemberSet> GetMemberSets()
            {
                Dictionary<string, MemberSet> memberSets = new Dictionary<string, MemberSet>();

                if (DataModel != null)
                {
                    foreach (var propertyInfo in DataModel.GetProperties())
                    {
                        if (!memberSets.ContainsKey(propertyInfo.Name))
                            memberSets[propertyInfo.Name] = new MemberSet();

                        memberSets[propertyInfo.Name].DataModel = propertyInfo;
                    }
                }

                if (Dto != null)
                {
                    foreach (var propertyInfo in Dto.GetProperties())
                    {
                        if (!memberSets.ContainsKey(propertyInfo.Name))
                            memberSets[propertyInfo.Name] = new MemberSet();

                        memberSets[propertyInfo.Name].Dto = propertyInfo;
                    }
                }

                return memberSets;
            }


        }
    }
}
