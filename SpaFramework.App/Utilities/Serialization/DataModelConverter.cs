using SpaFramework.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities.Serialization
{
    public static class DataModelConverter
    {
        public static dynamic ConvertToDTO(string context, object src, ConvertToDTOOptions options, string name = "")
        {
            if (src == null)
                return null;

            var dst = new ExpandoObject();
            var dstDict = dst as IDictionary<string, object>;

            var objectExclude = src.GetType().GetCustomAttributes(typeof(ExcludeFromDTOAttribute), true).OfType<ExcludeFromDTOAttribute>().Where(x => x.IsExcludedFromContext(context)).FirstOrDefault();
            var objectInclude = src.GetType().GetCustomAttributes(typeof(IncludeInDTOAttribute), true).OfType<IncludeInDTOAttribute>().Where(x => string.IsNullOrEmpty(x.Context) || x.Context == context).FirstOrDefault();

            foreach (var srcProperty in src.GetType().GetProperties())
            {
                try
                {
                    if (srcProperty.GetCustomAttributes(typeof(ExcludeFromDTOAttribute), true).OfType<ExcludeFromDTOAttribute>().Where(x => x.IsExcludedFromContext(context)).Any())
                        continue;

                    if (objectExclude != null && objectExclude.Properties.Contains(srcProperty.Name))
                        continue;

                    bool isExplicitInclude = false;
                    if (srcProperty.GetCustomAttributes(typeof(IncludeInDTOAttribute), true).OfType<IncludeInDTOAttribute>().Where(x => string.IsNullOrEmpty(x.Context) || x.Context == context).Any())
                        isExplicitInclude = true;

                    if (objectInclude != null && objectInclude.Properties.Contains(srcProperty.Name))
                        isExplicitInclude = true;

                    string srcPropertyName = name + (name != "" ? "." : "") + ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name);

                    if (options.ExplicitExcludes.Contains(srcPropertyName))
                        continue;

                    if (srcProperty.PropertyType.IsValueType || srcProperty.PropertyType == typeof(string) || srcProperty.PropertyType == typeof(Guid) || srcProperty.PropertyType == typeof(Dictionary<string, string>) || srcProperty.PropertyType == typeof(Dictionary<Guid, bool>))
                    {
                        dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), srcProperty.GetValue(src));
                    }
                    else if (srcProperty.PropertyType == typeof(DateTime))
                    {
                        dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), ((DateTime)srcProperty.GetValue(src)).ToString("o"));
                    }
                    else if (srcProperty.PropertyType == typeof(DateTime?))
                    {
                        dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), ((DateTime?)srcProperty.GetValue(src))?.ToString("o"));
                    }
                    else if (srcProperty.PropertyType.IsGenericType && srcProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        if (!isExplicitInclude && !options.ExplicitIncludes.Contains(srcPropertyName))
                            continue;

                        IList srcItems = (IList)srcProperty.GetValue(src);

                        if (srcItems == null)
                            dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), null);
                        else
                        {
                            List<object> dstItems = new List<object>();

                            foreach (object item in srcItems)
                            {
                                Type itemType = item.GetType();
                                if (itemType.IsValueType || itemType == typeof(string) || itemType == typeof(Guid))
                                    dstItems.Add(item);
                                else
                                    dstItems.Add(ConvertToDTO(context, item, options, srcPropertyName));
                            }

                            dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), dstItems);
                        }
                    }
                    else if (srcProperty.PropertyType.IsEnum)
                    {
                        string enumName = Enum.GetName(srcProperty.PropertyType, srcProperty.GetValue(src));

                        dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), enumName);
                    }
                    else
                    {
                        if (!isExplicitInclude && !options.ExplicitIncludes.Contains(srcPropertyName))
                            continue;

                        dstDict.Add(ConvertPascalCaseToCamelCase(options.Casing, srcProperty.Name), ConvertToDTO(context, srcProperty.GetValue(src), options, srcPropertyName));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error converting " + srcProperty.Name, ex);
                }
            }

            return dst;
        }

        private static string ConvertPascalCaseToCamelCase(Casing casing, string name)
        {
            if (casing == Casing.Camel)
                return SerializationUtilities.ConvertPascalCaseToCamelCase(name);
            else
                return name;
        }

        public static TDataModel ConvertToDataModel<TDataModel>(string context, ExpandoObject src, ConvertToDataModelOptions options = null)
            where TDataModel : class, new()
        {
            return (TDataModel)ConvertToDataModel(context, typeof(TDataModel), src, options);
        }

        public static object ConvertToDataModel(string context, Type dstType, ExpandoObject src, ConvertToDataModelOptions options = null)
        {
            var srcDict = src as IDictionary<string, object>;
            object dst = Activator.CreateInstance(dstType);

            foreach (var dstProperty in dst.GetType().GetProperties())
            {
                try
                {
                    if (dstProperty.GetCustomAttributes(typeof(ExcludeFromDTOAttribute), true).OfType<ExcludeFromDTOAttribute>().Where(x => x.IsExcludedFromContext(context)).Any())
                        continue;

                    string dstPropertyName = SerializationUtilities.ConvertPascalCaseToCamelCase(dstProperty.Name);
                    if (!srcDict.ContainsKey(dstPropertyName))
                        continue;

                    if (!dstProperty.CanWrite)
                        continue;

                    Type dstPropertyType = dstProperty.PropertyType;
                    Type nullableUnderlyingType = Nullable.GetUnderlyingType(dstPropertyType);

                    if (nullableUnderlyingType != null && srcDict[dstPropertyName] == null)
                        continue;
                    
                    if (nullableUnderlyingType != null &&
                        (nullableUnderlyingType.IsValueType || nullableUnderlyingType == typeof(Guid) || nullableUnderlyingType == typeof(DateTime)))
                    {
                        dstPropertyType = Nullable.GetUnderlyingType(dstPropertyType);
                    }

                    if (dstPropertyType == typeof(Guid))
                    {
                        dstProperty.SetValue(dst, Guid.Parse(srcDict[dstPropertyName].ToString()));
                    }
                    else if (dstPropertyType == typeof(string))
                    {
                        dstProperty.SetValue(dst, srcDict[dstPropertyName]);
                    }
                    else if (dstPropertyType == typeof(DateTime))
                    {
                        dstProperty.SetValue(dst, DateTime.Parse(srcDict[dstPropertyName].ToString()));
                    }
                    //else if (dstPropertyType == typeof(DateTime?))
                    //{
                    //    dstProperty.SetValue(dst, srcDict[dstPropertyName] == null ? null : DateTime.Parse(srcDict[dstPropertyName].ToString()));
                    //}
                    else if (dstPropertyType.IsEnum)
                    {
                        dstProperty.SetValue(dst, (int)Enum.Parse(dstPropertyType, srcDict[dstPropertyName].ToString()));
                    }
                    else if (dstPropertyType.IsValueType)
                    {
                        object srcObj = srcDict[dstPropertyName];
                        object dstObj = Convert.ChangeType(srcObj, dstPropertyType);

                        dstProperty.SetValue(dst, dstObj);
                    }
                    else if (dstPropertyType == typeof(Dictionary<string, string>))
                    {
                        var srcPropertyAsDict = srcDict[dstPropertyName] as IDictionary<string, object>;

                        if (srcPropertyAsDict == null)
                            continue;

                        var dstDict = srcPropertyAsDict.ToDictionary(v => v.Key, v => v.Value?.ToString());
                        dstProperty.SetValue(dst, dstDict);
                    }
                    else if (dstPropertyType == typeof(Dictionary<Guid, bool>))
                    {
                        var srcPropertyAsDict = srcDict[dstPropertyName] as IDictionary<string, object>;

                        if (srcPropertyAsDict == null)
                            continue;

                        var dstDict = srcPropertyAsDict.ToDictionary(v => Guid.Parse(v.Key), v => bool.Parse(v.Value?.ToString() ?? "false"));
                        dstProperty.SetValue(dst, dstDict);
                    }
                    else if (dstPropertyType.IsGenericType && dstPropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type genericType = dstPropertyType.GetGenericArguments()[0];
                        IList srcObj = (IList)srcDict[dstPropertyName];

                        if (srcObj == null)
                            continue;

                        IList dstItems = (IList)Activator.CreateInstance(dstPropertyType);

                        foreach (object srcObjItem in srcObj)
                            dstItems.Add(ConvertToDataModel(context, genericType, (ExpandoObject)srcObjItem, options));

                        dstProperty.SetValue(dst, dstItems);
                    }
                    else if (dstPropertyType.IsClass)
                    {
                        var propertyDataModel = ConvertToDataModel(context, dstPropertyType, (ExpandoObject)srcDict[dstPropertyName], options);
                        dstProperty.SetValue(dst, propertyDataModel);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error converting " + dstProperty.Name, ex);
                }
            }

            return dst;
        }
    }
}
