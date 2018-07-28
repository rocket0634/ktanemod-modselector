using UnityEngine;

public class ProfileConfirmDeletePage : MonoBehaviour
{
    public Profile Profile = null;

    public TextMesh QuestionText = null;

    private Page _page = null;

    private void Awake()
    {
        _page = GetComponent<Page>();
    }

    private void OnEnable()
    {
        _page.HeaderText = string.Format("<b>{0}</b>\n<size=16>Confirm Delete Profile</size>", Profile == null ? "**NULL**" : Profile.FriendlyName);

        if (QuestionText != null)
        {
            QuestionText.text = string.Format("Delete <b>{0}</b>?", Profile == null ? "NULL" : Profile.FriendlyName);
        }
    }

    public void Yes()
    {
        _page.GoBack();
        _page.GoBack();

        Profile.Delete();

        _page.GoBack();
    }

    public void No()
    {
        _page.GoBack();
    }
}
