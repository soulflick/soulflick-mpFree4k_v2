using Mpfree4k.Enums;

namespace Utilities
{
    public static class EnumUtils
    {
        public static FlagType? GetFlagType(string flagString)
        {
            var str = flagString.Trim().ToLower();
            switch (str)
            {
                case "ok": return FlagType.OK;
                case "tagged": return FlagType.Tagged;
                case "hidden": return FlagType.Hidden;
                case "easy": return FlagType.Easy;
                case "new": return FlagType.New;
                case "failures": return FlagType.Failures;
                case "unknown": return FlagType.Unknown;
            }

            return null;
        }
    }
}
