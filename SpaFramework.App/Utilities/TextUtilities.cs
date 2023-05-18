using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities
{
    public static class TextUtilities
    {
        public static Dictionary<string, string> FormParamStringToArray(string formParamString)
        {
            return formParamString.Split('&').ToDictionary(
                kvp => kvp.Split("=")[0],
                kvp =>
                {
                    var p = kvp.Split("=");
                    if (p.Length == 1)
                        return null;
                    else
                        return p[1];
                });
        }

        public static string InterpolateObject<T>(string value, string objectName, T obj)
        {
            return Regex.Replace(value, @"{(?<exp>[^}]+)}", match => {
                var p = Expression.Parameter(typeof(T), objectName);
                var e = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(new[] { p }, null, match.Groups["exp"].Value);
                return (e.Compile().DynamicInvoke(obj) ?? "").ToString();
            });
        }

        public static string InterpolateObject(string value, Dictionary<string, object> objects)
        {
            var parameters = objects.Select(x => Expression.Parameter(x.Value.GetType(), x.Key)).ToArray();
            var parameterValues = objects.Select(x => x.Value).ToArray();

            return Regex.Replace(value, @"{(?<exp>[^}]+)}", match => {
                var e = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(parameters, null, match.Groups["exp"].Value);
                return (e.Compile().DynamicInvoke(parameterValues) ?? "").ToString();
            });
        }

        public static string Truncate(this string x, int maxLength)
        {
            if (x.Length > maxLength)
                return x.Substring(0, 31);
            else
                return x;
        }

        public static T DeserializeFormRequest<T>(IFormCollection formCollection)
        {
            var dict = JsonConvert.SerializeObject(formCollection.ToDictionary(x => x.Key, x => x.Value.First()));
            return JsonConvert.DeserializeObject<T>(dict);
        }

    }
}
