using UnityEngine;

public class TabletPageInitialise : MonoBehaviour
{
    public TabletPage initialPage = null;
    public TabletPage[] otherPages = null;

    private KMSelectable _selectable = null;

    private void Start()
    {
        _selectable = GetComponentInParent<KMSelectable>();
        _selectable.Parent.OnInteract += OnInteract;

        Reset();
    }

    private void Destroy()
    {
        _selectable.Parent.OnInteract -= OnInteract;
    }

    private bool OnInteract()
    {
        Reset();
        return true;
    }

    public void Reset()
    {
        initialPage.gameObject.SetActive(true);

        foreach (TabletPage otherPage in otherPages)
        {
            otherPage.gameObject.SetActive(false);
        }
    }
}
