using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Core.Models
{
    public enum ProjectState : int
    {
        Open = 1,
        OnHold = 2,
        Closed = 3
    }

    public static class ApplicationRoleNames
    {
        /// <summary>
        /// SuperAdmin has complete access
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>
        /// ProjectManager has read/write access to Dealer/project data
        /// </summary>
        public const string ProjectManager = "ProjectManager";

        /// <summary>
        /// ProjectViewer has read access to Dealer/project data
        /// </summary>
        public const string ProjectViewer = "ProjectViewer";

        /// <summary>
        /// ContentManager has read/write access to content data
        /// </summary>
        public const string ContentManager = "ContentManager";
    }

    public enum Casing : int
    {
        /// <summary>
        /// Example: ThisIsPascalCase
        /// </summary>
        Pascal = 1,

        /// <summary>
        /// Example: thisIsCamelCase
        /// </summary>
        Camel = 2
    }

    public static class ModelContexts
    {
        public const string WebApi = "WebApi";
        public const string BigQuery = "BigQuery";
        public const string WebApiElevated = "WebApiElevated";
        public const string DealerApi = "DealerApi";
        public const string Reporting = "Reporting";
    }
}
