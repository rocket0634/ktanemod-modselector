using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class PlatformActions
{
    public static void ShowPath(string path)
    {
        //If trying to open a relative path, make it absolute
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Path.GetFullPath("."), path);
        }

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                Application.OpenURL(string.Format("file://{0}", path));
                break;

            case RuntimePlatform.OSXPlayer:
                Process.Start("open", string.Format("-n -R \"{0}\"", path));
                break;

            default:
                Application.OpenURL(string.Format("file://{0}", path));
                break;
        }
    }
}
