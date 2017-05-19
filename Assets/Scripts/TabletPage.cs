using System.Linq;
using UnityEngine;

public class TabletPage : MonoBehaviour
{
    public TextMesh header = null;
    public KMSelectable[] topSelectables = null;

    public TabletSelectable[] GridSelectables
    {
        get
        {
            return GetComponentsInChildren<TabletSelectable>().Where((x) => topSelectables.All((y) => y.GetComponent<TabletSelectable>() != x)).ToArray();
        }
    }

    public int gridSelectableColumnCount = 1;

    public void UpdatePage()
    {
        KMSelectable thisSelectable = GetComponent<KMSelectable>();

        KMSelectable[] gridSelectables = GetComponentsInChildren<KMSelectable>().Where((x) => !topSelectables.Contains(x) && x != thisSelectable).ToArray();

        int trueColumnCount = gridSelectableColumnCount + topSelectables.Length;
        int trueRowCount = ((gridSelectables.Length - 1) / gridSelectableColumnCount) + 2;

        thisSelectable.ChildRowLength = trueColumnCount;

        thisSelectable.Children = new KMSelectable[trueColumnCount * trueRowCount];
        for (int topSelectableIndex = 0; topSelectableIndex < topSelectables.Length; ++topSelectableIndex)
        {
            thisSelectable.Children[gridSelectableColumnCount + topSelectableIndex] = topSelectables[topSelectableIndex];
            topSelectables[topSelectableIndex].Parent = thisSelectable;
        }

        for (int gridSelectableIndex = 0; gridSelectableIndex < gridSelectables.Length; ++gridSelectableIndex)
        {
            int correctedIndex = (((gridSelectableIndex / gridSelectableColumnCount) + 1) * trueColumnCount) + (gridSelectableIndex % gridSelectableColumnCount);

            thisSelectable.Children[correctedIndex] = gridSelectables[gridSelectableIndex];
            gridSelectables[gridSelectableIndex].Parent = thisSelectable;
        }

        thisSelectable.DefaultSelectableIndex = trueColumnCount;
        thisSelectable.IsPassThrough = true;
    }

    public void UpdateSelectablePageLink(TabletSelectablePageLink pageLink)
    {
        pageLink.linkPage = this;

        KMSelectable thisSelectable = GetComponent<KMSelectable>();

        KMSelectable pageLinkSelectable = pageLink.GetComponent<KMSelectable>();
        pageLinkSelectable.Children = new KMSelectable[1] { thisSelectable };
        pageLinkSelectable.ChildRowLength = 1;
        pageLinkSelectable.DefaultSelectableIndex = 0;
        pageLinkSelectable.IsPassThrough = false;

        thisSelectable.Parent = pageLinkSelectable;
    }
}
