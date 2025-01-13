using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;

public class SimpleServiceLogger : InterviewTask.Helpers.ILogger
{
    public void Log(LogLevel level, string message)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

        string relativePath = ConfigurationManager.AppSettings["LogFilePath"];
        string absolutePath = HostingEnvironment.MapPath(relativePath);

        if (absolutePath == null)
        {
            throw new InvalidOperationException("Unable to resolve the path. Check your configuration.");
        }

        File.AppendAllText(absolutePath, logMessage + Environment.NewLine);
    }

}
