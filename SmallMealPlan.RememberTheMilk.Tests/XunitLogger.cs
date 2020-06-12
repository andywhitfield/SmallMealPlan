using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SmallMealPlan.RememberTheMilk.Tests
{
    public class XunitLogger
    {
        private readonly LoggerFactory _loggerFactory;

        public XunitLogger(ITestOutputHelper testOutputHelper)
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }

        public ILogger<T> CreateLogger<T>() => _loggerFactory.CreateLogger<T>();

        class XunitLoggerProvider : ILoggerProvider
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            public ILogger CreateLogger(string categoryName) => new XunitTestOutputLogger(_testOutputHelper, categoryName);

            public void Dispose()
            { }
        }

        class XunitTestOutputLogger : ILogger
        {
            private readonly ITestOutputHelper _testOutputHelper;
            private readonly string _categoryName;

            public XunitTestOutputLogger(ITestOutputHelper testOutputHelper, string categoryName)
            {
                _testOutputHelper = testOutputHelper;
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _testOutputHelper.WriteLine($"{DateTime.UtcNow:o} {_categoryName} [{eventId}] {formatter(state, exception)}");
                if (exception != null)
                    _testOutputHelper.WriteLine(exception.ToString());
            }

            private class NoopDisposable : IDisposable
            {
                public static NoopDisposable Instance = new NoopDisposable();
                public void Dispose()
                { }
            }
        }
    }
}