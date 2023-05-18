﻿using Microsoft.OpenApi.Models;
using SpaFramework.Web.Filters.Support;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SpaFramework.Web.Filters
{
    public class ResponseHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionAttributes = context.MethodInfo.GetCustomAttributes<SwaggerResponseHeaderAttribute>();

            foreach (var attr in actionAttributes)
            {
                var response = operation.Responses.FirstOrDefault(x => x.Key == ((int)attr.StatusCode).ToString(CultureInfo.InvariantCulture)).Value;

                if (response != null)
                {
                    if (response.Headers == null)
                    {
                        response.Headers = new Dictionary<string, OpenApiHeader>();
                    }

                    //response.Headers.Add(attr.Name, new OpenApiHeader { Description = attr.Description, Type = attr.Type });
                    response.Headers.Add(attr.Name, new OpenApiHeader
                    {
                        Description = attr.Description
                    });
                }
            }
        }
    }
}
