using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuidSystem
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class IdComponent : MonoBehaviour
    {
        public int ID = 0;

#if UNITY_EDITOR
        //[SerializeField]
        //[HideInInspector]
        //protected int m_InstanceID = 0;

        //private void Awake()
        //{
        //    if (!Application.isPlaying) {
        //        if (m_InstanceID != 0) {
        //            // clear ID when duplicate gameobject
        //            ID = 0;
        //        }

        //        m_InstanceID = gameObject.GetInstanceID();
        //    }
        //}
#endif
    }
}

