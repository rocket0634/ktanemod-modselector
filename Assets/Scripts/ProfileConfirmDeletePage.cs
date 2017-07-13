using UnityEngine;

public class ProfileConfirmDeletePage : MonoBehaviour
{
    public Profile profile = null;

    public TextMesh messageText = null;
    public TabletPage returnPage = null;

    private TabletPage _tabletPage = null;

    private void Awake()
    {
        _tabletPage = GetComponent<TabletPage>();
    }

    private void OnEnable()
    {
        if (_tabletPage != null && _tabletPage.header != null)
        {
            _tabletPage.header.text = string.Format("<b>{0}</b>\n<size=16>Confirm Delete Profile</size>", profile == null ? "**NULL**" : profile.FriendlyName);
        }

        if (messageText != null)
        {
            messageText.text = string.Format("Delete <b>{0}</b>?", profile == null ? "NULL" : profile.FriendlyName);
        }
    }

    public void Yes()
    {
        profile.Delete();
        InputHelper.InvokeCancel();
    }

    public void No()
    {
        InputHelper.InvokeCancel();
    }    
}
