using System;

public static class TypeHelper
{
    // Converts array of strings to array of type.
    public static T[] StringArrayToTypeArray<T>(string[] values)
    {
        T[] converted = new T[values.Length];

        for (int i = 0; i < values.Length; i++)
            converted[i] = StringToType<T>(values[i]);

        return converted;
    }

    public static string[] TypeArrayToStringArray<T>(T[] values)
    {
        string[] converted = new string[values.Length];

        for (int i = 0; i < values.Length; i++)
            converted[i] = values[i].ToString();

        return converted;
    }

    // Converts a string to a type.
    public static T StringToType<T>(string value)
    {
        // Special bool conversion.
        if (typeof(T) == typeof(bool))
            return (T)Convert.ChangeType(StringToBool(value), typeof(T));

        return (T)Convert.ChangeType(value, typeof(T));
    }

    // Converts a string to boolean.
    public static bool StringToBool(string toConvert)
    {
        switch (toConvert.ToLower())
        {
            case "0":
            case "true":
            case "t":
                return true;

            default:
                return false;
        }
    }
}
