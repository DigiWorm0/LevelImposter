using System;
using System.Collections.Generic;
using LevelImposter.Logging;

namespace LevelImposter.CoreTests.Logging;

public class LILoggerTests
{
    [Fact]
    public void Log_ShouldDispatchLevelAndData()
    {
        var events = new List<(LILogLevel Level, object Data)>();
        Action<LILogLevel, object> handler = (level, data) => events.Add((level, data));

        LILogger.OnLog += handler;
        try
        {
            LILogger.Warn("careful");
        }
        finally
        {
            LILogger.OnLog -= handler;
        }

        var entry = events.ShouldHaveSingleItem();
        entry.Level.ShouldBe(LILogLevel.Warning);
        entry.Data.ShouldBe("careful");
    }

    [Fact]
    public void Log_ShouldMapHelpersToLevels()
    {
        var levels = new List<LILogLevel>();
        Action<LILogLevel, object> handler = (level, _) => levels.Add(level);

        LILogger.OnLog += handler;
        try
        {
            LILogger.Debug("debug");
            LILogger.Info("info");
            LILogger.Msg("message");
            LILogger.Warn("warning");
            LILogger.Error("error");
        }
        finally
        {
            LILogger.OnLog -= handler;
        }

        levels.ShouldBe([
            LILogLevel.Debug,
            LILogLevel.Info,
            LILogLevel.Message,
            LILogLevel.Warning,
            LILogLevel.Error
        ]);
    }

    [Fact]
    public void Log_ShouldIsolateThrowingSubscribers()
    {
        var received = new List<object>();
        Action<LILogLevel, object> thrower = (_, _) => throw new InvalidOperationException("broken listener");
        Action<LILogLevel, object> recorder = (_, data) => received.Add(data);

        LILogger.OnLog += thrower;
        LILogger.OnLog += recorder;
        try
        {
            Should.NotThrow(() => LILogger.Error("still delivered"));
        }
        finally
        {
            LILogger.OnLog -= thrower;
            LILogger.OnLog -= recorder;
        }

        received.ShouldBe(["still delivered"]);
    }

    [Fact]
    public void Log_ShouldStopDeliveringAfterUnsubscribe()
    {
        var received = new List<object>();
        Action<LILogLevel, object> handler = (_, data) => received.Add(data);

        LILogger.OnLog += handler;
        LILogger.Info("first");
        LILogger.OnLog -= handler;
        LILogger.Info("second");

        received.ShouldBe(["first"]);
    }

    [Fact]
    public void LogException_ShouldIncludeExceptionMessage()
    {
        var received = new List<object>();
        Action<LILogLevel, object> handler = (_, data) => received.Add(data);

        LILogger.OnLog += handler;
        try
        {
            LILogger.LogException(new InvalidOperationException("kaboom"));
        }
        finally
        {
            LILogger.OnLog -= handler;
        }

        var message = received.ShouldHaveSingleItem().ToString();
        message.ShouldNotBeNull();
        message.ShouldContain("kaboom");
    }
}
