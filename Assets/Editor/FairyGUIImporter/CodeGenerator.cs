using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.CodeDom;
using System.CodeDom.Compiler;
using UnityEditor;
using FairyGUI;

namespace CodeGenerate
{

    public static class CodeGeneratorSetting
    {
        public const string RootNameSpace = "CMB.UI";
        public static string ExportPath = Application.dataPath + "/" + "Scripts/UI/Packages/";

    }

    // : UnityEditor.Editor 
    public class CodeGenerator
    {
        //wbox log
        private static void LogA(string str)
        {
            //Debug.Log(str);
        }

        //bbox log
        private static void LogB(string str)
        {
            Debug.Log(str);
        }

        private static Dictionary<string, System.Type> builtInElementType = new Dictionary<string, System.Type>()
    {

        { "list", typeof(FairyGUI.GList) },
        { "text", typeof(FairyGUI.GTextField)},
        { "richtext", typeof(FairyGUI.GRichTextField) },
        { "graph",typeof(FairyGUI.GGraph) },
        { "inputtext", typeof(FairyGUI.GTextInput) },
        { "loader", typeof(FairyGUI.GLoader) },
        { "group", typeof(FairyGUI.GGroup)},
        { "image", typeof(FairyGUI.GImage) },
        { "movie", typeof(FairyGUI.GMovieClip) },

    };

        private static Dictionary<string, System.Type> customComponentType = new Dictionary<string, System.Type>()
    {
        { "Button",typeof(FairyGUI.GButton) },
        { "Label",typeof(FairyGUI.GLabel) },
        { "ProgressBar",typeof(FairyGUI.GProgressBar) },
        { "Slider",typeof(FairyGUI.GSlider) },
        { "ScrollBar",typeof(FairyGUI.GScrollBar) },
        { "ComboBox",typeof(FairyGUI.GComboBox) },
        { "PopupMenu", typeof(FairyGUI.PopupMenu) },
        {"Window" , typeof(FairyGUI.Window)},
    };

        static int seed = new System.Random(System.DateTime.Now.Millisecond).Next(0, 10000);
        private static int GetRandomInt()
        {
            int res = new System.Random(seed).Next(0, 10000);
            seed++;
            return res;
        }


