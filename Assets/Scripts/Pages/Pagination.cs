using UnityEngine;

public abstract class Pagination<ValueType, ElementType> : MonoBehaviour where ElementType : MonoBehaviour
{
    public UIElement PreviousButton = null;
    public UIElement NextButton = null;

    public int ElementsPerRow = 3;

    protected Page Page
    {
        get;
        private set;
    }

    protected abstract ValueType[] ValueCollection
    {
        get;
    }

    protected abstract ElementType[] ElementCollection
    {
        get;
    }

    protected int PageIndex
    {
        get;
        private set;
    }

    protected int TotalPageCount
    {
        get
        {
            return ((ValueCollection.Length - 1) / ElementCollection.Length) + 1;
        }
    }

    protected string PageName
    {
        get
        {
            return string.Format("Page {0} of {1}", PageIndex + 1, TotalPageCount);
        }
    }

    protected int ValueOffset
    {
        get
        {
            return (PageIndex * ElementCollection.Length) + (_pageOffset * ElementsPerRow);
        }
    }

    private int RowCount
    {
        get
        {
            return ElementCollection.Length / ElementsPerRow;
        }
    }

    private int TotalRowCount
    {
        get
        {
            return (ValueCollection.Length + 2) / ElementsPerRow;
        }
    }

    private bool PreviousEnabled
    {
        get
        {
            return PageIndex > 0;
        }
    }

    private bool NextEnabled
    {
        get
        {
            return PageIndex < TotalPageCount - 1;
        }
    }

    private int _pageOffset = 0;

    protected virtual void Awake()
    {
        Page = GetComponent<Page>();

        PreviousButton.InteractAction.AddListener(PreviousPage);
        NextButton.InteractAction.AddListener(NextPage);
    }

    protected virtual void OnDestroy()
    {
        PreviousButton.InteractAction.RemoveListener(PreviousPage);
        NextButton.InteractAction.RemoveListener(NextPage);
    }

    protected virtual void Update()
    {
        int scrollDelta = Mathf.RoundToInt(Input.GetAxis("Mouse ScrollWheel") * -8);
        if (scrollDelta != 0)
        {
            SetPage(PageIndex, _pageOffset + scrollDelta);
        }
    }

    public virtual void SetPage(int pageIndex, int pageOffset = 0)
    {
        while (pageOffset < 0)
        {
            pageIndex--;
            pageOffset += RowCount;
        }

        if (pageIndex < 0)
        {
            pageIndex = 0;
            pageOffset = 0;
        }
        else
        {
            pageIndex += pageOffset / RowCount;

            if (pageIndex > TotalPageCount - 1)
            {
                pageOffset = TotalRowCount - 1;
            }
            else
            {
                pageOffset %= RowCount;
            }
        }

        PageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);
        _pageOffset = Mathf.Clamp(pageOffset, 0, Mathf.Max(0, TotalRowCount - (PageIndex * RowCount) - 1));

        for (int elementIndex = 0; elementIndex < ElementCollection.Length; ++elementIndex)
        {
            ElementType element = ElementCollection[elementIndex];
            int valueIndex = ValueOffset + elementIndex;

            if (valueIndex < ValueCollection.Length)
            {
                element.gameObject.SetActive(true);
                SetElement(ValueCollection[valueIndex], element);
            }
            else
            {
                element.gameObject.SetActive(false);
            }
        }

        if (PreviousButton != null)
        {
            PreviousButton.CanSelect = PreviousEnabled;
        }

        if (NextButton != null)
        {
            NextButton.CanSelect = NextEnabled;
        }
    }

    protected abstract void SetElement(ValueType value, ElementType element);

    private void NextPage()
    {
        SetPage(PageIndex + 1);
    }

    private void PreviousPage()
    {
        SetPage(PageIndex - 1);
    }
}
