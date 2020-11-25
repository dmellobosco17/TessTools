using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TessTools.Editor
{
    /// <summary>
    /// UnityEditor window to list all textures from project and filter them by their type and names.
    /// You can also change formatting of all textures in bulk which are filtered by given query. 
    /// </summary>
    public class TessTextureFormatWindow : EditorWindow
    {
        private static TessTextureFormatWindow _window;
        private static TextureImporterType _textureImporterType;
        private static TextureImporterFormat _textureImporterFormat;
        private static Vector2 _scrollPos;
        private static string _filter, _platform;
        private static BuildTarget _target;

        private List<string> _texturePaths;
        private GenericMenu _textureImportTypeMenu, _textureImportFormatMenu;

        /// <summary>
        /// Launch Texture Formatting Window
        /// </summary>
        public static void ShowWindow()
        {
            _window = GetWindow<TessTextureFormatWindow>("Tess Texture Formatter");
            _textureImporterType = TextureImporterType.Sprite;
            _textureImporterFormat = TextureImporterFormat.Automatic;
            _window.FilterTextures();

            _target = EditorUserBuildSettings.activeBuildTarget;
            _platform = _target.ToString();

            _window._textureImportTypeMenu = _window.TextureTypeMenu();
            _window._textureImportFormatMenu = _window.TextureFormatMenu();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawFilterBar();

            if (_texturePaths.Count > 0)
                DrawApplyBar();

            EditorGUILayout.Space(30);
            PopulateTextureList();
        }

        /// <summary>
        /// Draws the texture type select dropdown, text field and search button
        /// </summary>
        private void DrawFilterBar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Texture Type :", GUILayout.Width(100));
            if (EditorGUILayout.DropdownButton(
                new GUIContent(_textureImporterType.ToString()),
                FocusType.Passive,
                GUILayout.Width(200)))
            {
                _textureImportTypeMenu.ShowAsContext();
            }


            GUILayout.Space(100);
            _filter = EditorGUILayout.TextField("Texture Name :", _filter, GUILayout.MinWidth(300));


            EditorGUILayout.Space(1, false);
            if (GUILayout.Button(
                EditorGUIUtility.IconContent("Search Icon", "Search"),
                GUILayout.Width(30), GUILayout.Height(30)))
            {
                FilterTextures();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the texture format select dropdown and apply button 
        /// </summary>
        private void DrawApplyBar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Texture Format :", GUILayout.Width(100));
            if (EditorGUILayout.DropdownButton(
                new GUIContent(_textureImporterFormat.ToString()),
                FocusType.Passive,
                GUILayout.Width(200)))
            {
                _textureImportFormatMenu.ShowAsContext();
            }

            if (GUILayout.Button("Apply", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ApplyFormat();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws scrolling list of textures and paths provided in <see cref="_texturePaths"/>
        /// </summary>
        private void PopulateTextureList()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _texturePaths.Count; i++)
            {
                DrawTextureItem(_texturePaths[i]);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTextureItem(string path)
        {
            // Load Texture
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
            var textureGuids = AssetDatabase.FindAssets("t:Texture " + _filter);

            _texturePaths = new List<string>();

            for (int i = 0; i < textureGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                var importer = (TextureImporter) AssetImporter.GetAtPath(path);

                // Check if texture type matches the selected importType
                if (importer.textureType == _textureImporterType)
                {
                    _texturePaths.Add(path);
                }
            }
        }

        private void ApplyFormat()
        {
            for (int i = 0; i < _texturePaths.Count; i++)
            {
                // Texture importer object
                var importer = (TextureImporter) AssetImporter.GetAtPath(_texturePaths[i]);

                // Platform specific setting override object
                var settings = importer.GetPlatformTextureSettings(_platform);

                // Set selected texture format
                settings.overridden = true;
                settings.format = _textureImporterFormat;
                importer.SetPlatformTextureSettings(settings);

                // Reimport texture to apply changes
                AssetDatabase.ImportAsset(_texturePaths[i]);
            }

            // Refresh indices and repaint
            AssetDatabase.Refresh();
            Repaint();
        }

        /// <summary>
        /// Generates and return <see cref="GenericMenu"/> having all the values from <see cref="TextureImporterType"/>
        /// </summary>
        /// <returns></returns>
        private GenericMenu TextureTypeMenu()
        {
            GenericMenu menu = new GenericMenu();
            foreach (var type in Enum.GetValues(typeof(TextureImporterType)))
            {
                menu.AddItem(new GUIContent(type.ToString()), false, OnSelectTextureType, type);
            }

            return menu;
        }

        /// <summary>
        /// Generates and return <see cref="GenericMenu"/> having all the valid values from <see cref="TextureImporterFormat"/>.
        /// It only includes formats which are valid for the current <see cref="_textureImporterType"/> and <see cref="BuildTarget"/>.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Event function is called when new texture type is selected from <see cref="_textureImportTypeMenu"/>.
        /// It also generates new <see cref="_textureImportFormatMenu"/> for new <see cref="_textureImporterType"/>.
        /// </summary>
        /// <param name="userdata"></param>
        private void OnSelectTextureType(object userdata)
        {
            _textureImporterType = (TextureImporterType) userdata;
            _textureImportFormatMenu = TextureFormatMenu();
        }

        /// <summary>
        /// Event function is called when new texture format is selected from <see cref="_textureImportFormatMenu"/>.
        /// </summary>
        /// <param name="userdata"></param>
        private void OnSelectTextureFormat(object userdata)
        {
            _textureImporterFormat = (TextureImporterFormat) userdata;
        }
    }
}