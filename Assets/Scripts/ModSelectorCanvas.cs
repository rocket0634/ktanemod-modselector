using UnityEngine;

public class ModSelectorCanvas : MonoBehaviour
{
    public ModSelectorWindow modSelectorWindow = null;
    public SetupScreenOverlay modSelectorSetupOverlay = null;

    private KMGamepad _gamepad = null;

    public void OnEditModSelection()
    {
        if (modSelectorSetupOverlay.gameObject.activeInHierarchy && !modSelectorWindow.gameObject.activeInHierarchy)
        {
            modSelectorWindow.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        GetComponent<KMGameInfo>().OnStateChange += OnStateChange;
        _gamepad = GetComponent<KMGamepad>();
    }

    private void Update()
    {
        if (_gamepad.GetButtonDown(KMGamepad.ButtonEnum.X))
        {
            OnEditModSelection();
        }
    }

    private void OnStateChange(KMGameInfo.State state)
    {
        switch (state)
        {
            case KMGameInfo.State.Transitioning:
                modSelectorSetupOverlay.gameObject.SetActive(false);
                break;
            case KMGameInfo.State.Setup:
                modSelectorSetupOverlay.gameObject.SetActive(true);
                break;
            default:
                break;
        }

        modSelectorWindow.gameObject.SetActive(false);
    }
}
