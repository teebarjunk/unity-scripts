using System;
using System.IO;
using System.Collections.Generic;

public static class FileHelper
{
    public static List<string> LoadFile(string path, bool ignoreBlankLines = true)
    {
        if (!File.Exists(path))
            return new List<string>();

        //    File.CreateText(path).Close();

        StreamReader file = File.OpenText(path);
        string line = file.ReadLine();
        List<string> lines = new List<string>();

        while (line != null)
        {
            if (ignoreBlankLines && line.Trim() == "")
            {
                // ignore
            }
            else
                lines.Add(line);

            line = file.ReadLine();
        }
        file.Close();

        return lines;
    }
}
