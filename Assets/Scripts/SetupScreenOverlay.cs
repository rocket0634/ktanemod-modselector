using UnityEngine;

public class SetupScreenOverlay : MonoBehaviour
{
    public void OnFocusEnter()
    {
        InputInterceptor.DisableControls();
    }

    public void OnFocusExit()
    {
        InputInterceptor.EnableControls();
    }
}
