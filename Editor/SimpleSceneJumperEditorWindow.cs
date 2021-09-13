using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SimpleSceneJumper.Editor
{
    public class SimpleSceneJumperEditorWindow : EditorWindow
    {
        private static List<SimpleSceneJumperScene> scenes = new List<SimpleSceneJumperScene>();

        private static bool forceRefresh;
        private static GUIStyle buttonStyle;
        private static GUIContent enableIcon;
        private static GUIContent disableIcon;
        private static GUIContent refreshIcon;
        private Vector2 scrollPosition;

        [MenuItem("Window/Simple Scene Jumper")]
        public static void ShowWindow()
        {
            var window = GetWindow<SimpleSceneJumperEditorWindow>();
            window.titleContent = new GUIContent("SimpleSceneJumper");
            window.Show();
        }

        private void OnGUI()
        {
            if ((buttonStyle == null) || (refreshIcon == null) || (enableIcon == null) || (disableIcon == null) || forceRefresh)
            {
                forceRefresh = false;
                Refresh();
                Repaint();
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(refreshIcon, GUILayout.Width(32), GUILayout.Height(32)))
            {
                Refresh();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Scenes In Build", EditorStyles.boldLabel);
            var s1 = scenes.FindAll(x => x.scenesInBuild);
            foreach (var item in s1)
            {
                var text = string.Format("<b>{0}</b>\n{1}", item.name, item.path);
                var content = item.enabled ? new GUIContent(text, enableIcon.image) : new GUIContent(text, disableIcon.image);
                if (GUILayout.Button(content, buttonStyle, GUILayout.Height(40)))
                {
                    if (Event.current.button == 1)
                    {
                        OnRightClicked(item);
                    }
                    else
                    {
                        JumpScene(item.path);
                    }
                }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Others", EditorStyles.boldLabel);
            var s2 = scenes.FindAll(x => !x.scenesInBuild);
            foreach (var item in s2)
            {
                var text = string.Format("<b>{0}</b>\n{1}", item.name, item.path);
                var content = new GUIContent(text);
                if (GUILayout.Button(content, buttonStyle, GUILayout.Height(40)))
                {
                    if (Event.current.button == 1)
                    {
                        OnRightClicked(item);
                    }
                    else
                    {
                        JumpScene(item.path);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

        }

        private static void Refresh()
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.richText = true;
            enableIcon = EditorGUIUtility.IconContent("TestPassed");
            disableIcon = EditorGUIUtility.IconContent("TestIgnored");
            refreshIcon = EditorGUIUtility.IconContent("d_Refresh@2x");

            CreateScenes();
        }

        private static void CreateScenes()
        {
            scenes.Clear();

            var scenes1 = EditorBuildSettings.scenes;
            foreach (var item in scenes1)
            {
                var scene = new SimpleSceneJumperScene(item.path, true, item.enabled);
                scenes.Add(scene);
            }

            var scenes2 = AssetDatabase.FindAssets("t:Scene");
            foreach (var item in scenes2)
            {
                var scene = new SimpleSceneJumperScene(AssetDatabase.GUIDToAssetPath(item), false);
                if (!scenes.Exists(x => x.path == scene.path))
                {
                    scenes.Add(scene);
                }
            }
        }

        private static void JumpScene(string scenePath)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("This cannot be used during play mode.");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        private static void OnRightClicked(SimpleSceneJumperScene scene)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select Inspector"), false, () =>
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scene.path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            });
            menu.AddItem(new GUIContent("Copy Path"), false, () =>
            {
                GUIUtility.systemCopyBuffer = scene.path;
            });
            menu.AddItem(new GUIContent("Copy Absolute Path"), false, () =>
            {
                GUIUtility.systemCopyBuffer = Application.dataPath.Replace("Assets", scene.path);
            });
            if (scene.scenesInBuild)
            {
                if (scene.enabled)
                {
                    menu.AddItem(new GUIContent("Disable Scene"), false, () =>
                    {
                        if (string.IsNullOrEmpty(scene.path))
                        {
                            return;
                        }
                        var scenes = EditorBuildSettings.scenes.ToList();
                        scenes.ForEach(x =>
                        {
                            if (x.path == scene.path)
                            {
                                x.enabled = false;
                            }
                        });
                        EditorBuildSettings.scenes = scenes.ToArray();
                        forceRefresh = true;
                    });

                }
                else
                {
                    menu.AddItem(new GUIContent("Enable Scene"), false, () =>
                    {
                        if (string.IsNullOrEmpty(scene.path))
                        {
                            return;
                        }
                        var scenes = EditorBuildSettings.scenes.ToList();
                        scenes.ForEach(x =>
                        {
                            if (x.path == scene.path)
                            {
                                x.enabled = true;
                            }
                        });
                        EditorBuildSettings.scenes = scenes.ToArray();
                        forceRefresh = true;
                    });
                }
                menu.AddItem(new GUIContent("Remove Scenes In Build"), false, () =>
                {
                    if (string.IsNullOrEmpty(scene.path))
                    {
                        return;
                    }
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.RemoveAll((x) => x.path == scene.path);
                    EditorBuildSettings.scenes = scenes.ToArray();
                    forceRefresh = true;
                });
            }
            else
            {
                menu.AddItem(new GUIContent("Add Scenes In Build"), false, () =>
                {
                    if (string.IsNullOrEmpty(scene.path))
                    {
                        return;
                    }
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.Add(new EditorBuildSettingsScene(scene.path, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                    forceRefresh = true;
                });
            }
            menu.ShowAsContext();
        }
    }
}