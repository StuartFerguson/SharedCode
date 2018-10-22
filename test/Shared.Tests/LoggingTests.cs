using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.General;
using Shouldly;
using Xunit;

namespace Logging.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void Logger_Initialise_IsInitialised()
        {
            ILogger logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            Logger.Initialise(logger);

            Logger.IsInitialised.ShouldBeTrue();
        }

        [Fact]
        public void Logger_Initialise_NullLogger_ErrorThrown()
        {
            Should.Throw<ArgumentNullException>(() => { Logger.Initialise(null); });
        }

        [Theory]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void Logger_LogMethods_LogWrittenNoErrors(LogLevel loglevel)
        {
            ILogger logger = NullLogger.Instance;

            String message = "Log Message";
            Logger.Initialise(logger);

            switch (loglevel)
            {
                case LogLevel.Critical:
                    Should.NotThrow(() => Logger.LogCritical(new Exception(message)));
                    break;
                case LogLevel.Debug:
                    Should.NotThrow(() => Logger.LogDebug(message));
                    break;
                case LogLevel.Error:
                    Should.NotThrow(() => Logger.LogError(new Exception(message)));
                    break;
                case LogLevel.Information:
                    Should.NotThrow(() => Logger.LogInformation(message));
                    break;
                case LogLevel.Trace:
                    Should.NotThrow(() => Logger.LogTrace(message));
                    break;
                case LogLevel.Warning:
                    Should.NotThrow(() => Logger.LogWarning(message));
                    break;
            }
        }

        [Theory(Skip = "Need to fix these tests")]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void Logger_LogMethods_NullLogger_ErrorThrown(LogLevel logLevel)
        {
            ILogger logger = null;

            String message = "Log Message";

            switch (logLevel)
            {
                case LogLevel.Critical:
                    Should.Throw<InvalidOperationException>(() => Logger.LogCritical(new Exception(message)));
                    break;
                case LogLevel.Debug:
                    Should.Throw<InvalidOperationException>(() => Logger.LogDebug(message));
                    break;
                case LogLevel.Error:
                    Should.Throw<InvalidOperationException>(() => Logger.LogError(new Exception(message)));
                    break;
                case LogLevel.Information:
                    Should.Throw<InvalidOperationException>(() => Logger.LogInformation(message));
                    break;
                case LogLevel.Trace:
                    Should.Throw<InvalidOperationException>(() => Logger.LogTrace(message));
                    break;
                case LogLevel.Warning:
                    Should.Throw<InvalidOperationException>(() => Logger.LogWarning(message));
                    break;
            }
        }
    }
}
