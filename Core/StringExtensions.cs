using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

namespace TimeTable.Core;

public static class StringExtensions
{
    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string EnsureEndsWith(this string str, char c) => str.EnsureEndsWith(c, StringComparison.Ordinal);

    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        return str.EndsWith(c.ToString(), comparisonType) ? str : str + c.ToString();
    }

    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string EnsureEndsWith(
        this string str,
        char c,
        bool ignoreCase,
        CultureInfo culture)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        return str.EndsWith(c.ToString((IFormatProvider)culture), ignoreCase, culture) ? str : str + c.ToString();
    }

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string EnsureStartsWith(this string str, char c) => str.EnsureStartsWith(c, StringComparison.Ordinal);

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        return str.StartsWith(c.ToString(), comparisonType) ? str : c.ToString() + str;
    }

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string EnsureStartsWith(
        this string str,
        char c,
        bool ignoreCase,
        CultureInfo culture)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        return str.StartsWith(c.ToString((IFormatProvider)culture), ignoreCase, culture) ? str : c.ToString() + str;
    }

    /// <summary>
    /// Indicates whether this string is null or an System.String.Empty string.
    /// </summary>
    public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// indicates whether this string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// Gets a substring of a string from beginning of the string.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="str" /> is null</exception>
    /// <exception cref="T:System.ArgumentException">Thrown if <paramref name="len" /> is bigger that string's length</exception>
    public static string Left(this string str, int len)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        if (str.Length < len)
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        return str.Substring(0, len);
    }

    /// <summary>
    /// Converts line endings in the string to <see cref="P:System.Environment.NewLine" />.
    /// </summary>
    public static string NormalizeLineEndings(this string str) =>
        str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);

    /// <summary>Gets index of nth occurence of a char in a string.</summary>
    /// <param name="str">source string to be searched</param>
    /// <param name="c">Char to search in <paramref name="str" /></param>
    /// <param name="n">Count of the occurence</param>
    public static int NthIndexOf(this string str, char c, int n)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        int num = 0;
        for (int index = 0; index < str.Length; ++index)
        {
            if ((int)str[index] == (int)c && ++num == n)
                return index;
        }

        return -1;
    }

    /// <summary>
    /// Removes first occurrence of the given postfixes from end of the given string.
    /// Ordering is important. If one of the postFixes is matched, others will not be tested.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="postFixes">one or more postfix.</param>
    /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
    public static string RemovePostFix(this string str, params string[] postFixes)
    {
        if (str == null)
            return (string)null;
        if (string.IsNullOrEmpty(str))
            return string.Empty;
        if (((ICollection<string>)postFixes).IsNullOrEmpty<string>())
            return str;
        foreach (string postFix in postFixes)
        {
            if (str.EndsWith(postFix))
                return str.Left(str.Length - postFix.Length);
        }

        return str;
    }


    /// <summary>Gets a substring of a string from end of the string.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="str" /> is null</exception>
    /// <exception cref="T:System.ArgumentException">Thrown if <paramref name="len" /> is bigger that string's length</exception>
    public static string Right(this string str, int len)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        if (str.Length < len)
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        return str.Substring(str.Length - len, len);
    }

    /// <summary>
    /// Uses string.Split method to split given string by given separator.
    /// </summary>
    public static string[] Split(this string str, string separator) => str.Split(new string[1]
    {
        separator
    }, StringSplitOptions.None);

    /// <summary>
    /// Uses string.Split method to split given string by given separator.
    /// </summary>
    public static string[] Split(this string str, string separator, StringSplitOptions options) => str.Split(
        new string[1]
        {
            separator
        }, options);

    /// <summary>
    /// Uses string.Split method to split given string by <see cref="P:System.Environment.NewLine" />.
    /// </summary>
    public static string[] SplitToLines(this string str) => str.Split(Environment.NewLine);

    /// <summary>
    /// Uses string.Split method to split given string by <see cref="P:System.Environment.NewLine" />.
    /// </summary>
    public static string[] SplitToLines(this string str, StringSplitOptions options) =>
        str.Split(Environment.NewLine, options);

    /// <summary>Converts PascalCase string to camelCase string.</summary>
    /// <param name="str">String to convert</param>
    /// <param name="invariantCulture">Invariant culture</param>
    /// <returns>camelCase of the string</returns>
    public static string ToCamelCase(this string str, bool invariantCulture = true)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        if (str.Length != 1)
            return (invariantCulture ? char.ToLowerInvariant(str[0]) : char.ToLower(str[0])).ToString() +
                   str.Substring(1);
        return !invariantCulture ? str.ToLower() : str.ToLowerInvariant();
    }

    /// <summary>
    /// Converts PascalCase string to camelCase string in specified culture.
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <param name="culture">An object that supplies culture-specific casing rules</param>
    /// <returns>camelCase of the string</returns>
    public static string ToCamelCase(this string str, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        return str.Length == 1 ? str.ToLower(culture) : char.ToLower(str[0], culture).ToString() + str.Substring(1);
    }

    /// <summary>
    /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
    /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="invariantCulture">Invariant culture</param>
    public static string ToSentenceCase(this string str, bool invariantCulture = false) =>
        string.IsNullOrWhiteSpace(str)
            ? str
            : Regex.Replace(str, "[a-z][A-Z]", (MatchEvaluator)(m =>
            {
                char ch = m.Value[0];
                string str1 = ch.ToString();
                ch = invariantCulture ? char.ToLowerInvariant(m.Value[1]) : char.ToLower(m.Value[1]);
                string str2 = ch.ToString();
                return str1 + " " + str2;
            }));

    /// <summary>
    /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
    /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="culture">An object that supplies culture-specific casing rules.</param>
    public static string ToSentenceCase(this string str, CultureInfo culture) => string.IsNullOrWhiteSpace(str)
        ? str
        : Regex.Replace(str, "[a-z][A-Z]", (MatchEvaluator)(m =>
        {
            char lower = m.Value[0];
            string str1 = lower.ToString();
            lower = char.ToLower(m.Value[1], culture);
            string str2 = lower.ToString();
            return str1 + " " + str2;
        }));

    /// <summary>Converts string to enum value.</summary>
    /// <typeparam name="T">Type of enum</typeparam>
    /// <param name="value">String value to convert</param>
    /// <returns>Returns enum object</returns>
    public static T ToEnum<T>(this string value) where T : struct => value != null
        ? (T)Enum.Parse(typeof(T), value)
        : throw new ArgumentNullException(nameof(value));

    /// <summary>Converts string to enum value.</summary>
    /// <typeparam name="T">Type of enum</typeparam>
    /// <param name="value">String value to convert</param>
    /// <param name="ignoreCase">Ignore case</param>
    /// <returns>Returns enum object</returns>
    public static T ToEnum<T>(this string value, bool ignoreCase) where T : struct => value != null
        ? (T)Enum.Parse(typeof(T), value, ignoreCase)
        : throw new ArgumentNullException(nameof(value));

    public static string ToMd5(this string str)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = md5.ComputeHash(bytes);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in hash)
                stringBuilder.Append(num.ToString("X2"));
            return stringBuilder.ToString();
        }
    }

    /// <summary>Converts camelCase string to PascalCase string.</summary>
    /// <param name="str">String to convert</param>
    /// <param name="invariantCulture">Invariant culture</param>
    /// <returns>PascalCase of the string</returns>
    public static string ToPascalCase(this string str, bool invariantCulture = true)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        if (str.Length != 1)
            return (invariantCulture ? char.ToUpperInvariant(str[0]) : char.ToUpper(str[0])).ToString() +
                   str.Substring(1);
        return !invariantCulture ? str.ToUpper() : str.ToUpperInvariant();
    }

    /// <summary>
    /// Converts camelCase string to PascalCase string in specified culture.
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <param name="culture">An object that supplies culture-specific casing rules</param>
    /// <returns>PascalCase of the string</returns>
    public static string ToPascalCase(this string str, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        return str.Length == 1 ? str.ToUpper(culture) : char.ToUpper(str[0], culture).ToString() + str.Substring(1);
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="str" /> is null</exception>
    public static string Truncate(this string str, int maxLength)
    {
        if (str == null)
            return (string)null;
        return str.Length <= maxLength ? str : str.Left(maxLength);
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// It adds a "..." postfix to end of the string if it's truncated.
    /// Returning string can not be longer than maxLength.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="str" /> is null</exception>
    public static string TruncateWithPostfix(this string str, int maxLength) =>
        str.TruncateWithPostfix(maxLength, "...");

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// It adds given <paramref name="postfix" /> to end of the string if it's truncated.
    /// Returning string can not be longer than maxLength.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="str" /> is null</exception>
    public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
    {
        if (str == null)
            return (string)null;
        if (string.IsNullOrEmpty(str) || maxLength == 0)
            return string.Empty;
        if (str.Length <= maxLength)
            return str;
        return maxLength <= postfix.Length ? postfix.Left(maxLength) : str.Left(maxLength - postfix.Length) + postfix;
    }
}