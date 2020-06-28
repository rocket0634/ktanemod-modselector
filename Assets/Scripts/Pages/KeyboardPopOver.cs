using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;

public class KeyboardPopOver : MonoBehaviour
{
    public TextMesh Text = null;
    public UIElement[] Letters = null;

    private Page _page = null;

    public string CurrentText
    {
        get
        {
            return Text.text.Substring(0, Text.text.Length - 1);
        }
        set
        {
            Text.text = string.Format("{0} ", value);
        }
    }

    private char Caret
    {
        get
        {
            return Text.text[Text.text.Length - 1];
        }
        set
        {
            Text.text = string.Format("{0}{1}", CurrentText, value);
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
        if (Text != null && Text.text.Length == 0)
        {
            Text.text = " ";
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
            _page.GoBack();
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
        Text.text = CurrentText + text + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void AddCharacter(string text)
    {
        Text.text = CurrentText + (_capsOn ? text.ToUpper() : text.ToLower()) + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    public void DeleteCharacter()
    {
        if (Text.text.Length == 1)
        {
            return;
        }

        Text.text = CurrentText.Substring(0, CurrentText.Length - 1) + Caret;

        if (_capsOn)
        {
            _capsOn = false;
            UpdateLetters();
        }
    }

    private void UpdateLetters()
    {
        foreach (UIElement letter in Letters)
        {
            letter.Text = _capsOn ? letter.Text.ToUpper() : letter.Text.ToLower();
        }
    }
}