        /** 递归查找自定义组件下的自定义组件 */
        private static void FillInfoRecursively(IExportable exportComponentInfo , Dictionary<string, ExportComponentInfo> exportInfoMap, Dictionary<string, ExportCustomElementInfo> exportCustomElementInfoMap, FairyGUI.UIPackage pak, List<FairyGUI.PackageItem> allComponents)
        {
            //递归查找component下面元素
            var rootItem = exportComponentInfo.PakItem;
            rootItem.Load();
            var displayItems = rootItem.extensionCreator().GetChildren();// .displayList;
            for (int i = 0, c = displayItems.Length; i < c; i++)
            {
                var displayItem = displayItems[i];
                var item = displayItem.packageItem;


                //var xml = item.componentData;
                var xml = displayItem.desc;
                //字段名
                var fieldName = xml.GetAttribute("name");
                //字段id
                var id = xml.GetAttribute("id");
                //是否应用了包内的组建
                bool hasSrc = xml.HasAttribute("src");

                //是否是否引用了其他包
                bool isRefPakComponent = xml.HasAttribute("pkg");
                string refPakId = isRefPakComponent ? xml.GetAttribute("pkg") : "";
                string refPakName = isRefPakComponent ? UIPackage.GetById(refPakId).name : "";
                if (isRefPakComponent)
                {
                    LogA("1.1$$ xml.node.src: " + xml.GetAttribute("src"));
                }
                LogA("2$$ desc xml.node.name: " + xml.name + "  fieldName:" + fieldName);

                //ultra feature
                string CustomData = xml.GetAttribute("customData");
                //Debug.Log("$$ " + fieldName + " customData:" + CustomData);
                // 否则displayListItem为内置控件
                if (item == null)
                {
                    //内置控件类型或者自定义控件元素名称 组件内的内置控件
                    var builtInTypeName = xml.name;
                    if (builtInElementType.ContainsKey(builtInTypeName))
                    {
                        LogA("2.1$$ find exist builtInElementType: " + builtInTypeName);
                        var exportInfo = new ExportInfo();
                        exportInfo.ExportCategory = ExportInfo.EExportCategory.builtInElement;
                        exportInfo.DisplayListItem = displayItem;
                        exportInfo.ExportName = fieldName;
                        exportInfo.ExportType = builtInElementType[builtInTypeName];
                        exportInfo.ExportTypeName = exportInfo.ExportType.FullName;
                        exportInfo.NodeIndex = i;
                        exportInfo.ID = id;
                        exportInfo.CustomData = CustomData;
                        exportComponentInfo.ExportInfos.Add(exportInfo);
                    }
                    else
                    {
                        Debug.LogError("2.1$$ find unexpected builtInElementType: " + builtInTypeName + " fieldName:" + fieldName);
                    }

                    LogA("2.1$$ pakItem is null desc xml.node.name:" + xml.name + " fieldName:" + fieldName);
                }
                else //item不为null 引用的是包内资源或组件
                {
                    //if (allComponents.Contains(item))
                    //{
                    //    LogA("2.2$$ exist in allComponents name: " + item.name);
                    //}
                    //else
                    //{
                    //    LogA("2.2$$ not exist in allComponents name: " + item.name);
                    //}

                    item.Load();

                    //资源和自定义动画
                    if (item.type == FairyGUI.PackageItemType.Image)
                    {
                        LogA("4$$ find elementType: Image");
                        var elementType = new ExportInfo();
                        elementType.ExportCategory = ExportInfo.EExportCategory.resElement;
                        elementType.ExportName = fieldName; //item.name;
                        elementType.ExportType = typeof(FairyGUI.GImage);
                        elementType.ExportTypeName = elementType.ExportType.FullName;
                        elementType.NodeIndex = i;
                        elementType.DisplayListItem = displayItem;
                        elementType.ID = id;
                        elementType.CustomData = CustomData;
                        exportComponentInfo.ExportInfos.Add(elementType);
                    }
                    else if (item.type == FairyGUI.PackageItemType.MovieClip)
                    {
                        LogA("4$$ find elementType: MovieClip");
                        var elementType = new ExportInfo();
                        elementType.ExportCategory = ExportInfo.EExportCategory.resElement;
                        elementType.ExportName = fieldName;// item.name;
                        elementType.ExportType = typeof(FairyGUI.GMovieClip);
                        elementType.ExportTypeName = elementType.ExportType.FullName;
                        elementType.NodeIndex = i;
                        elementType.DisplayListItem = displayItem;
                        elementType.ID = id;
                        elementType.CustomData = CustomData;
                        exportComponentInfo.ExportInfos.Add(elementType);

                    }
                    else if (item.type == FairyGUI.PackageItemType.Sound)
                    {

                    }
                    else if (item.type == FairyGUI.PackageItemType.Atlas)
                    {

                    }
                    else if (item.type == FairyGUI.PackageItemType.Font)
                    {

                    }
                    else if (item.type == FairyGUI.PackageItemType.Misc)
                    {

                    }
                    // custom elements 自定义控件元素
                    else if (item.type == FairyGUI.PackageItemType.Component)
                    {
                        string extention = item.componentData.GetAttribute("extention");
                        if (extention != null)
                        {
                            var TypeName = item.name;
                            // 引用了包内自定义控件
                            if (customComponentType.ContainsKey(extention))
                            {
                                // 导出自定义控件类型
                                if (!exportCustomElementInfoMap.ContainsKey(TypeName) && !isRefPakComponent)
                                {
                                    var packageName = pak.name;
                                    var exportCustomElementInfo = new ExportCustomElementInfo();
                                    exportCustomElementInfo.ExportExtentionType = customComponentType[extention];
                                    exportCustomElementInfo.ExportPackageName = string.Join(".", new string[] {
                                        CodeGeneratorSetting.RootNameSpace,
                                        packageName,
                                    });
                                    exportCustomElementInfo.ExportTypeName = TypeName;
                                    exportCustomElementInfo.pakItem = item;
                                    exportCustomElementInfo.ID = item.id;
                                    exportCustomElementInfoMap[TypeName] = exportCustomElementInfo;
                                    //exportComponentInfo.pakItem = item;
                                    //exportComponentInfo.ID = item.id;
                                    FillInfoRecursively(exportCustomElementInfo, exportInfoMap, exportCustomElementInfoMap, pak, allComponents);

                                }
                                LogA("6$$ find elementType:" + extention);

                                //为组件添加字段 组件里面添加自定义控件
                                var elementType = new ExportInfo();
                                elementType.ExportCategory = ExportInfo.EExportCategory.customElement;
                                elementType.ExportName = fieldName;
                                elementType.ExportType = customComponentType[extention];
                                elementType.isTypeInPak = !isRefPakComponent;
                                string FullName = string.Join(".", new string[] { CodeGeneratorSetting.RootNameSpace, refPakName, TypeName });
                                if (isRefPakComponent)
                                {
                                    //跨包只使用基础类型
                                    FullName = customComponentType[extention].FullName;
                                }
                                elementType.ExportTypeName = isRefPakComponent? FullName : string.Join(".", new string[] { CodeGeneratorSetting.RootNameSpace, pak.name, TypeName });
                                elementType.ExtentionType = customComponentType[extention];
                                elementType.NodeIndex = i;
                                elementType.DisplayListItem = displayItem;
                                elementType.ID = id;
                                elementType.CustomData = CustomData;
                                exportComponentInfo.ExportInfos.Add(elementType);
                            }
                            // 引用了包内自定义组件


                        }
                        else
                        {
                            var TypeName = item.name;
                            // 添加新的导出信息
                            if (!exportInfoMap.ContainsKey(TypeName) && !isRefPakComponent)
                            {
                                LogA("8$$ find inline component:" + extention);
                                var componentType = new ExportComponentInfo();
                                var packageName = pak.name;
                                componentType.ExportTypeName = TypeName;
                                componentType.ExportPackageName = string.Join(".", new string[] {
                                CodeGeneratorSetting.RootNameSpace,
                                    packageName,
                                //componentType.ExportTypeName,
                                });
                                componentType.pakItem = item;
                                componentType.ID = item.id;
                                exportInfoMap[componentType.ExportTypeName] = componentType;
                                //exportComponentInfo.ExportComponentInfos.Add(componentType);
                                FillInfoRecursively(componentType, exportInfoMap, exportCustomElementInfoMap, pak, allComponents);
                            }

                            // 为组件添加自定义组件成员或字段 组件里面有组件 内嵌
                            var componentTypeFiled = new ExportInfo();
                            componentTypeFiled.ExportCategory = ExportInfo.EExportCategory.customComponent;
                            componentTypeFiled.ExportName = fieldName;
                            string FullName = string.Join(".", new string[] { CodeGeneratorSetting.RootNameSpace, refPakName, TypeName });
                            if (isRefPakComponent)
                            {
                                //跨包只使用基础类型
                                FullName = typeof(FairyGUI.GComponent).FullName;
                            }
                            componentTypeFiled.ExportTypeName = isRefPakComponent? FullName: TypeName;
                            componentTypeFiled.DisplayListItem = displayItem;
                            componentTypeFiled.isTypeInPak = !isRefPakComponent;
                            componentTypeFiled.ExtentionType = typeof(FairyGUI.GComponent);
                            componentTypeFiled.NodeIndex = i;
                            componentTypeFiled.ID = id;
                            componentTypeFiled.CustomData = CustomData;
                            exportComponentInfo.ExportInfos.Add(componentTypeFiled);
                        }
                    }

                }//end if(item != null)
            }
        }

