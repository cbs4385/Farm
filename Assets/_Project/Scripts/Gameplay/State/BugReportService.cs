using System.IO;
using System.Text;
using System;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Gathers what goes into a bug report (the logs, the computer, where in the game the player is) and hands it to BugReport. The screen that asks
    // for the report lives in the UI, which never touches file paths or account names (stream mode); everything that needs them is here.
    public static class BugReportService
    {
        public sealed class Outcome
        {
            public bool Ok;
            public string Error;
            public string FileName;                  // the zip, without its folder
            public string Folder;                    // for opening it; never shown
        }

        public static Outcome Send(GameSession session, string subject, string body, bool includeSave)
        {
            var result = BugReport.Send(Request(session, subject, body, includeSave));
            return new Outcome { Ok = result.Ok, Error = result.Error, FileName = result.Ok ? Path.GetFileName(result.ZipPath) : null, Folder = result.Folder };
        }

        public static void OpenFolder(Outcome outcome)
        {
            if (outcome != null && !string.IsNullOrEmpty(outcome.Folder)) BugReport.OpenUrl(BugReport.FolderUrl(outcome.Folder));
        }

        public static BugReport.Request Request(GameSession session, string subject, string body, bool includeSave)
        {
            var inGame = session != null && session.InGame;
            return new BugReport.Request
            {
                Subject = subject,
                Body = body,
                OutputDirectory = BugReport.DefaultFolder(Application.persistentDataPath),
                LogDirectory = Path.Combine(Application.persistentDataPath, "logs"),
                ConsoleLogPath = Application.consoleLogPath,
                SystemInfo = BugReportInfo.System(),
                GameInfo = BugReportInfo.Game(inGame ? session : null),
                SaveJson = inGame && includeSave ? BugReportInfo.Save(session) : null,
            };
        }
    }

    // What is written into a report about the computer and the game. No player or farm names, no account name.
    public static class BugReportInfo
    {
        public static string System()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Operating system: " + SystemInfo.operatingSystem);
            sb.AppendLine("Computer: " + SystemInfo.deviceModel);
            sb.AppendLine("Processor: " + SystemInfo.processorType + " x" + SystemInfo.processorCount);
            sb.AppendLine("Memory: " + SystemInfo.systemMemorySize + " MB");
            sb.AppendLine("Graphics: " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsDeviceVersion + "), " + SystemInfo.graphicsMemorySize + " MB");
            sb.AppendLine("Screen: " + Screen.width + "x" + Screen.height + (Screen.fullScreen ? " full screen" : " window") + ", " + Screen.currentResolution);
            sb.AppendLine("Frame rate target: " + Application.targetFrameRate);
            return sb.ToString();
        }

        public static string Game(GameSession session)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Application.productName} {Application.version} ({Application.platform}, {(Debug.isDebugBuild ? "development" : "release")} build)");
            sb.AppendLine("Language: " + L.Language);
            var settings = GameSession.Settings;
            if (settings != null) sb.AppendLine($"Settings: horror level {settings.HorrorLevel}, stream mode {settings.StreamMode}, relaxed energy {settings.RelaxedEnergy}");
            if (session == null || !session.InGame) { sb.AppendLine("In game: no (a menu)"); return sb.ToString(); }
            var s = session.State;
            sb.AppendLine($"In game: yes. Map {s.CurrentMap}, year {s.Year} season {s.SeasonIndex} day {s.Day}, minute {s.MinuteOfDay}, weather {s.Weather}");
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null) sb.AppendLine($"Player at x {player.transform.position.x:0.00}, y {player.transform.position.y:0.00} (cell {Mathf.FloorToInt(player.transform.position.x)}, {Mathf.FloorToInt(player.transform.position.y)})");
            sb.AppendLine($"Gold {s.Gold}, energy {s.Energy}/{s.MaxEnergy}, health {s.Health}/{s.MaxHealth}");
            sb.AppendLine($"Horror layer: wakefulness {session.GetVar("mythos.wakefulness")}, step {session.GetVar("mythos.step")}, dread {session.GetVar("dread")}, lore {session.GetVar("lore")}");
            sb.AppendLine($"Flags set: {s.Flags.Count}; save slot {session.ActiveSlot}");
            return sb.ToString();
        }

        public static string Save(GameSession session)
        {
            try { return session.StateJson(); }
            catch (Exception e) { Log.Warn("The bug report could not include the save: " + e.Message); return null; }
        }
    }
}
