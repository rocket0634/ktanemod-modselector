using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class MainPage : MonoBehaviour
{
    public void OpenModProfilesFolder()
    {
        Application.OpenURL(string.Format("file://{0}", Profile.ProfileDirectory));
    }
}

