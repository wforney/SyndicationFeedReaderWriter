// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text;

namespace Microsoft.SyndicationFeed.Utils;

internal static class DateTimeUtils
{
    private const string Rfc3339LocalDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
    private const string Rfc3339UTCDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    public static string ToRfc1123String(DateTimeOffset dto) => dto.ToString("r");

    public static string ToRfc3339String(DateTimeOffset dto)
    {
        return dto.Offset == TimeSpan.Zero
            ? dto.ToUniversalTime().ToString(Rfc3339UTCDateTimeFormat, CultureInfo.InvariantCulture)
            : dto.ToString(Rfc3339LocalDateTimeFormat, CultureInfo.InvariantCulture);
    }

    public static bool TryParseDate(string value, out DateTimeOffset result) => TryParseDateRfc3339(value, out result) || TryParseDateRssSpec(value, out result);

    private static void CollapseWhitespaces(StringBuilder builder)
    {
        int index = 0;
        int whiteSpaceStart = -1;
        while (index < builder.Length)
        {
            if (char.IsWhiteSpace(builder[index]))
            {
                if (whiteSpaceStart < 0)
                {
                    whiteSpaceStart = index;
                    // normalize all white spaces to be ' ' so that the date time parsing works
                    builder[index] = ' ';
                }
            }
            else if (whiteSpaceStart >= 0)
            {
                if (index > whiteSpaceStart + 1)
                {
                    // there are at least 2 spaces... replace by 1
                    _ = builder.Remove(whiteSpaceStart, index - whiteSpaceStart - 1);
                    index = whiteSpaceStart + 1;
                }

                whiteSpaceStart = -1;
            }

            ++index;
        }

        // we have already trimmed the start and end so there cannot be a trail of white spaces in the end
        //Fx.Assert(builder.Length == 0 || builder[builder.Length - 1] != ' ', "The string builder doesnt end in a white space");
    }

    private static string NormalizeTimeZone(string rfc822TimeZone, out bool isUtc)
    {
        isUtc = false;
        // return a string in "-08:00" format
        if (rfc822TimeZone[0] is '+' or '-')
        {
            // the time zone is supposed to be 4 digits but some feeds omit the initial 0
            StringBuilder result = new(rfc822TimeZone);
            if (result.Length == 4)
            {
                // the timezone is +/-HMM. Convert to +/-HHMM
                _ = result.Insert(1, '0');
            }

            _ = result.Insert(3, ':');
            return result.ToString();
        }
        switch (rfc822TimeZone)
        {
            case "UT":
            case "Z":
                isUtc = true;
                return "-00:00";

            case "GMT":
                return "-00:00";

            case "A":
                return "-01:00";

            case "B":
                return "-02:00";

            case "C":
                return "-03:00";

            case "D":
            case "EDT":
                return "-04:00";

            case "E":
            case "EST":
            case "CDT":
                return "-05:00";

            case "F":
            case "CST":
            case "MDT":
                return "-06:00";

            case "G":
            case "MST":
            case "PDT":
                return "-07:00";

            case "H":
            case "PST":
                return "-08:00";

            case "I":
                return "-09:00";

            case "K":
                return "-10:00";

            case "L":
                return "-11:00";

            case "M":
                return "-12:00";

            case "N":
                return "+01:00";

            case "O":
                return "+02:00";

            case "P":
                return "+03:00";

            case "Q":
                return "+04:00";

            case "R":
                return "+05:00";

            case "S":
                return "+06:00";

            case "T":
                return "+07:00";

            case "U":
                return "+08:00";

            case "V":
                return "+09:00";

            case "W":
                return "+10:00";

            case "X":
                return "+11:00";

            case "Y":
                return "+12:00";

            default:
                return "";
        }
    }

    private static void TrimStart(StringBuilder sb)
    {
        int i = 0;
        while (i < sb.Length)
        {
            if (!char.IsWhiteSpace(sb[i]))
            {
                break;
            }
            ++i;
        }
        if (i > 0)
        {
            _ = sb.Remove(0, i);
        }
    }

    private static bool TryParseDateRfc3339(string dateTimeString, out DateTimeOffset result)
    {
        const string Rfc3339LocalDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        const string Rfc3339UTCDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

        result = default;

        dateTimeString = dateTimeString.Trim();

        if (dateTimeString[19] == '.')
        {
            // remove any fractional seconds, we choose to ignore them
            int i = 20;
            while (dateTimeString.Length > i && char.IsDigit(dateTimeString[i]))
            {
                ++i;
            }

            dateTimeString = string.Concat(dateTimeString.AsSpan(0, 19), dateTimeString.AsSpan(i));
        }

        if (DateTimeOffset.TryParseExact(
            dateTimeString,
            Rfc3339LocalDateTimeFormat,
            CultureInfo.InvariantCulture.DateTimeFormat,
            DateTimeStyles.None,
            out DateTimeOffset localTime))
        {
            result = localTime;
            return true;
        }

        if (DateTimeOffset.TryParseExact(
            dateTimeString,
            Rfc3339UTCDateTimeFormat,
            CultureInfo.InvariantCulture.DateTimeFormat,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out DateTimeOffset utcTime))
        {
            result = utcTime;
            return true;
        }

        return false;
    }

    private static bool TryParseDateRssSpec(string value, out DateTimeOffset result)
    {
        result = default;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        StringBuilder sb = new(value.Trim());

        if (sb.Length < 18)
        {
            return false;
        }

        if (sb[3] == ',')
        {
            // There is a leading (e.g.) "Tue, ", strip it off
            _ = sb.Remove(0, 4);

            // There's supposed to be a space here but some implementations dont have one
            TrimStart(sb);
        }

        CollapseWhitespaces(sb);

        if (!char.IsDigit(sb[1]))
        {
            _ = sb.Insert(0, '0');
        }

        if (sb.Length < 19)
        {
            return false;
        }

        bool thereAreSeconds = sb[17] == ':';
        int timeZoneStartIndex = thereAreSeconds ? 21 : 18;

        string timeZoneSuffix = sb.ToString()[timeZoneStartIndex..];
        _ = sb.Remove(timeZoneStartIndex, sb.Length - timeZoneStartIndex);

        _ = sb.Append(NormalizeTimeZone(timeZoneSuffix, out bool isUtc));

        string wellFormattedString = sb.ToString();

        string parseFormat = thereAreSeconds ? "dd MMM yyyy HH:mm:ss zzz" : "dd MMM yyyy HH:mm zzz";

        return DateTimeOffset.TryParseExact(
            wellFormattedString,
            parseFormat,
            CultureInfo.InvariantCulture.DateTimeFormat,
            isUtc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None,
            out result);
    }
}