        public struct ExportPipeInfo
        {
            public Dictionary<string, ExportComponentInfo> ExportComponentInfoMap;
            public Dictionary<string, ExportCustomElementInfo> ExportCustomElementInfoMap;
        }

        public static ExportPipeInfo ParseExportInfo(List<FairyGUI.PackageItem> itemsToGen, FairyGUI.UIPackage pak)
        {
            //处理fairyGUI包根目录
            var allComponents = new List<FairyGUI.PackageItem>();
            pak.GetItems().ForEach((item) =>
            {
                if (item.type == FairyGUI.PackageItemType.Component)
                {
                    allComponents.Add(item);
                }
            });

            /** 需要导出的自定义组件信息*/
            var exportInfoMap = new Dictionary<string, ExportComponentInfo>();
            /** 需要导出的自定义元素信息 */
            var exportCustomElementInfoMap = new Dictionary<string, ExportCustomElementInfo>();

            itemsToGen.ForEach((item) =>
            {
            /** 处理组件 */
                if (item.type == FairyGUI.PackageItemType.Component)
                {
                    item.Load();

                    var xml = item.componentData;
                /** 自定控件继承的父类类型 */
                    string extention = xml.GetAttribute("extention");

                // elements
                if (extention != null)
                    {
                        var TypeName = item.name;
                        // 引用了包内自定义控件
                        if (customComponentType.ContainsKey(extention))
                        {
                            // 导出自定义控件类型
                            if (!exportCustomElementInfoMap.ContainsKey(TypeName))
                            {
                                var packageName = pak.name;
                                var exportCustomElementInfo = new ExportCustomElementInfo();
                                exportCustomElementInfo.ExportExtentionType = customComponentType[extention];
                                exportCustomElementInfo.ExportPackageName = string.Join(".", new string[] {
                                        CodeGeneratorSetting.RootNameSpace,
                                        packageName,
                                    });
                                exportCustomElementInfo.ExportTypeName = TypeName;
                                exportCustomElementInfo.pakItem = item;
                                exportCustomElementInfo.ID = item.id;
                                exportCustomElementInfoMap[TypeName] = exportCustomElementInfo;
                                FillInfoRecursively(exportCustomElementInfo, exportInfoMap, exportCustomElementInfoMap, pak, allComponents);

                            }
                        }
                        // 引用了包内自定义组件

                    }
                    else //root component
                {
                        var componentType = new ExportComponentInfo();
                        var packageName = pak.name;
                        componentType.ExportTypeName = item.name;
                        componentType.ExportPackageName = string.Join(".", new string[] {
                    CodeGeneratorSetting.RootNameSpace,
                        packageName,
                            //componentType.ExportTypeName,
                        });
                        componentType.pakItem = item;
                        componentType.ID = item.id;
                        exportInfoMap[componentType.ExportTypeName] = componentType;

                    //递归查找component下面元素
                    FillInfoRecursively(componentType, exportInfoMap, exportCustomElementInfoMap, pak, allComponents);
                    /*
                    foreach (var kv in exportInfoMap)
                    {
                        LogB("1$$ customComponent type to export:" + kv.Key);

                        kv.Value.ExportInfos.ForEach((info) =>
                        {
                            LogB("1.1$$ export Category" + info.ExportCategory.ToString() + " name:" + info.ExportName + " type: " + info.ExportTypeName);
                        });

                    }

                    foreach (var kv in exportCustomElementInfoMap)
                    {
                        LogB("2$$ customElement type to export:" + kv.Key);
                    }
                    */
                    }
                }
            });

            ExportPipeInfo exportPipeInfo = new ExportPipeInfo()
            {
                ExportComponentInfoMap = exportInfoMap,
                ExportCustomElementInfoMap = exportCustomElementInfoMap
            };

            return exportPipeInfo;
        }



