using SpaFramework.App.Models.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    public static class SerializationUtilities
    {
        private static JsonSerializerSettings _jsonSerializerSettings;

        public static JsonSerializerSettings GetJsonDefaultSerializerSettings()
        {
            if (_jsonSerializerSettings == null)
            {
                _jsonSerializerSettings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            }

            return _jsonSerializerSettings;
        }

        /// <summary>
        /// Performs a shallow clone of a model. By default, it removes navigation properties and IEnumerables. This is designed for serializing a data model object without its navigation properties.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="excludeIHasId">If set to true, all properties that implement IHasId will be excluded from the clone (unless listed in explicitIncludes)</param>
        /// <param name="excludeIEnumerable">If set to true, all properties that implement IEnumerable (except for strings) will be excluded from the clone (unless listed in explicitIncludes)</param>
        /// <param name="explicitIncludes">A list of properties to explicitly include, overriding other rules</param>
        /// <param name="explicitExcludes">A list of properties to explicitly exclude, overriding other rules</param>
        /// <returns></returns>
        public static TModel CloneModel<TModel>(TModel model, bool excludeIHasId = true, bool excludeIEnumerable = true, string[] explicitIncludes = null, string[] explicitExcludes = null)
            where TModel : class, new()
        {
            if (explicitIncludes == null)
                explicitIncludes = new string[] { };
            if (explicitExcludes == null)
                explicitExcludes = new string[] { };

            TModel dst = new TModel();
            CloneModel<TModel, TModel>(model, dst, excludeIHasId, excludeIEnumerable, explicitIncludes, explicitExcludes);
            return dst;
        }

        public static void CloneModel<TSrc, TDst>(TSrc src, TDst dst, bool excludeIHasId, bool excludeIEnumerable, string[] explicitIncludes, string[] explicitExcludes)
            where TSrc : class
            where TDst : class
        {
            foreach (var srcProperty in src.GetType().GetProperties())
            {
                try
                {
                    if (explicitExcludes.Contains(srcProperty.Name))
                        continue;

                    var dstProperty = dst.GetType().GetProperty(srcProperty.Name);

                    if (dstProperty == null)
                        continue;

                    if (!dstProperty.CanWrite)
                        continue;

                    if (!explicitIncludes.Contains(srcProperty.Name))
                    {
                        if (excludeIHasId && (srcProperty.PropertyType.GetInterfaces().Contains(typeof(IHasId<long>)) || srcProperty.PropertyType.GetInterfaces().Contains(typeof(IHasId<Guid>))))
                            continue;

                        if (excludeIEnumerable && srcProperty.PropertyType != typeof(string) && srcProperty.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                            continue;
                    }

                    dstProperty.SetValue(dst, srcProperty.GetValue(src));
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static List<PropertyInfo> GetDifferentFields<T>(T obj1, T obj2, bool ignoreObjects = true, bool ignoreConcurrencyCheck = true)
        {
            List<PropertyInfo> changedFields = new List<PropertyInfo>();
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (ignoreObjects)
                {
                    if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if (!Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsPrimitive)
                            continue;
                    }
                    else if (!propertyInfo.PropertyType.IsPrimitive && propertyInfo.PropertyType != typeof(string))
                        continue;
                }

                if (ignoreConcurrencyCheck && propertyInfo.Name == "ConcurrencyCheck")
                    continue;

                // This is a read-only property
                if (propertyInfo.SetMethod == null)
                    continue;

                object val1 = propertyInfo.GetValue(obj1);
                object val2 = propertyInfo.GetValue(obj2);

                if (val1 == null && val2 == null)
                    continue;

                if (val1 != null && val2 != null && val1.ToString() == val2.ToString())
                    continue;

                changedFields.Add(propertyInfo);
            }

            return changedFields;
        }

        public static string ConvertPascalCaseToCamelCase(string src)
        {
            if (string.IsNullOrEmpty(src))
                return src;

            if (src.Length == 1)
                return src.ToUpper();

            return string.Join(".", src.Split(".").Select(x => CamelCasePart(x)).Select(x => x.Trim()));
        }

        private static string CamelCasePart(string src)
        {
            string retVal = "";

            for (int x = 0; x < src.Length; ++x)
            {
                if (char.IsUpper(src[x]))
                    retVal += char.ToLower(src[x]);
                else
                {
                    retVal += src.Substring(x);
                    break;
                }
            }

            return retVal;
        }

        public static string ConvertCamelCaseToPascalCase(string src)
        {
            if (string.IsNullOrEmpty(src))
                return src;

            if (src.Length == 1)
                return src.ToUpper();

            return string.Join(".", src.Split(".").Select(x => x.Length > 1 ? x.Substring(0, 1).ToUpper() + x.Substring(1) : x.ToUpper()).Select(x => x.Trim()));
        }
    }
}
