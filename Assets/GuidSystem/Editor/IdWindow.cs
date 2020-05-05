using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuidSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class IdWindow : EditorWindow
    {
        [MenuItem("Window/ID Window")]
        public static void CreateWindow()
        {
            GetWindow<IdWindow>().Show();
        }

        private Dictionary<int, IdComponent> m_IdDict = new Dictionary<int, IdComponent>();
        private Dictionary<int, List<IdComponent>> m_ConflictDict = new Dictionary<int, List<IdComponent>>();
        private bool m_GenerateAllOpenScenes = false;
        private int m_CurrentId = 0;
        private int m_MinId = 0;
        private Vector2 m_ScrollPos;

        private List<IdComponent> m_DirtyComs = new List<IdComponent>();

        private void OnGUI()
        {
            m_GenerateAllOpenScenes = EditorGUILayout.Toggle("Generate All Open Scenes", m_GenerateAllOpenScenes);
            m_MinId = EditorGUILayout.IntField("Min ID", m_MinId);

            if (GUILayout.Button("Generate ID")) {
                m_IdDict.Clear();
                m_ConflictDict.Clear();
                m_CurrentId = 0;
                m_DirtyComs.Clear();

                if (m_GenerateAllOpenScenes) {
                    AutoGenerateId();
                }
                else {
                    AutoGenerateId(SceneManager.GetActiveScene());
                }

                foreach (var com in m_DirtyComs) {
                    EditorUtility.SetDirty(com);
                }
            }

            if (m_ConflictDict.Count > 0) {
                var sum = 0;
                foreach (var kvp in m_ConflictDict) {
                    sum += kvp.Value.Count;
                }
                var old_color = GUI.color;

                EditorGUILayout.LabelField(string.Format("Detect ID Conflict Count: {0}", sum), EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                if (GUILayout.Button("Auto Reslove")) {
                    foreach (var kvp in m_ConflictDict) {
                        for (var i = 1; i < kvp.Value.Count; ++i) {
                            var id_com = kvp.Value[i];
                            Undo.RecordObject(id_com, "Reslove Id");
                            id_com.ID = ++m_CurrentId;
                            EditorUtility.SetDirty(id_com);
                        }
                    }
                    m_ConflictDict.Clear();
                }

                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
                foreach (var kvp in m_ConflictDict) {
                    GUILayout.Space(5);
                    GUILayout.Label(string.Format("ID ({0}) Conflicted", kvp.Key));
                    foreach (var id_com in kvp.Value) {
                        EditorGUILayout.ObjectField(id_com, typeof(IdComponent), false);
                    }
                }
                EditorGUILayout.EndScrollView();

                GUI.color = old_color;
                EditorGUILayout.EndVertical();
            }
        }

        public void AutoGenerateId()
        {
            for (var i = 0; i < SceneManager.sceneCount; ++i) {
                var scene = SceneManager.GetSceneAt(i);
                AutoGenerateId(scene);
            }
        }

        public void AutoGenerateId(Scene scene)
        {
            try {
                var root_go_list = scene.GetRootGameObjects();
                for (var i = 0; i < root_go_list.Length; ++i) {
                    AutoGenerateId(root_go_list[i].transform);
                    EditorUtility.DisplayProgressBar("Auto Generating...", scene.name, (float)i / root_go_list.Length);
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        public void AutoGenerateId(Transform parent)
        {
            var id_com = parent.GetComponent<IdComponent>();
            if (id_com != null) {
                if (id_com.ID == 0) {
                    Undo.RecordObject(id_com, "Auto Id");
                    id_com.ID = ++m_CurrentId;
                    m_DirtyComs.Add(id_com);
                }
                else if (m_IdDict.ContainsKey(id_com.ID)) {
                    // conflict
                    List<IdComponent> conflict_list;
                    if (!m_ConflictDict.TryGetValue(id_com.ID, out conflict_list)) {
                        conflict_list = new List<IdComponent>() { m_IdDict[id_com.ID] };
                        m_ConflictDict.Add(id_com.ID, conflict_list);
                    }

                    conflict_list.Add(id_com);
                }
                else {
                    m_IdDict.Add(id_com.ID, id_com);

                    // make sure the auto gen id won't conflict existing ids.
                    if (id_com.ID > m_CurrentId) {
                        m_CurrentId = id_com.ID;
                    }
                }
            }

            for (var i = 0; i < parent.childCount; ++i) {
                AutoGenerateId(parent.GetChild(i));
            }
        }
    }
}