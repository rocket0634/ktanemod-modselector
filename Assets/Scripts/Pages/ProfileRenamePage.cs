using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProfileRenamePage : MonoBehaviour
{
    public TextMesh NewNameText = null;
    public UIElement[] Letters = null;

    public Profile Profile = null;

    public ProfileSettingsPage SettingsPagePrefab = null;

    private Page _page = null;

    private string Filename
    {
        get
        {
            return NewNameText.text.Substring(0, NewNameText.text.Length - 1);
        }
    }

    private char Caret
    {
        get
        {
            return NewNameText.text[NewNameText.text.Length - 1];
        }
        set
        {
            NewNameText.text = string.Format("{0}{1}", Filename, value);
        }
    }

    private bool _capsOn = false;
    private Coroutine _caretFlashCoroutine = null;

    private static readonly char[] INVALID_CHARACTERS = Path.GetInvalidFileNameChars();

    private void Awake()
    {
        _page = GetComponent<Page>();

        foreach (UIElement letter in Letters)
        {
            UIElement localLetter = letter;
            localLetter.InteractAction.AddListener(delegate ()
            {
                AddCharacter(localLetter.Text);
            });
        }
    }

    private void OnEnable()
    {
        if (NewNameText != null)
        {
            NewNameText.text = Profile.Name + " ";
        }

        _capsOn = true;
        UpdateLetters();

        _caretFlashCoroutine = StartCoroutine(CaretFlash());
    }

    private void OnDisable()
    {
        if (_caretFlashCoroutine != null)
        {
            StopCoroutine(_caretFlashCoroutine);
            _caretFlashCoroutine = null;
        }
    }

    private void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                DeleteCharacter();
            }
            else if (!INVALID_CHARACTERS.Contains(c))
            {
                AddCharacterNoModify(c.ToString());
            }
        }

        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            ToogleCaps();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            _capsOn = true;
            UpdateLetters();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            _capsOn = false;
            UpdateLetters();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Apply();
        }
    }

    private IEnumerator CaretFlash()
    {
        while (true)
        {
            Caret = ' ';
            yield return new WaitForSeconds(0.4f);

            Caret = '|';
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void ToogleCaps()
    {
        _capsOn = !_capsOn;
        UpdateLetters();
    }

    public void AddCharacterNoModify(string text)
    {
        NewNameText.text = Filename + text + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void AddCharacter(string text)
    {
        NewNameText.text = Filename + (_capsOn ? text.ToUpper() : text.ToLower()) + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void DeleteCharacter()
    {
        if (NewNameText.text.Length == 1)
        {
            return;
        }

        NewNameText.text = Filename.Substring(0, Filename.Length - 1) + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void Apply()
    {
        if (string.IsNullOrEmpty(Filename) || !ProfileManager.CanCreateProfile(Filename))
        {
            Toast.QueueMessage(string.Format("Cannot rename profile to <i>'{0}'</i>.", Filename));
            return;
        }

        Profile.Rename(Filename);
        _page.GetPageWithComponent(SettingsPagePrefab).Profile = Profile;
        _page.GoBack();

        Toast.QueueMessage(string.Format("Renamed profile to <i>'{0}'</i>.", Filename));
    }

    private void UpdateLetters()
    {
        foreach (UIElement letter in Letters)
        {
            letter.Text = _capsOn ? letter.Text.ToUpper() : letter.Text.ToLower();
        }
    }
}
