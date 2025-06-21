// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Provides methods to convert strings to various types.
/// </summary>
static class Converter
{
    /// <summary>
    /// Tries the parse value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the parsing succeeded, <c>false</c> otherwise.</returns>
    public static bool TryParseValue<T>(string? value, out T? result)
    {
        result = default;

        Type type = typeof(T);

        //
        // String
        if (type == typeof(string))
        {
            result = (T?)(object?)value;
            return true;
        }

        if (value is null)
        {
            return false;
        }

        //
        // DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            if (DateTimeUtils.TryParseDate(value, out DateTimeOffset dt))
            {
                result = (T)(object)dt;
                return true;
            }

            return false;
        }

        //
        // DateTime
        if (type == typeof(DateTime))
        {
            if (DateTimeUtils.TryParseDate(value, out DateTimeOffset dt))
            {
                result = (T)(object) dt.DateTime;
                return true;
            }

            return false;
        }

        //
        // Enum
        if (type.IsEnum)
        {
            if (Enum.TryParse(typeof(T), value, true, out object? o))
            {
                result = (T)o;
                return true;
            }
        }

        //
        // Uri
        if (type == typeof(Uri))
        {
            if (UriUtils.TryParse(value, out Uri uri))
            {
                result = (T)(object)uri;
                return true;
            }

            return false;
        }

        //
        // Fall back default
        return (result = (T)Convert.ChangeType(value, typeof(T))) is not null;
    }
}
