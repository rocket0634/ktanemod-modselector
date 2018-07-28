using UnityEngine;

public class ProfileMainPage : MonoBehaviour
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

