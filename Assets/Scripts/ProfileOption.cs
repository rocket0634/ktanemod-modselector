using UnityEngine;
using UnityEngine.UI;

public class ProfileOption : MonoBehaviour
{
    public Text text;

    public string ProfileName
    {
        get
        {
            return text.text;
        }
        set
        {
            text.text = value;
        }
    }
}
