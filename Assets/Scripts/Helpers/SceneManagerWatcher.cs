using System;
using System.Reflection;

public static class SceneManagerWatcher
{
    public enum State
    {
        Gameplay,
        Setup,
        PostGame,
        Transitioning,
        Unlock,
        ModManager,
        Quitting
    }

    private static readonly Type SceneManagerType = null;
    private static readonly PropertyInfo InstanceProperty = null;
    private static readonly PropertyInfo CurrentStateProperty = null;

    static SceneManagerWatcher()
    {
        SceneManagerType = ReflectionHelper.FindTypeInGame("SceneManager");
        InstanceProperty = SceneManagerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
        CurrentStateProperty = SceneManagerType.GetProperty("CurrentState", BindingFlags.Instance | BindingFlags.Public);
    }

    public static State CurrentState
    {
        get
        {
            object sceneManager = InstanceProperty.GetValue(null, null);
            if (sceneManager == null)
            {
                return State.Transitioning;
            }

            int currentState = (int)CurrentStateProperty.GetValue(sceneManager, null);

            return (State)currentState;
        }
    }
}
