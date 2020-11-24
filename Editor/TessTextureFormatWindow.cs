using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace TessTools.Editor
{
    public class TessTextureFormatWindow : EditorWindow
    {
        private static TessTextureFormatWindow _window;
        private static TextureImporterType _textureImporterType;
        private static TextureImporterFormat _textureImporterFormat;
        private static Vector2 scrollPos;
        private static string filter, platform;
        private static BuildTarget _target;
        private static Material _material;

        private string[] textureGuids;
        private List<string> texturePaths;
        private GenericMenu textureImportTypeMenu, textureImportFormatMenu;

        public static void Show()
        {
            _window = GetWindow<TessTextureFormatWindow>("Tess Texture Formatter");
            _textureImporterType = TextureImporterType.Sprite;
            _textureImporterFormat = TextureImporterFormat.Automatic;
            _material = new Material(Shader.Find("Sprites/Default"));
            _window.FilterTextures();

            _target = EditorUserBuildSettings.activeBuildTarget;
            platform = _target.ToString();

            _window.textureImportTypeMenu = _window.TextureTypeMenu();
            _window.textureImportFormatMenu = _window.TextureFormatMenu();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawFilterBar();

            if (texturePaths.Count > 0)
                DrawApplyBar();

            EditorGUILayout.Space(30);
            PopulateTextureList();
        }

        private void DrawFilterBar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Texture Type :", GUILayout.Width(100));
            if (EditorGUILayout.DropdownButton(
                new GUIContent(_textureImporterType.ToString()),
                FocusType.Passive,
                GUILayout.Width(200)))
            {
                textureImportTypeMenu.ShowAsContext();
            }


            GUILayout.Space(100);
            filter = EditorGUILayout.TextField("Texture Name :", filter, GUILayout.MinWidth(300));


            EditorGUILayout.Space(1, false);
            if (GUILayout.Button(
                EditorGUIUtility.IconContent("Search Icon", "Search"),
                GUILayout.Width(30), GUILayout.Height(30)))
            {
                FilterTextures();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawApplyBar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Texture Format :", GUILayout.Width(100));
            if (EditorGUILayout.DropdownButton(
                new GUIContent(_textureImporterFormat.ToString()),
                FocusType.Passive,
                GUILayout.Width(200)))
            {
                textureImportFormatMenu.ShowAsContext();
            }

            if (GUILayout.Button("Apply", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ApplyFormat();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void PopulateTextureList()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < texturePaths.Count; i++)
            {
                DrawTextureItem(texturePaths[i]);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTextureItem(string path)
        {
            Texture t = AssetDatabase.LoadAssetAtPath<Texture>(path);

            EditorGUILayout.BeginHorizontal();
            Rect r = EditorGUILayout.GetControlRect(false, 50);
            r.x += 20;
            r.width = 50;
            EditorGUI.DrawTextureTransparent(r, t);

            r.x += 70;
            r.width = 1000;
            EditorGUI.LabelField(r, path);

            EditorGUILayout.EndHorizontal();
        }

        private void FilterTextures()
        {
            textureGuids = AssetDatabase.FindAssets("t:Texture " + filter);

            texturePaths = new List<string>();

            for (int i = 0; i < textureGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath(path);
                if (importer.textureType == _textureImporterType)
                {
                    texturePaths.Add(path);
                }
            }
        }

        private void ApplyFormat()
        {
            for (int i = 0; i < texturePaths.Count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath(path);
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.overridden = true;
                settings.format = _textureImporterFormat;
                importer.SetPlatformTextureSettings(settings);
                AssetDatabase.ImportAsset(path);
            }

            AssetDatabase.Refresh();
            Repaint();
        }

        private GenericMenu TextureTypeMenu()
        {
            GenericMenu menu = new GenericMenu();
            foreach (var type in Enum.GetValues(typeof(TextureImporterType)))
            {
                menu.AddItem(new GUIContent(type.ToString()), false, OnSelectTextureType, type);
            }

            return menu;
        }

        private GenericMenu TextureFormatMenu()
        {
            GenericMenu menu = new GenericMenu();
            foreach (var type in Enum.GetValues(typeof(TextureImporterFormat)))
            {
                var format = (TextureImporterFormat) type;
                if (TextureImporter.IsPlatformTextureFormatValid(_textureImporterType, _target, format))
                    menu.AddItem(new GUIContent(type.ToString()), false, OnSelectTextureFormat, type);
            }

            return menu;
        }

        private void OnSelectTextureType(object userdata)
        {
            _textureImporterType = (TextureImporterType) userdata;
            textureImportFormatMenu = TextureFormatMenu();
        }

        private void OnSelectTextureFormat(object userdata)
        {
            _textureImporterFormat = (TextureImporterFormat) userdata;
        }
    }
}