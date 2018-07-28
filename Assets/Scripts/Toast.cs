using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toast : MonoBehaviour
{
    public MeshRenderer backgroundRenderer = null;
    public TextMesh textRenderer = null;

    public float padding = 0.0f;
    public float fadeDuration = 0.0f;
    public float holdDuration = 0.0f;

    private int _colorPropertyID = 0;
    private MaterialPropertyBlock _backgroundBlock = null;
    private Color _backgroundColor = Color.white;
    private Color _textColor = Color.white;

    private static Toast _instance = null;
    private static readonly Queue<string> _pendingMessages = new Queue<string>();
    private static Coroutine _messageCoroutine = null;

    public static void QueueMessage(string message)
    {
        if (_instance == null)
        {
            return;
        }

        _pendingMessages.Enqueue(message);

        if (_messageCoroutine == null)
        {
            _messageCoroutine = _instance.StartCoroutine(_instance.ShowMessagesCoroutine());
        }
    }

    private void Awake()
    {
        _backgroundColor = backgroundRenderer.material.color;
        _backgroundColor.a = 0.0f;
        _textColor = textRenderer.color;
        _textColor.a = 0.0f;

        _colorPropertyID = Shader.PropertyToID("_Color");

        _backgroundBlock = new MaterialPropertyBlock();

        SetAlpha(0.0f);
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private IEnumerator ShowMessagesCoroutine()
    {
        while (_pendingMessages.Count > 0)
        {
            string message = _pendingMessages.Dequeue();
            SetText(message);
            SetAlpha(0.0f);

            float alpha = 0.0f;
            while (alpha < 1.0f)
            {
                yield return null;
                alpha = Mathf.Clamp01(alpha + (Time.deltaTime / fadeDuration));
                SetAlpha(alpha);
            }

            yield return new WaitForSeconds(holdDuration);

            while (alpha > 0.0f)
            {
                yield return null;
                alpha = Mathf.Clamp01(alpha - (Time.deltaTime / fadeDuration));
                SetAlpha(alpha);
            }
        }

        _messageCoroutine = null;
    }

    private void SetText(string text)
    {
        textRenderer.text = text;

        float width = 0.0f;
        foreach (char character in text)
        {
            CharacterInfo characterInfo;
            if (textRenderer.font.GetCharacterInfo(character, out characterInfo, textRenderer.fontSize, textRenderer.fontStyle))
            {
                width += characterInfo.advance;
            }
        }

        width *= 2.0f;
        width += padding;
        Vector3 localScale = backgroundRenderer.transform.localScale;
        localScale.x = width;
        backgroundRenderer.transform.localScale = localScale;
    }

    private void SetAlpha(float alpha)
    {
        _backgroundColor.a = alpha;
        _textColor.a = alpha;

        _backgroundBlock.SetColor(_colorPropertyID, _backgroundColor);
        backgroundRenderer.SetPropertyBlock(_backgroundBlock);

        textRenderer.color = _textColor;
    }
}
