using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CdsWeb.Logging
{
    public class LoggerTraceListener : TraceListener
    {
        private readonly ILogger _logger;

        public LoggerTraceListener(string name, ILogger logger) : base(name)
        {
            _logger = logger;
        }

        public override void Write(string message)
        {
            _logger.LogInformation(message);
        }

        public override void WriteLine(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
