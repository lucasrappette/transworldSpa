using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpaFramework.Web.Filters.SchemaFilters
{
    public class DtoMappingSchemaFilter : ISchemaFilter
    {
        private static XElement _docXml;

        private static XElement DocXml
        {
            get
            {
                if (_docXml == null)
                    _docXml = XElement.Load(File.OpenText(Path.Combine(AppContext.BaseDirectory, "SpaFramework.Core.xml")));

                return _docXml;
            }
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context?.MemberInfo?.DeclaringType == null)
                return;

            // If there's an explicit description, don't overwrite it
            if (!string.IsNullOrEmpty(schema.Description))
                return;

            string sourceClass = context.MemberInfo.DeclaringType.FullName;
            string sourceProperty = context.MemberInfo.Name;

            if (!sourceClass.EndsWith("DTO"))
                return;

            // Find the corresponding class in the ".Data." namespace instead of ".DTO."
            string targetMember = "P:" + sourceClass.Replace(".DTO.", ".Data.");

            // Strip DTO from the end of the class name
            targetMember = targetMember.Substring(0, targetMember.Length - 3);
            targetMember += "." + sourceProperty;

            XElement targetElement= DocXml.Element("members")?.Elements("member")?.Where(x => x.Attribute("name")?.Value == targetMember).FirstOrDefault();

            if (targetElement != null)
                schema.Description = targetElement.Element("summary")?.Value?.Trim();
        }
    }
}
