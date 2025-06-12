using System;
using Spijuniro_Golubiro;
using TralaleroTralala;

namespace TralaleroTralala
{
    public static class DebugLogger
    {
        public static void LogMSG(string message)
        {
            DebugConsole.Instance.Log(message);
        }

        public static void LogError(string message)
        {
            DebugConsole.Instance.Log($"ERROR : {message}");
        }
        public static void LogWarn(string message)
        {
            DebugConsole.Instance.Log($"Warm : {message}" );
        }
        public static void LogDownlods(string message)
        {
            DebugLogger.LogMSG(message);
        }
    }
}