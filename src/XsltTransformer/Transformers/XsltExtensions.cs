using System.Globalization;
using System.Reflection;
using Genocs.Fonet.XsltTransformer.Configurations;

namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// XsltExtensions is a container of helper methods used to extend the functionality of style sheets.
/// This class is used as an extension object in the XsltArgumentList of the XmlTransformationManager.
/// </summary>
public class XsltExtensions
{
    /// <summary>
    /// Returns the physical file path that corresponds to the specified virtual path on the Web server.
    /// </summary>
    /// <param name="filePath">The virtual path.</param>
    public static string MapPath(string filePath)
    {
        if (filePath.StartsWith('.'))
        {
            // Resolve assets relative to the executable so the app works from any working directory.
            string baseDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"XsltExtensions.MapPath: baseDirectory={baseDirectory}, filePath={filePath}");
            return Path.Combine(baseDirectory, filePath);
        }

        if (filePath.StartsWith("http"))
        {
            return filePath;
        }

        if (!Path.IsPathRooted(filePath))
        {
            return Path.GetFullPath(filePath);
        }

        return filePath;
    }

    /// <summary>
    /// Returns the physical file path that corresponds to the specified relative path on the Web server.
    /// </summary>
    /// <returns>The assembly version as a string.</returns>
    public static string GetAssemblyVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "Unknown";
    }

    public static string? GetExecutingFolder(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        // Resolve assets relative to the executable so the app works from any working directory.
        string baseDirectory = AppContext.BaseDirectory;
        return Path.Combine(baseDirectory, relativePath);
    }

    /// <summary>
    /// Inserts a white space after every 'breakLen' number of chars.
    /// <paragraph>
    ///
    /// Example:
    /// longWord: 'supercalifragilistichespiralitoso'
    /// breakLen: 8
    ///
    /// returns: 'supercal ifragili stichesp iralitos o'.
    /// </paragraph>
    /// </summary>
    /// <param name="longWord">The word to break.</param>
    /// <param name="breakLen">The maximum length of each sub-word within the 'longWord'.</param>
    private static string BreakWords(string longWord, int breakLen)
    {
        string[] words = longWord.Split(' ');

        var breakedWords = new List<string>();

        foreach (string word in words)
            BreakWord(word, breakLen, breakedWords);

        return string.Join(" ", breakedWords.ToArray());
    }

    private static void BreakWord(string longWord, int breakLen, List<string> words)
    {
        if (string.IsNullOrWhiteSpace(longWord))
            return;

        if (longWord.Length > breakLen)
        {
            words.Add(longWord.Substring(0, breakLen));
            BreakWord(longWord.Substring(breakLen), breakLen, words);
        }
        else
        {
            words.Add(longWord);
        }
    }

    /// <summary>
    /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
    /// </summary>
    private static string Substring(string s, int from, int len)
    {
        if (s.Length > len)
            return s.Substring(from, len);

        return s;
    }

    /// <summary>
    /// Formats date string according to the date format string defined on web.config.
    /// </summary>
    private static string FormatDateTime(string dateAsString)
    {
        return FormatDateTime(dateAsString, GlobalSettings.DefaultDateFormat);
    }

    /// <summary>
    /// Formats input date string with the specified date format.
    /// </summary>
    private static string FormatDateTime(string dateAsString, string format)
    {
        try
        {
            var date = DateTime.Parse(dateAsString);
            return date.ToString(format);
        }
        catch
        {
            return dateAsString;
        }
    }

    /// <summary>
    /// Parses decimal string according to the current thread culture.
    /// </summary>
    private static bool ParseDecimalString(string decimalValue, out decimal d)
    {
        return decimal.TryParse(decimalValue.Replace(" ", string.Empty),
                                NumberStyles.Integer | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowParentheses,
                                CultureInfo.InvariantCulture,
                                out d);
    }

    /// <summary>
    /// Parses integer string according to the current thread culture.
    /// </summary>
    private static bool ParseIntString(string intValue, out int i)
    {
        return int.TryParse(intValue.Replace(" ", string.Empty),
                            NumberStyles.Integer | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowParentheses,
                            CultureInfo.InvariantCulture,
                            out i);
    }

    /// <summary>
    /// Formats decimal value according to the culture defined on web.config.
    /// </summary>
    private string FormatDecimal(string decimalValue)
    {
        return FormatDecimal(decimalValue, GlobalSettings.DefaultCulture);
    }

    /// <summary>
    /// Formats decimal value according to the culture name, see <see cref="CultureInfo"/>.
    /// </summary>
    private static string FormatDecimal(string decimalValue, string culture)
    {
        string formattedAmount = decimalValue;

        if (ParseDecimalString(decimalValue, out decimal y))
        {
            var cultureInfo = new CultureInfo(culture);
            cultureInfo.NumberFormat.NumberDecimalDigits = 2;
            formattedAmount = y.ToString("N", cultureInfo);
        }

        return formattedAmount;
    }

    /// <summary>
    /// Formats integer value according to the culture defined on web.config.
    /// </summary>
    private static string FormatInteger(string intValue)
    {
        return FormatDecimal(intValue, GlobalSettings.DefaultCulture);
    }

    /// <summary>
    /// Formats integer value according to the culture name, see <see cref="CultureInfo"/>.
    /// </summary>
    private static string FormatInteger(string intValue, string culture)
    {
        string formattedAmount = intValue;

        if (ParseIntString(intValue, out int x))
        {
            var cultureInfo = new CultureInfo(culture);
            cultureInfo.NumberFormat.NumberDecimalDigits = 0;
            formattedAmount = x.ToString("N", cultureInfo);
        }

        return formattedAmount;
    }

    /// <summary>
    /// Tests if the given number is negative.
    /// </summary>
    private static bool IsNegativeNumber(string number)
    {
        if (ParseDecimalString(number, out decimal d))
            return d < 0;

        return false;
    }
}
