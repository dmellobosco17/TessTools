using UnityEditor;

namespace TessTools.Editor
{
    public class MenuItems
    {
        [MenuItem("TessTools/TessTextureEditor")]
        static void OpenTessTextureEditor()
        {
            TessTextureFormatWindow.ShowWindow();
        }
    }
}