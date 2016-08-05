using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

// TODO
// The two loops could be combined.
// Improve error reporting.
// optimize CommandSplit.

public class MethodHelper
{
    // Objects to be returned in StringsToArguments.
    public static Dictionary<Type, object> Objects = new Dictionary<Type, object>() {
        { typeof(int[]), new int[] { } },
        { typeof(float[]), new float[] { } },
        { typeof(string[]), new string[] { } },
        { typeof(bool[]), new bool[] { } }
    };

    // Calls a method. Converts data to relevant objects.
    public static void Invoke(Component component, MethodInfo method, string[] data)
    {
        Invoke(component, method, StringsToArguments(method, data));
    }

    // 
    public static void Invoke(Component component, MethodInfo method, object[] data)
    {
        try
        {
            method.Invoke(component, data);
        }
        catch (Exception e)
        {
            string newPara = "";
            foreach (object o in data)
                newPara += "(" + o.ToString() + ":" + o.GetType() + ") ";

            string defPara = "";
            foreach (ParameterInfo pi in method.GetParameters())
                defPara += "(" + pi.Name + ":" + pi.ParameterType + ") ";

            Debug.Log("(" + newPara + ") couldn't be converted to (" + defPara + ")\n" + e.Message);
        }
    }

    // Converts an array of string data into arguments for a given method.
    // Attempts to be as flexible as possible, but still not perfect.
    public static object[] StringsToArguments(MethodInfo method, string[] data)
    {
        // Default parameters.
        ParameterInfo[] parameterInfo = method.GetParameters();

        // New parameters.
        object[] parameters = new object[parameterInfo.Length];

        // Set everything to default to be safe.
        for (int i = 0; i < parameterInfo.Length; i++)
        {
            ParameterInfo pi = parameterInfo[i];
            Type t = pi.ParameterType;

            // Invoke doesn't like getting null parameters, so try to use an object from Objects.
            if (pi.DefaultValue == null)
                parameters[i] = (Objects.ContainsKey(t)) ? Objects[t] : null;
            // Default.
            else
                parameters[i] = pi.DefaultValue;
        }

        for (int i = 0; i < data.Length; i++)
        {
            ParameterInfo pi = parameterInfo[i];
            Type t = pi.ParameterType;

            // Do we need an array?.
            if (IsArray(t))
            {
                string[] subarray;
                bool allDone = false;

                // Is the current string an entire array?
                if (data[i].Contains(","))
                    subarray = data[i].Split(',');
                else if (data[i].Contains(" "))
                    subarray = data[i].Split(' ');
                // If not, use all the rest of the data.
                else
                {
                    subarray = ArrayHelper.SubArray(data, i);
                    allDone = true;
                }

                // Convert the strings to their requested type.
                if (t == typeof(string[])) parameters[i] = subarray;
                else if (t == typeof(float[])) parameters[i] = TypeHelper.StringArrayToTypeArray<float>(subarray);
                else if (t == typeof(int[])) parameters[i] = TypeHelper.StringArrayToTypeArray<int>(subarray);
                else if (t == typeof(bool[])) parameters[i] = TypeHelper.StringArrayToTypeArray<bool>(subarray);

                if (allDone)
                    break;
            }
            // Boolean.
            else if (t == typeof(bool))
                parameters[i] = TypeHelper.StringToBool(data[i]);
            // String, int, float.
            else
                parameters[i] = Convert.ChangeType(data[i], t);
        }

        return parameters;
    }

    public static string[] CommandSplit(string command, string splitOn = "", string groupStart = "\"", string groupEnd = "\"")
    {
        // Extract grouped parts.
        string[] strings = StringHelper.GetSubStrings(command, groupStart, groupEnd);

        // Temporarilly replace their spaces with ____.
        foreach (string str in strings)
        {
            string search = groupStart + str + groupEnd;
            string replacement = string.Join("____", str.Split(new string[] { splitOn }, StringSplitOptions.None));
            command = StringHelper.ReplaceFirst(command, search, replacement);
        }

        // Now split final command on remaining spaces.
        string[] parts = command.Split(new string[] { splitOn }, StringSplitOptions.None);

        // Now go back and replace the ____ with spaces.
        for (int i = 0; i < parts.Length; i++)
            parts[i] = string.Join(" ", parts[i].Split(new string[] { "____" }, StringSplitOptions.None));

        // Whew.
        return parts;
    }

    static bool IsArray(Type t)
    {
        if (t == typeof(string[])) return true;
        if (t == typeof(int[])) return true;
        if (t == typeof(float[])) return true;
        if (t == typeof(bool[])) return true;

        return false;
    }
}
