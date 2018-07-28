using UnityEngine;

public class MainPage : MonoBehaviour
{
    public void ReloadProfiles()
    {
        Profile.ReloadActiveConfiguration();
        Toast.QueueMessage("Profiles reloaded.");
    }

    public void OpenModProfilesFolder()
    {
        PlatformActions.ShowPath(Profile.ProfileDirectory);
    }
}

