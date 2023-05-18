using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities
{
    public static class AppInitUtilities
    {
        private static string _instanceId;
        private static string _assemblyDate;

        public static string GetInstanceId()
        {
            if (_instanceId == null)
                _instanceId = Guid.NewGuid().ToString();

            return _instanceId;
        }

        public static string GetAssemblyDate()
        {
            if (_assemblyDate == null)
            {
                var filePath = typeof(AppInitUtilities).Assembly.Location;
                var fileInfo = new FileInfo(filePath);
                _assemblyDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd-HH-mm-ss");
            }

            return _assemblyDate;
        }

        public static string GetWebsiteDeploymentId()
        {
            return Environment.GetEnvironmentVariable("WEBSITE_DEPLOYMENT_ID");
        }

        public static string GetSlotName()
        {
            return Environment.GetEnvironmentVariable("APPSETTING_SlotName");
        }

        public static void LogEvent(ILogger logger, string component, string eventName)
        {
            logger.LogInformation("AppInit - Component: {Component}, AssemblyDate: {AssemblyDate}, InstanceId: {InstanceId}, SlotName: {SlotName}, Event: {EventName}", component, GetAssemblyDate(), GetInstanceId(), GetSlotName(), eventName);
        }

        public static void LogEvent(ILogger logger, string component, string eventName, string additionalData, params object[] args)
        {
            logger.LogInformation("AppInit - Component: {Component}, AssemblyDate: {AssemblyDate}, InstanceId: {InstanceId}, SlotName: {SlotName}, Event: {EventName} - " + additionalData, component, GetAssemblyDate(), GetInstanceId(), GetSlotName(), eventName, args);
        }

        public static void LogError(ILogger logger, Exception ex, string component, string eventName)
        {
            logger.LogError(ex, "AppInit - Component: {Component}, AssemblyDate: {AssemblyDate}, InstanceId: {InstanceId}, SlotName: {SlotName}, Event: {EventName}", component, GetAssemblyDate(), GetInstanceId(), GetSlotName(), eventName);
        }

        public static void LogError(ILogger logger, Exception ex, string component, string eventName, string additionalData, params object[] args)
        {
            logger.LogError(ex, "AppInit - Component: {Component}, AssemblyDate: {AssemblyDate}, InstanceId: {InstanceId}, SlotName: {SlotName}, Event: {EventName} - " + additionalData, component, GetAssemblyDate(), GetInstanceId(), GetSlotName(), eventName, args);
        }

    }
}
