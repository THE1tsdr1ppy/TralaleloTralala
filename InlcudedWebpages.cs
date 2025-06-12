using System;
using System.Collections.Generic;

namespace TralaleroTralala
{
    public static class IncludedWebpages
    {
        public const string IncludeStart = "TralaleroTralala";
        public const string HistoryURL = "::History";
        public const string SettingsURL = "::Settings";
        public const string IncognitoName = "Spijuniro Golubiro";

        public static readonly Dictionary<string, string> SpecialPages = new Dictionary<string, string>
        {
            { HistoryURL, "about:history" },
            { SettingsURL, "about:settings" }
        };
    }
}