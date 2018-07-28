using UnityEngine;

[ExecuteInEditMode]
public class UILayout : MonoBehaviour
{
    public Vector2 StartPosition = Vector2.zero;
    public int ColumnCount = 1;
    public Vector2 GridIncrement = Vector2.zero;

#if UNITY_EDITOR
    private void Update()
    {
        for (int childIndex = 0; childIndex < transform.childCount; ++childIndex)
        {
            Vector2 position = StartPosition;
            position.x += (childIndex % ColumnCount) * GridIncrement.x;
            position.y += (childIndex / ColumnCount) * GridIncrement.y;
            transform.GetChild(childIndex).localPosition = position;
        }
    }
#endif
}