        /**
         * 用于生成上下文代码
         * 因为fairGUI每个component层级深度固定，不会很多层，所以相对清晰
         * 从选中的组件为根节点 递归获取内部引用
         */
        public static void GenerateCtrlPartGen(List<FairyGUI.PackageItem> itemsToGen, FairyGUI.UIPackage pak)
        {
            EditorApplication.LockReloadAssemblies();


            var exportPipeInfo = ParseExportInfo(itemsToGen, pak);

            foreach (var typeExportInfo in exportPipeInfo.ExportComponentInfoMap)
            {
                CodeTemplateFairyGUI.GenerateCustomComponent(typeExportInfo.Key, typeExportInfo.Value, pak.id, pak.name);
            }


            //generate readonly code
            foreach (var typeExportInfo in exportPipeInfo.ExportCustomElementInfoMap)
            {
                CodeTemplateFairyGUI.GenerateCustomElement(typeExportInfo.Key, typeExportInfo.Value, pak.id, pak.name);
            }



            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UnityEditor.EditorApplication.UnlockReloadAssemblies();

        }

        public static void GenerateCtrlPartMain(List<FairyGUI.PackageItem> itemsToGen)
        {
            //generate code for user editing

        }

        public static void GenerateViewModel()
        {

        }

        public static void GenerateModel()
        {

        }




    }


