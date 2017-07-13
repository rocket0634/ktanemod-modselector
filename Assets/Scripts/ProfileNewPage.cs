using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProfileNewPage : MonoBehaviour
{
    public TextMesh newNameText = null;
    public TextMesh[] toggleableLetters = null;
    public KMSelectable[] selectablesOnCreate = null;

    public ProfileSettingsPage profileSettingsPage = null;

    private string Filename
    {
        get
        {
            return newNameText.text.Substring(0, newNameText.text.Length - 1);
        }
    }

    private char Caret
    {
        get
        {
            return newNameText.text[newNameText.text.Length - 1];
        }
        set
        {
            newNameText.text = string.Format("{0}{1}", Filename, value);
        }
    }

    private bool _capsOn = false;
    private Coroutine _caretFlashCoroutine = null;

    private static readonly char[] INVALID_CHARACTERS = Path.GetInvalidFileNameChars();

    private void OnEnable()
    {
        if (newNameText != null)
        {
            newNameText.text = " ";
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
        newNameText.text = Filename + text + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void AddCharacter(string text)
    {
        newNameText.text = Filename + (_capsOn ? text.ToUpper() : text.ToLower()) + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void DeleteCharacter()
    {
        if (newNameText.text.Length == 1)
        {
            return;
        }

        newNameText.text = Filename.Substring(0, Filename.Length - 1) + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void Create()
    {
        if (string.IsNullOrEmpty(Filename) || !Profile.CanCreateProfile(Filename))
        {
            return;
        }

        InputHelper.InvokeCancel();
        foreach(KMSelectable selectable in selectablesOnCreate)
        {
            selectable.InvokeSelect(false, true);
        }

        InputInvoker.Instance.Enqueue(delegate ()
        {
            profileSettingsPage.profile = Profile.CreateProfile(Filename);
            profileSettingsPage.OnEnable();
        });
    }

    private void UpdateLetters()
    {
        foreach (TextMesh textMesh in toggleableLetters)
        {
            textMesh.text = _capsOn ? textMesh.text.ToUpper() : textMesh.text.ToLower();
        }
    }
}
