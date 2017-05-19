using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class KMHoldable : MonoBehaviour
{
    public enum HoldableScene
    {
        SetupRoom,
        GameRoom
    }

    public HoldableScene holdableScene = HoldableScene.SetupRoom;

    public Vector3 spawnPosition = new Vector3(1.0f, 0.8f, -0.1f);
    public Vector3 targetPosition = new Vector3(0.25f, 1.3f, -1.52f);
    public Vector3 targetRotation = new Vector3(13.5f, 0f, 0f);
    public int holdableChildIndex = 2;
    public int holdableAnimationIndex = 2;
    public KMSelectable[] faces = null;

    private KMSelectable _selectable = null;

    static KMHoldable()
    {
        SelectableType = ReflectionHelper.FindType("Selectable");
        if (SelectableType != null)
        {
            ParentField = SelectableType.GetField("Parent", BindingFlags.Instance | BindingFlags.Public);
            ChildrenField = SelectableType.GetField("Children", BindingFlags.Instance | BindingFlags.Public);
            ChildRowLengthField = SelectableType.GetField("ChildRowLength", BindingFlags.Instance | BindingFlags.Public);
            FocusOnInteractionField = SelectableType.GetField("FocusOnInteraction", BindingFlags.Instance | BindingFlags.Public);
            InitMethod = SelectableType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
        }

        SetupRoomType = ReflectionHelper.FindType("SetupRoom");
        GameplayRoomType = ReflectionHelper.FindType("GameplayRoom");

        ModSelectableType = ReflectionHelper.FindType("ModSelectable");
        if (ModSelectableType != null)
        {
            CopySettingsFromProxyMethod = ModSelectableType.GetMethod("CopySettingsFromProxy", BindingFlags.Instance | BindingFlags.Public);
        }

        FloatingHoldableType = ReflectionHelper.FindType("FloatingHoldable");
        if (FloatingHoldableType != null)
        {
            HoldableTargetField = FloatingHoldableType.GetField("HoldableTarget", BindingFlags.Instance | BindingFlags.Public);
            FacesField = FloatingHoldableType.GetField("Faces", BindingFlags.Instance | BindingFlags.Public);
        }

        HoldableTargetType = ReflectionHelper.FindType("HoldableTarget");
        if (HoldableTargetType != null)
        {
            AnimationIndexField = HoldableTargetType.GetField("AnimationIndex", BindingFlags.Instance | BindingFlags.Public);
        }

        FaceSelectableType = ReflectionHelper.FindType("Assets.Scripts.Input.FaceSelectable");
        if (FaceSelectableType != null)
        {
            FaceField = FaceSelectableType.GetField("Face", BindingFlags.Instance | BindingFlags.Public);
            SelectableField = FaceSelectableType.GetField("Selectable", BindingFlags.Instance | BindingFlags.Public);
        }

        FaceEnumType = ReflectionHelper.FindType("Assets.Scripts.Input.FaceEnum");
    }

    private void Awake()
    {
        StartCoroutine(AttachToScene());
    }

    private void Start()
    {
        transform.position = spawnPosition;
    }

    private IEnumerator AttachToScene()
    {
        _selectable = GetComponent<KMSelectable>();
        SetupExternalSelectable();

        Type roomType = null;
        switch (holdableScene)
        {
            case HoldableScene.SetupRoom:
                roomType = SetupRoomType;
                break;

            case HoldableScene.GameRoom:
                roomType = GameplayRoomType;
                break;

            default:
                break;
        }

        while (!AddToRoom(roomType))
        {
            yield return new WaitForSeconds(0.1f);
        }

        MakeHoldable();
    }

    private bool AddToRoom(Type roomType)
    {
        Component gameplayRoom = (Component)GameObject.FindObjectOfType(roomType);
        if (gameplayRoom != null)
        {
            AddToSelectable(gameplayRoom);
            return true;
        }

        return false;
    }

    private void AddToSelectable(Component parentSelectableComponent)
    {
        _selectable.transform.SetParent(parentSelectableComponent.transform, true);

        object thisSelectable = _selectable.GetComponent(SelectableType);
        object parentSelectable = parentSelectableComponent.GetComponent(SelectableType);

        ParentField.SetValue(thisSelectable, parentSelectable);

        Array parentChildren = (Array)ChildrenField.GetValue(parentSelectable);
        Array newParentChildren = Array.CreateInstance(SelectableType, parentChildren.Length + 1);

        Array.Copy(parentChildren, newParentChildren, holdableChildIndex);
        newParentChildren.SetValue(thisSelectable, holdableChildIndex);
        Array.Copy(parentChildren, holdableChildIndex, newParentChildren, holdableChildIndex + 1, parentChildren.Length - holdableChildIndex);

        ChildrenField.SetValue(parentSelectable, newParentChildren);
        ChildRowLengthField.SetValue(parentSelectable, ((int)ChildRowLengthField.GetValue(parentSelectable)) + 1);

        InitMethod.Invoke(parentSelectable, null);

        InputHelper.InvokeConfigureSelectableAreas(parentSelectableComponent);
    }

    private void SetupExternalSelectable()
    {
        Component[] modSelectables = _selectable.GetComponentsInChildren(ModSelectableType, true);
        foreach (Component modSelectable in modSelectables)
        {
            CopySettingsFromProxyMethod.Invoke(modSelectable, null);
        }
    }

    private void MakeHoldable()
    {
        FocusOnInteractionField.SetValue(_selectable.GetComponent(SelectableType), true);

        Component floatingHoldable = _selectable.gameObject.AddComponent(FloatingHoldableType);

        GameObject holdableTargetGameObject = new GameObject("HoldableTarget");
        holdableTargetGameObject.transform.position = targetPosition;
        holdableTargetGameObject.transform.rotation = Quaternion.Euler(targetRotation);
        holdableTargetGameObject.transform.SetParent(_selectable.transform.parent, true);

        Component holdableTarget = holdableTargetGameObject.AddComponent(HoldableTargetType);
        AnimationIndexField.SetValue(holdableTarget, holdableAnimationIndex);

        HoldableTargetField.SetValue(floatingHoldable, holdableTarget);

        Array faceEnumValues = Enum.GetValues(FaceEnumType);

        Array faceSelectables = Array.CreateInstance(FaceSelectableType, faces.Length);
        for (int faceIndex = 0; faceIndex < faces.Length && faceIndex < faceEnumValues.Length; ++faceIndex)
        {
            object faceSelectable = Activator.CreateInstance(FaceSelectableType);
            FaceField.SetValue(faceSelectable, faceEnumValues.GetValue(faceIndex));
            SelectableField.SetValue(faceSelectable, faces[faceIndex].GetComponent(SelectableType));

            faceSelectables.SetValue(faceSelectable, faceIndex);
        }

        FacesField.SetValue(floatingHoldable, faceSelectables);
    }

    #region Static Reflection Fields
    private static readonly Type SelectableType;
    private static readonly FieldInfo ParentField;
    private static readonly FieldInfo ChildrenField;
    private static readonly FieldInfo ChildRowLengthField;
    private static readonly FieldInfo FocusOnInteractionField;
    private static readonly MethodInfo InitMethod;

    private static readonly Type SetupRoomType;
    private static readonly Type GameplayRoomType;

    private static readonly Type ModSelectableType;
    private static readonly MethodInfo CopySettingsFromProxyMethod;

    private static readonly Type FloatingHoldableType;
    private static readonly FieldInfo HoldableTargetField;
    private static readonly FieldInfo FacesField;

    private static readonly Type HoldableTargetType;
    private static readonly FieldInfo AnimationIndexField;

    private static readonly Type FaceSelectableType;
    private static readonly FieldInfo FaceField;
    private static readonly FieldInfo SelectableField;

    private static readonly Type FaceEnumType;
    #endregion
}
