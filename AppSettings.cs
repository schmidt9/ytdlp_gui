using Microsoft.Maui.Storage;

namespace ytdlp_gui;
public static class AppSettings
{
    private const string URLKey = "url_key";
    private const string SavePathKey = "save_path_key";

    public static string URL
    {
        get => Preferences.Default.Get(URLKey, "");
        set => Preferences.Default.Set(URLKey, value);
    }

    public static string SavePath
    {
        get => Preferences.Default.Get(SavePathKey, "");
        set => Preferences.Default.Set(SavePathKey, value);
    }
}
