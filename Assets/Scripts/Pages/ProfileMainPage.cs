using UnityEngine;

public class ProfileMainPage : MonoBehaviour
{
    public void ReloadProfiles()
    {
        ProfileManager.ReloadActiveConfiguration();
        Toast.QueueMessage("Profiles reloaded.");
    }

    public void OpenModProfilesFolder()
    {
        PlatformActions.ShowPath(Profile.ProfileDirectory);
    }
}

