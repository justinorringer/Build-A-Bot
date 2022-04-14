using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BuildABot
{
    public class PackSpritesWindow : EditorWindow
    {
        [MenuItem("Build-A-Bot/Pack Sprites")]
        private static void ShowWindow()
        {
            PackSpritesWindow window = GetWindow<PackSpritesWindow>();
            window.titleContent = new GUIContent("Pack Sprites Tool");
            Texture2D[] sources = new Texture2D[Selection.objects.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                if (Selection.objects[i] is Texture2D s)
                {
                    sources[i] = s;
                }
            }
            window.sources.Clear();
            window.sources.AddRange(sources);
            window.nameOverrides.Clear();
            foreach (Texture2D tex in sources)
            {
                window.nameOverrides.Add(tex.name);
            }
            window.Show();
        }
        
        [MenuItem("Build-A-Bot/Pack Sprites", true)]
        private static bool ShowWindowValidation()
        {
            bool valid = Selection.objects.Length > 0;
            foreach (Object obj in Selection.objects)
            {
                valid &= obj is Texture2D;
                if (!valid) return false;
            }

            return valid;
        }

        [SerializeField] private string resultName = "Spritesheet";
        [SerializeField] private int padding = 2;
        [SerializeField] private List<Texture2D> sources = new List<Texture2D>();
        [SerializeField] private List<string> nameOverrides = new List<string>();

        private SerializedObject _serializedObject;
        private ReorderableList _displayList;
        private string _lastOutput;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            _displayList = new ReorderableList(_serializedObject, _serializedObject.FindProperty("sources"), 
                false, true, true, true);

            _displayList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = _displayList.serializedProperty.GetArrayElementAtIndex(index);
                
                Rect inputRect = rect;
                inputRect.width *= 0.4f;
                inputRect.height -= EditorGUIUtility.standardVerticalSpacing;
                nameOverrides[index] = EditorGUI.TextField(inputRect, GUIContent.none, nameOverrides[index]).Trim();
                
                Rect texRect = rect;
                texRect.width *= 0.5f;
                texRect.x += texRect.width;
                texRect.y += EditorGUIUtility.standardVerticalSpacing * 0.5f;
                EditorGUI.PropertyField(texRect, element, GUIContent.none, true);
            };

            _displayList.onRemoveCallback += list =>
            {
                nameOverrides.RemoveAt(list.index);
                sources.RemoveAt(list.index);
            };
            
            _displayList.drawHeaderCallback = (rect) => {
                EditorGUI.LabelField(rect, "Textures to pack");
            };

            _displayList.elementHeightCallback = index =>
            {
                SerializedProperty element = _displayList.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing;
            };

        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
            _serializedObject.Update();
            resultName = EditorGUILayout.TextField("Result Name", resultName).Trim();
            padding = EditorGUILayout.IntField("Padding", padding);
            _displayList.DoLayoutList();
            _serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Pack Textures"))
            {
                PackTextures();
            }
            EditorGUILayout.EndScrollView();
        }

        private void PackTextures()
        {
            string folder = EditorUtility.SaveFolderPanel("Output Folder", _lastOutput, "");
            if (folder.Length == 0) return;
            if (!folder.StartsWith(Application.dataPath)) {
                Debug.LogWarning("The output folder specified must be within this project.");
                return;
            }
            string relFolder = "Assets" + folder.Substring(Application.dataPath.Length);
            _lastOutput = relFolder;

            Texture2D result = new Texture2D(0, 0, TextureFormat.RGBAFloat, false);
            Rect[] packedSprites = result.PackTextures(sources.ToArray(), padding, 4096);
            
            int count = packedSprites.Length;
            SpriteMetaData[] metadata = new SpriteMetaData[count];
            TMP_Sprite[] tmpMetadata = new TMP_Sprite[count];
            for (int i = 0; i < count; i++)
            {

                Rect pixelRect = packedSprites[i];
                pixelRect.x *= result.width;
                pixelRect.y *= result.height;
                pixelRect.width *= result.width;
                pixelRect.height *= result.height;
                
                metadata[i] = new SpriteMetaData
                {
                    name = nameOverrides[i],
                    pivot = new Vector2(0, 0),
                    rect = pixelRect
                };

                tmpMetadata[i] = new TMP_Sprite
                {
                    name = nameOverrides[i],
                    x = pixelRect.x,
                    y = pixelRect.y,
                    width = pixelRect.width,
                    height = pixelRect.height,
                    xAdvance = pixelRect.width,
                    xOffset = 0,
                    yOffset = 0,
                    scale = 1f,
                    id = i,
                    hashCode = TMP_TextUtilities.GetSimpleHashCode(nameOverrides[i])
                };
            }

            string filepath = $"{folder}/{resultName}.png";
            string relFilepath = $"{relFolder}/{resultName}.png";
            
            byte[] data = result.EncodeToPNG();
            File.WriteAllBytes(filepath, data);
            
            AssetDatabase.ImportAsset(relFilepath, ImportAssetOptions.ForceUpdate);
            
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(relFilepath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.spritesheet = metadata;
            
            AssetDatabase.ImportAsset(relFilepath, ImportAssetOptions.ForceUpdate);
            Texture2D texResult = AssetDatabase.LoadAssetAtPath<Texture2D>(relFilepath);
            EditorUtility.SetDirty(texResult);
            AssetDatabase.SaveAssetIfDirty(texResult);
            
            AssetDatabase.Refresh();
        }
    }
}