    public interface IExportable
    {
        List<ExportInfo> ExportInfos { get; }
        FairyGUI.PackageItem PakItem { get; }
    }

    //导出自定义组件信息
    #region ExportComponentInfo
    public class ExportComponentInfo : IExportable
    {
        //类型名称
        //public string ExportTypeName;
        //节点名称 
        public string ExportTypeName;
        //节点路径
        public string ExportPath;
        //fullname namespace+name
        public string ExportPackageName;
        //节点Index
        public int NodeIndex;
        //component item
        public FairyGUI.PackageItem pakItem;

        public string URL
        {
            get
            {

                return string.Concat("ui://", pakItem.owner.id, pakItem.id);
            }
        }

        public List<ExportInfo> ExportInfos
        {
            get
            {
                return exportInfos;
            }
        }

        public PackageItem PakItem
        {
            get
            {
                return pakItem;
            }
        }

        //子物体的导出信息 可能有其他用于存放非自定义控件和非自定义组件
        public List<ExportInfo> exportInfos = new List<ExportInfo>();
        //存放自定义控件和组件
        //public List<ExportComponentInfo> ExportComponentInfos = new List<ExportComponentInfo>();

        //options
        public string ID;
    }
    #endregion

    //导出自定义控件信息
    #region ExportCustomElementInfo
    public class ExportCustomElementInfo : IExportable
    {
        //导出类型名称
        public string ExportTypeName;
        //导出字段名
        public string ExportName;
        //full name namespace+name
        public string ExportPackageName;
        public System.Type ExportExtentionType;
        public List<ExportInfo> exportInfos = new List<ExportInfo>();
        public FairyGUI.PackageItem pakItem;
        public string URL
        {
            get
            {
                return string.Concat("ui://", pakItem.owner.id, pakItem.id);
            }
        }

        public List<ExportInfo> ExportInfos
        {
            get
            {
                return exportInfos;
            }
        }

        public PackageItem PakItem
        {
            get
            {
                return pakItem;
            }
        }

        //options
        public string ID;
    }
    #endregion

    #region ExportInfo or ExportElementInfo
    //导出字段信息
    public class ExportInfo
    {
        //导出字段类型的大类 内置元素 自定义元素 自定义组件
        public enum EExportCategory
        {
            builtInElement, //内置控件 有ExportType 无ExtentionType
            customElement,  //自定义控件 无ExportType 有ExtentionType
            customComponent,//自定义组件 无ExportType 无ExtentionType
            resElement,//资源 无ExportType 无ExtentionType
        }
        //导出字段所属类型的大类
        public EExportCategory ExportCategory;
        //非内置控件 查找自定义控件和自定义组件
        //public bool NotInBuilt;
        //节点需要导出的类型
        public System.Type ExportType;
        public System.Type ExtentionType;
        //引用了其他组件
        public string ExportTypeName;
        //public string PackagName;
        //节点名称
        public string ExportName;
        //节点路径
        public string ExportPath;
        //节点Index
        public int NodeIndex;

        public FairyGUI.PackageItem DisplayListItem;//DisplayListItem

        //options
        public string ID;

        //自定义数据
        public string CustomData;

        public bool isTypeInPak = true;
    }

    #endregion

    //导出资源信息 Image movieClip
    #region ExportResInfo
    public class ExportResInfo
    {

    }

    #endregion

}