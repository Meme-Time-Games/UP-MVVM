using UnityEngine;

namespace MVVM.Core
{
    public abstract class BaseReactiveVariableSO : ScriptableObject
    {
#if UNITY_EDITOR
        public abstract void Raise();
#endif
    }
}