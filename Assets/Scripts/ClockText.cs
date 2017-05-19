using System;
using System.Collections;
using UnityEngine;

public class ClockText : MonoBehaviour
{
    private TextMesh _textMesh = null;

    private void Awake()
    {
        _textMesh = GetComponent<TextMesh>();
    }

    private void Update()
    {
        StartCoroutine(UpdatePerSecond());
    }

    private IEnumerator UpdatePerSecond()
    {
        while (true)
        {
            _textMesh.text = DateTime.Now.ToString("HH:mm");
            yield return new WaitForSeconds(1.0f);
        }
    }
}

