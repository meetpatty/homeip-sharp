using System;
using System.IO;
using System.Xml.Serialization;

public static class AppSettings {

    [Serializable]
    public class AppSettingsXml {
        public string BackgroundColor = "#000000";
        public ushort BackgroundAlpha = 32767;
        public string ForegroundColor = "#FFFFFF";
        public ushort ForegroundAlpha = 255;
        public string Font = "Sans 16";
        public bool Autosize = true;
    }


    private static string settingsPath;
    private static AppSettingsXml settingsXml = new AppSettingsXml();
    private static bool loaded = false;

    public static string BackgroundColor
    {
        get { return settingsXml.BackgroundColor; }
        set { settingsXml.BackgroundColor = value; }
    }
    public static ushort BackgroundAlpha
    {
        get { return settingsXml.BackgroundAlpha; }
        set { settingsXml.BackgroundAlpha = value; }
    }
    public static string ForegroundColor
    {
        get { return settingsXml.ForegroundColor; }
        set { settingsXml.ForegroundColor = value; }
    }
    public static ushort ForegroundAlpha {
        get { return settingsXml.ForegroundAlpha; }
        set { settingsXml.ForegroundAlpha = value; } }
    public static string Font
    {
        get { return settingsXml.Font; }
        set { settingsXml.Font = value; }
    }
    public static bool Autosize
    {
        get { return settingsXml.Autosize; }
        set { settingsXml.Autosize = value; }
    }

    public static void Load(string path)
    {
        if (loaded)
            return;

        settingsPath = path;

        if (File.Exists(path))
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettingsXml));
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    settingsXml = (AppSettingsXml)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load settings, using defaults. Error: " + ex.Message);
                settingsXml = new AppSettingsXml();
            }
        }

        loaded = true;
    }

    public static void Save()
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettingsXml));
            using (FileStream fs = new FileStream(settingsPath, FileMode.Create))
            {
                serializer.Serialize(fs, settingsXml);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to save settings.");
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}