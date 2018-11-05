using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CodeGenerate;

public class CodeGenerateEditor : EditorWindow{

    
    static CodeGenerateEditor _instance;
    public static void Create(FairyGUI.UIPackage target)
    {
        if(_instance!=null)
        {
            _instance.Close();
            _instance = null;
        }
        _instance = CreateInstance<CodeGenerateEditor>();
        _instance.target = target;
        _instance.Show();
    }

    public CodeGenerateEditor()
    {
    }

    FairyGUI.UIPackage target;

    Vector2 scrollPos = Vector2.zero;
    Vector2 scrollPos1 = Vector2.zero;

    List<FairyGUI.PackageItem> fairGUIItems = null;
    bool[] selectInfos = null;

    private void OnFocus()
    {
        

    }

    private void OnGUI()
    {
        if (target == null || EditorApplication.isCompiling)
        {
            Close();
            DestroyImmediate(this);
            _instance = null;
        }

        if(fairGUIItems==null){
            fairGUIItems = target.GetItems();
            selectInfos = new bool[fairGUIItems.Count];
        }

        EditorGUILayout.BeginVertical();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0, c = selectInfos.Length; i < c; i++)
        {
            var itemName = fairGUIItems[i].name;
            if (!string.IsNullOrEmpty(itemName))
            {
                //GUI.Button(Rect.MinMaxRect(0, 0, 600, 80), "");
                //EditorGUI.DropdownButton(Rect.MinMaxRect(0, 0, 600, 80), new GUIContent("", ""), FocusType.Passive);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(itemName);
                selectInfos[i] = EditorGUILayout.Toggle(selectInfos[i]);

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("生成Ctrl"))
        {
            List<FairyGUI.PackageItem> itemsToGen = new List<FairyGUI.PackageItem>();
            for(int i = 0, c = selectInfos.Length; i < c; i++)
            {
                if (selectInfos[i])
                {
                    itemsToGen.Add(fairGUIItems[i]);
                }
            }
            
            CodeGenerator.GenerateCtrlPartGen(itemsToGen, target);
            CodeGenerator.GenerateCtrlPartMain(itemsToGen);
        }
        EditorGUILayout.EndVertical();

    }

    [MenuItem("Assets/Tools/GenerateFairyGUICode")]
    public static void PickPackageToGenerateCode()
    {
        if (UnityEditor.Selection.objects.Length > 1) return;

        var selected = UnityEditor.Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(selected);
        if (!path.StartsWith(FairyGUIImportSetting.UIRes_Path) && !path.StartsWith(FairyGUIImportSetting.UIRes_Path1))
        {
            return;
        }
        var Object = selected as Object;
        if (Object.name.Contains("@"))
        {
            return;
        }

        var packageName = Object.name.Split('.')[0];
        #region Comment reload
        //FairyGUI.UIPackage.RemovePackage(packageName);
        //FairyGUI.UIPackage.AddPackage(path, (string name, string extension, System.Type type) =>
        //{
        //    AssetDatabase.LoadAssetAtPath<type>()
        //});
        #endregion

        FairyGUIEditor.EditorToolSet.ReloadPackages();
        var pak = FairyGUI.UIPackage.GetByName(packageName);

        if (pak != null)
        {
            CodeGenerateEditor.Create(pak);
        }
    }



    

}
