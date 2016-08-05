using System.Text.RegularExpressions;
using System.Collections.Generic;

public class StringHelper
{
    // Converts an array (or sub-array) to a string.
    /*public static string ArrayToString(string[] array, string seperator = " ", int start = 0, int end = -1)
    {
        string[] subarray = ArrayHelper.SubArray<string>(array, start, end);
        return string.Join(seperator, subarray);
    }*/

    public static string ArrayToString<T>(T[] array, string seperator = " ", int start = 0, int end = -1)
    {
        T[] subarray = ArrayHelper.SubArray<T>(array, start, end);
        return string.Join(seperator, TypeHelper.TypeArrayToStringArray<T>(subarray));
    }

    public static string[] GetSubStrings(string input, string start = "\"", string end = "\"")
    {
        Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
        MatchCollection matches = r.Matches(input);

        string[] strings = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
            strings[i] = matches[i].Groups[1].Value;

        return strings;
    }

    public static string ReplaceFirst(string text, string search, string replacement)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replacement + text.Substring(pos + search.Length);
    }
}
