using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class LogHandler
{
    public static string Path = Application.persistentDataPath + "/log.txt";

    public static void WriteText(string text)
    {
        using (StreamWriter sw = new StreamWriter(Path, true, System.Text.Encoding.Default))
        {
            sw.WriteLine(text);
        }
    }

    public static void Initialise()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }


}
