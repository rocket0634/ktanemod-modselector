using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FilterButton : MonoBehaviour
{
    [Serializable]
    public class FilterTextChangeEvent : UnityEvent<string>
    {
    }

    public KeyboardPopOver KeyboardPopOver = null;
    public Color FilterInactiveUnselectedColor = Color.white;
    public Color FilterInactiveSelectedColor = Color.white;
    public Color FilterActiveUnselectedColor = Color.white;
    public Color FilterActiveSelectedColor = Color.white;

    public FilterTextChangeEvent FilterTextChange = new FilterTextChangeEvent();

    public string FilterText
    {
        get
        {
            return _filterText;
        }
        set
        {
            if (_filterText != value)
            {
                _filterText = value;
                FilterTextChange.Invoke(value);

                UpdateHighlight();
            }
        }
    }

    public bool FilterActive
    {
        get
        {
            return !string.IsNullOrEmpty(FilterText);
        }
    }

    private Page _thisPage = null;
    private UIElement _element = null;
    private BackgroundHighlight _highlight = null;
    private string _filterText = "";

    private void Awake()
    {
        _thisPage = GetComponentInParent<Page>();

        _element = GetComponent<UIElement>();
        _element.InteractAction.AddListener(ShowPopOver);

        _highlight = GetComponent<BackgroundHighlight>();
        UpdateHighlight();
    }

    private void OnDestroy()
    {
        _element.InteractAction.RemoveListener(ShowPopOver);
    }

    private void ShowPopOver()
    {
        KeyboardPopOver popOverInstance = _thisPage.GetPageWithComponent(KeyboardPopOver);
        popOverInstance.CurrentText = FilterText;
        _thisPage.ShowPopOver(KeyboardPopOver);

        StartCoroutine(PopOverCoroutine(popOverInstance));
    }

    private IEnumerator PopOverCoroutine(KeyboardPopOver popOverInstance)
    {
        while (popOverInstance.gameObject.activeInHierarchy)
        {
            if (FilterText != popOverInstance.CurrentText)
            {
                FilterText = popOverInstance.CurrentText;
            }

            yield return null;
        }
    }

    private void UpdateHighlight()
    {
        if (FilterActive)
        {
            _highlight.UnselectedColor = FilterActiveUnselectedColor;
            _highlight.SelectedColor = FilterActiveSelectedColor;
        }
        else
        {
            _highlight.UnselectedColor = FilterInactiveUnselectedColor;
            _highlight.SelectedColor = FilterInactiveSelectedColor;
        }
    }
}
