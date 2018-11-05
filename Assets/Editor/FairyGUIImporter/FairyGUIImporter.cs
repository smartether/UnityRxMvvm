using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FairyGUIEditor;
using FairyGUI;

public class FairyGUIImporter : UnityEditor.AssetPostprocessor {

    static void OnPreprocessAsset()
    {


    }
    static bool isInProcessing = false;

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        return;
        if (isInProcessing) return;
        if (movedAssets.Length > 0 || movedFromAssetPaths.Length > 0) return;
        List<TextAsset> fairyGUIAssets = new List<TextAsset>();
        List<Texture> fairyGUIAtlas = new List<Texture>();
        List<string> fairyGUIAssetGUIDs = new List<string>();

        List<string> packageNames = new List<string>();

        foreach (string path in importedAssets)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            //if (!path.Contains("@"))
            //{
            //    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            //    packageNames.Add(AssetDatabase.);
            //}
            if (path.StartsWith(FairyGUIImportSetting.UIRes_Path)) // && !path.Contains("@")
            {
                
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                
                if (asset != null)
                {
                    fairyGUIAssets.Add(asset);
                    fairyGUIAssetGUIDs.Add(guid);
                }
                var assetAtlas = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (assetAtlas != null)
                {
                    fairyGUIAtlas.Add(assetAtlas);
                    fairyGUIAssetGUIDs.Add(guid);
                }
            }
        }

        if (packageNames.Count > 0)
        {
            packageNames.ForEach((pakName) =>
            {
            });


        }

        List<string> InProcessPackageNames = new List<string>();


        isInProcessing = true;
        fairyGUIAssets.ForEach((asset) =>
        {

            string packageName = asset.name.Split('@')[0];
            if (!InProcessPackageNames.Exists((name) => packageName == name))
            {
                InProcessPackageNames.Add(packageName);
                //if (!AssetDatabase.IsValidFolder(FairyGUIImportSetting.UIRes_Path + "/" + packageName))
                {
                    var folder = AssetDatabase.CreateFolder(FairyGUIImportSetting.UIRes_Path, packageName);
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(folder));
                    AssetDatabase.Refresh(options: ImportAssetOptions.ForceUpdate);
                    
                }
            }
            //var folder = AssetDatabase.CreateFolder(FairyGUIImportSetting.UIRes_Path, packageName);
            //AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(folder));


            var assetPath = AssetDatabase.GetAssetPath(asset);
            var assetFileNameSp = assetPath.Split('/', '\\');
            var assetFileName = assetFileNameSp[assetFileNameSp.Length - 1];


            var res = AssetDatabase.MoveAsset(FairyGUIImportSetting.UIRes_Path + "/" + assetFileName, FairyGUIImportSetting.UIRes_Path + "/" + packageName + "/" + assetFileName);
            Debug.Log("$$ err:" + res);

            //FileUtil.MoveFileOrDirectory(FairyGUIImportSetting.UIRes_Path + "/" + assetFileName, FairyGUIImportSetting.UIRes_Path + "/" + packageName + "/" + assetFileName);

            //FileUtil.ReplaceFile(FairyGUIImportSetting.UIRes_Path + "/" + assetFileName, FairyGUIImportSetting.UIRes_Path + "/" + packageName + "/" + assetFileName);
            //Debug.Log("$$ assetName:" + asset.name);

  
        });
        /*
        
        UIPackage.RemoveAllPackages();
        FontManager.Clear();
        NTexture.DisposeEmpty();
        int cnt = fairyGUIAssetGUIDs.Count;
        fairyGUIAssetGUIDs.ForEach((guid) =>
        {
            string[] ids = AssetDatabase.FindAssets("@sprites t:textAsset");

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            int pos = assetPath.LastIndexOf("@");
            if (pos == -1)
                return;

            assetPath = assetPath.Substring(0, pos);
            if (AssetDatabase.AssetPathToGUID(assetPath) != null)
                UIPackage.AddPackage(assetPath,
                    (string name, string extension, System.Type type) =>
                    {
                        return AssetDatabase.LoadAssetAtPath(name + extension, type);
                    }
                );

        });

        List<UIPackage> pkgs = UIPackage.GetPackages();
        pkgs.Sort(CompareUIPackage);

        cnt = pkgs.Count;
        packagesPopupContents = new GUIContent[cnt + 1];
        for (int i = 0; i < cnt; i++)
            packagesPopupContents[i] = new GUIContent(pkgs[i].name);
        packagesPopupContents[cnt] = new GUIContent("Please Select");

        UIConfig.ClearResourceRefs();
        UIConfig[] configs = GameObject.FindObjectsOfType<UIConfig>();
        foreach (UIConfig config in configs)
            config.Load();

        EMRenderSupport.Reload();
        

        isInProcessing = false;

        */

    }

    public static GUIContent[] packagesPopupContents;

    static int CompareUIPackage(UIPackage u1, UIPackage u2)
    {
        return u1.name.CompareTo(u2.name);
    }

}
