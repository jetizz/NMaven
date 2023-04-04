using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NMaven.Logging
{
    public class TaskLogger : ITaskLogger
    {
        private readonly TaskLoggingHelper _loggingHelper;

        public TaskLogger(TaskLoggingHelper loggingHelper)
        {
            _loggingHelper = loggingHelper;
        }

        public void LogMessage(string message)
        {
            _loggingHelper.LogMessage(message);
        }

        public void LogImportantMessage(string message)
        {
            _loggingHelper.LogMessage(MessageImportance.High, message);
        }

        public void LogWarning(string warning)
        {
            _loggingHelper.LogWarning(warning);
        }

        public void LogError(string error)
        {
            _loggingHelper.LogError(error);
        }
    }
}
