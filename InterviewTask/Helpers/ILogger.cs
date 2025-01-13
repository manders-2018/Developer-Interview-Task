using Microsoft.Extensions.Logging;

namespace InterviewTask.Helpers
{
    public interface ILogger
    {
        void Log(LogLevel level, string message);
    }
}
