using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TextShrink : MonoBehaviour
{
	public float MaxWidth = 1.0f;

	private Transform _transform = null;
	private TextMesh _textMesh = null;
	private string _text = null;

	private void Awake()
	{
		_transform = transform;
		_textMesh = GetComponent<TextMesh>();
	}

	private void LateUpdate()
	{
		if (_textMesh.text == _text)
		{
			return;
		}

		_text = _textMesh.text;

		CharacterInfo info;
		float width = 0.0f;

		foreach (char character in _textMesh.text)
		{
			_textMesh.font.GetCharacterInfo(character, out info, _textMesh.fontSize, _textMesh.fontStyle);
			width += info.advance;
		}

		width *= _textMesh.characterSize * 0.1f;

		float scaling = Mathf.Min(MaxWidth / width, 1.0f);
		_transform.localScale = new Vector3(scaling, 1.0f, 1.0f);
	}
}
