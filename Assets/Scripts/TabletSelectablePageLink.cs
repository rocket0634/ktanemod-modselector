using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class TabletSelectablePageLink : MonoBehaviour
{
    public TabletPage thisPage = null;
    public TabletPage linkPage = null;

    private KMSelectable _selectable = null;
    private bool _cancelConfigured = false;

    private void Awake()
    {
        _selectable = GetComponent<KMSelectable>();
        _selectable.OnInteractEnded += OnInteractEnded;
    }

    private void OnDestroy()
    {
        if (_selectable != null)
        {
            _selectable.OnInteractEnded -= OnInteractEnded;

            if (_selectable.Children != null)
            {
                foreach (KMSelectable childSelectable in _selectable.Children)
                {
                    if (childSelectable != null)
                    {
                        childSelectable.OnCancel -= OnCancel;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (_selectable != null && _selectable.Children != null && !_cancelConfigured)
        {
            _cancelConfigured = true;
            foreach (KMSelectable childSelectable in _selectable.Children)
            {
                if (childSelectable != null)
                {
                    childSelectable.OnCancel += OnCancel;
                }
            }
        }
    }

    private void OnInteractEnded()
    {
        if (thisPage != null)
        {
            thisPage.gameObject.SetActive(false);
        }

        if (linkPage != null)
        {
            linkPage.gameObject.SetActive(true);
            if (linkPage.GridSelectables != null && linkPage.GridSelectables.Length > 0)
            {
                _selectable.UpdateChildren(linkPage.GridSelectables[0].GetComponent<KMSelectable>());
            }
        }
    }

    private bool OnCancel()
    {
        if (thisPage != null)
        {
            thisPage.gameObject.SetActive(true);
        }

        if (linkPage != null)
        {
            linkPage.gameObject.SetActive(false);
        }

        return true;
    }
}
