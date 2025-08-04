// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Helpers;
public class TestLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _entries = new();
    public IReadOnlyList<LogEntry> Entries => _entries;

    public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, _entries);
    public void Dispose() { }

    private class TestLogger(string category, List<LogEntry> entries) : ILogger
    {
        private readonly List<LogEntry> _entries = entries;
        private readonly string _category = category;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            _entries.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = formatter(state, exception),
                Exception = exception,
                State = state,
                Category = _category
            });
        }
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }

    public record LogEntry
    {
        public string? Category { get; init; }
        public LogLevel LogLevel { get; init; }
        public EventId EventId { get; init; }
        public string? Message { get; init; }
        public Exception? Exception { get; init; }
        public object? State { get; init; }
    }
}
