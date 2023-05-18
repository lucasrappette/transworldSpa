using AutoMapper;
using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Web.Mappings.Converters
{
    public class MarkdownConverter : IValueConverter<string, string>
    {
        public string Convert(string sourceMember, ResolutionContext context)
        {
            Markdown markdown = new Markdown();
            return markdown.Transform(sourceMember);
        }
    }
}
