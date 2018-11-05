//#define _CREATE_LISTITEM_FROMPOOL_
using System.Collections;
using System.Collections.Generic;
//using FairyGUI;
using UnityEngine;

using UIPackage = FairyGUI.UIPackage;
using GRoot = FairyGUI.GRoot;
using RelationType = FairyGUI.RelationType;
using ChildrenRenderOrder = FairyGUI.ChildrenRenderOrder;
namespace UIFrame
{
    public class UIManager : QFramework.QSingleton<UIManager>
    {
        private Dictionary<System.Type, List<UIBase>> managedUIMap = new Dictionary<System.Type, List<UIBase>>();
        //private Dictionary<System.Type, UIBase> managedUIMap = new Dictionary<System.Type, UIBase>();
        private List<UIBase> managedUIList = new List<UIBase>();
        private List<UIBase> pendingUIList = new List<UIBase>();

        System.Object[] cachedUIpackageIDAtts;
        Dictionary<System.Type, UIAttributes.UIPackageIDAttribute> uiTypeWithAttsDic = new Dictionary<System.Type, UIAttributes.UIPackageIDAttribute>();
        Dictionary<System.Type, UIAttributes.TargetCtrlAttribute> vmTypeWithAttsDic = new Dictionary<System.Type, UIAttributes.TargetCtrlAttribute>();

        private Dictionary<string, UIPackage> packagesWithId = new Dictionary<string, UIPackage>();

        UIManager() {

        }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            // preload fairyGUI type mapping
            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            int len = types.Length;
            for (int i = 0; i < len; i++) {
                var type = types[i];
                var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIPackageItemIDAttribute), true);
                if (attrs.Length > 0) {
                    foreach (var att in attrs)
                    {
                        var a = att as UIFrame.UIAttributes.UIPackageItemIDAttribute;
                        var constructors = a.type.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (constructors.Length > 0)
                        {
                            constructors[0].Invoke(null);
                            //Debug.Log(a.type.FullName);
                        }
                    }
                }
            }
#if _CREATE_LISTITEM_FROMPOOL_
            FairyGUI.Stage.inst.CreatePoolManager("ListPool");
#endif
            UIPackage.AddPackage("FairyGUIPak/Common");
            //AddUIPackage();

            //FairyGUI.UIConfig.defaultFont = "Microsoft YaHei, SimHei";
            //设置默认字体为黑体
            FairyGUI.UIConfig.defaultFont = "simhei";
            //获取simhei（黑体的英文名）字体
            var loadedSimhei = FairyGUI.FontManager.GetFont("simhei");
            //为黑体字体设置中文别名
            FairyGUI.FontManager.RegisterFont(loadedSimhei, @"黑体");
        }

        private UIAttributes.UIPackageIDAttribute GetPakIDAttFromType(System.Type type)
        {
            if (uiTypeWithAttsDic.ContainsKey(type))
            {
                return uiTypeWithAttsDic[type];
            }
            else
            {
                var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIPackageIDAttribute), false);
                if (attrs.Length > 0)
                {
                    uiTypeWithAttsDic[type] = attrs[0] as UIAttributes.UIPackageIDAttribute;
                }
                return uiTypeWithAttsDic[type];
            }
        }

        private UIAttributes.TargetCtrlAttribute GetTargetAttFromType(System.Type type)
        {
            if (uiTypeWithAttsDic.ContainsKey(type))
            {
                return vmTypeWithAttsDic[type];
            }
            else
            {
                var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.TargetCtrlAttribute), false);
                if (attrs.Length > 0)
                {
                    vmTypeWithAttsDic[type] = attrs[0] as UIAttributes.TargetCtrlAttribute;
                }
                return vmTypeWithAttsDic[type];
            }
        }

        public bool IsPackageLoaded(string id)
        {
            return packagesWithId.ContainsKey(id);
        }


        public void LoadPackage(System.Type type, System.Action cb)
        {
            //var attrs = typeof(T5).GetCustomAttributes(typeof(UIAttributes.UIPackageIDAttribute), false);
            //if (attrs.Length > 0)
            //{
                var att = GetPakIDAttFromType(type);// attrs[0] as UIAttributes.UIPackageIDAttribute;
                if (!this.IsPackageLoaded(att.ID))
                {
                    if (!string.IsNullOrEmpty(att.Name))
                    {
                        this.Loadpackage(att.Name, (pak) =>
                        {
                            cb();
                        });
                    }
                }
                else
                {
                    cb();
                }

            //}
        }

        public void Loadpackage(string name, System.Action<UIPackage> cb)
        {
            Debug.Log("$$ load package :" + name);
            var pak = UIPackage.AddPackage(string.Concat("FairyGUIPak/", name));
            if (!packagesWithId.ContainsKey(pak.id))
            {
                packagesWithId[pak.id] = pak;
            }
            cb(pak);
        }

        public UIPackage GetPackage(string id)
        {
            return packagesWithId[id];
        }

        /// <summary>
        /// 同步创建Fairygui对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="createObjectCallback">回调</param>
        /// <param name="list">列表容器</param>
        public void CreateInstanceSync(System.Type type, FairyGUI.UIPackage.CreateObjectCallback createObjectCallback, bool fromPool = false, FairyGUI.GList list = null)
        {
#if _CREATE_INSTANCE
            var createInstanceAsyncMethod = type.GetMethod("CreateInstance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            createInstanceAsyncMethod.Invoke(null, new object[]{
                  createObjectCallback
                });
#else
            var pakAtt = GetPakIDAttFromType(type);
            var packageName = pakAtt.Name;
            var itemName = type.Name;
            FairyGUI.GObject obj = null;
#if _CREATE_LISTITEM_FROMPOOL_
            if (fromPool && list != null)
            {
                string url = string.Concat("ui://", packageName, "/", itemName);
                obj = list.itemPool.GetObject(url);
                createObjectCallback(obj);
            }
            else
            {
                obj = FairyGUI.UIPackage.CreateObject(packageName, itemName);
                createObjectCallback(obj);
            }
#else
            obj = FairyGUI.UIPackage.CreateObject(packageName, itemName);
            createObjectCallback(obj);
#endif

#endif

        }

        public void CreateInstanceAsync(System.Type type, FairyGUI.UIPackage.CreateObjectCallback createObjectCallback)
        {
#if _CREATE_INSTANCE
            var createInstanceAsyncMethod = type.GetMethod("CreateInstance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            createInstanceAsyncMethod.Invoke(null, new object[]{
                  createObjectCallback
                });
#else
            Debug.Log("$$ package loaded, CreateInstanceAsync ...");
            var pakAtt = GetPakIDAttFromType(type);
            var packageName = pakAtt.Name;
            var itemName = type.Name;
            FairyGUI.UIPackage.CreateObjectAsync(packageName, itemName, createObjectCallback);
#endif
        }

        [System.Obsolete]
        public void CreateInstanceFromConstruct(System.Type type, FairyGUI.UIPackage.CreateObjectCallback createObjectCallback)
        {
            var createInstanceAsyncMethod = type.GetMethod("CreateInstanceAsync", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            createInstanceAsyncMethod.Invoke(null, new object[]{
                  (FairyGUI.UIPackage.CreateObjectCallback)createObjectCallback
                });
        }

        bool loopRegisted = false;
        private void registLoop()
        {
            if (!loopRegisted)
            {
                Globals.Instance.OnLateUpdateEvent += loop;
                loopRegisted = true;
            }
        }

        private void unregistLoop()
        {
            if (loopRegisted)
            {
                Globals.Instance.OnLateUpdateEvent -= loop;
                loopRegisted = false;
            }
        }

        private void loop()
        {
            if (pendingUIList.Count > 0)
            {
                var loadedUIs = new List<UIBase>(pendingUIList);
                loadedUIs.Sort((a, b) =>
                {
                    if (a.CurrentSort < b.CurrentSort)
                    {
                        return -1;
                    }
                    else if (a.CurrentSort > b.CurrentSort)
                    {
                        return 1;
                    }

                    return 0;
                });

                if (loadedUIs[0].IsLoaded)
                {
                    var loadedUI = loadedUIs[0];
                    GRoot.inst.AddChild(loadedUI.gObject);
                    loadedUI.gObject.visible = true;
                    pendingUIList.Remove(loadedUI);
                }

            }
            else
            {
                ClearPackage();
                unregistLoop();
            }



        }

        public void AddUIPackage()
        {
            UIPackage.AddPackage("FairyGUIPak/Package1");
            UIPackage.AddPackage("FairyGUIPak/Details");
            UIPackage.AddPackage("FairyGUIPak/FVideoPlayer");
            UIPackage.AddPackage("FairyGUIPak/P12_1");
            UIPackage.AddPackage("FairyGUIPak/P13_1_2");
        }

        public UIBase OpenPanelAsync(System.Type uiType, System.Action<object> OnLoad, params IViewModel[] iuidatas)
        {
            registLoop();
            return OpenPanelAsync(uiType, OnLoad, null, iuidatas);
        }

        public UIBase OpenPanelAsync(System.Type uiType, System.Action<object> OnLoad, IViewModel mainViewModel, params IViewModel[] iuidatas)
        {
            registLoop();
            var ctrl = System.Activator.CreateInstance(uiType) as UIBase;

            if (mainViewModel == null)
                mainViewModel = UIDataRepo.Instance.GetViewModelWithPanelType(uiType, true, ctrl);
            else
                UIDataRepo.Instance.RegistViewModel(uiType, mainViewModel);

            ctrl.SetMainViewModel(mainViewModel);
            ctrl.SetOptionViewModel(iuidatas);
            ctrl.Init(mainViewModel, iuidatas);
            //ctrl.Init(mainViewModel);

            ctrl.OnLoaded = () =>
            {
                ctrl.gObject.SetSize(GRoot.inst.width, GRoot.inst.height);
                ctrl.gObject.AddRelation(GRoot.inst, RelationType.Size);
                ctrl.gObject.visible = false;
                //QFramework.QEventSystem.SendEvent<UserEvents.Events>(UserEvents.Events.UI_OPEN, ctrl);
            };

            ctrl.OnLoadGObject = OnLoad;

            ctrl.CreateUIInstance();

            int sort = -1;
            managedUIList.ForEach((obj) =>
            {
                sort = Mathf.Max(sort, obj.CurrentSort);
            });
            ctrl.CurrentSort = sort + 1;

            pendingUIList.Add(ctrl);
            managedUIList.Add(ctrl);

            var type = uiType;
            if (!managedUIMap.ContainsKey(type))
            {
                managedUIMap[type] = new List<UIBase>();
            }
            managedUIMap[type].Add(ctrl);

            managedUIList.Sort((a, b) =>
            {
                return a.CurrentSort < b.CurrentSort ? -1 : 1;
            });

            for (int i = 0, c = managedUIList.Count; i < c; i++)
            {
                managedUIList[i].CurrentSort = i;
            }

            GRoot.inst.childrenRenderOrder = ChildrenRenderOrder.Ascent;// FairyGUI.ChildrenRenderOrder.Descent;

 


            return ctrl;
        }

        public T OpenPanelAsync<T>(params IViewModel[] iuidatas) where T : UIBase
        {
            return OpenPanelAsync<T>(null, iuidatas);
        }


        public T OpenPanelAsync<T>(System.Action<object> OnLoad, params IViewModel[] iuidatas) where T : UIBase
        {
            return (T)OpenPanelAsync(typeof(T), OnLoad, iuidatas);
        }

        public UIBase OpenPanelAsyncWithViewModel<T>(params IViewModel[] iuidatas)
        {
            var type = typeof(T);
            return OpenPanelAsyncWithViewModel(null, type, null, iuidatas);
        }


        public UIBase OpenPanelAsyncWithViewModel<T>(IViewModel mainViewModel, params IViewModel[] iuidatas)
        {
            var type = typeof(T);
            return OpenPanelAsyncWithViewModel(null, type, mainViewModel, iuidatas);
        }

        public UIBase OpenPanelAsyncWithViewModel<T>(System.Action<object> OnLoad, IViewModel mainViewModel, params IViewModel[] iuidatas)
        {
            var type = typeof(T);
            return OpenPanelAsyncWithViewModel(OnLoad, type, mainViewModel, iuidatas);
        }

        public UIBase OpenPanelAsyncWithViewModel(System.Action<object> OnLoad, IViewModel mainViewModel, params IViewModel[] iuidatas)
        {
            return OpenPanelAsyncWithViewModel(OnLoad, mainViewModel.GetType(), mainViewModel, iuidatas);
        }

        public UIBase OpenPanelAsyncWithViewModel(System.Action<object> OnLoad, System.Type viewModel, IViewModel mainViewModel, params IViewModel[] iuidatas) 
        {
            //var atts = viewModel.GetCustomAttributes(typeof(UIAttributes.TargetCtrlAttribute), false);
            //if (atts.Length > 0)
            //{
                var att = GetTargetAttFromType(viewModel);//atts[0] as UIAttributes.TargetCtrlAttribute;
                if (att.Type != null)
                {
                    return OpenPanelAsync(att.Type, OnLoad, mainViewModel, iuidatas);
                }else if (!string.IsNullOrEmpty(att.TypeName))
                {
                    var t = System.Reflection.Assembly.GetCallingAssembly().GetType(att.TypeName);
                    return OpenPanelAsync(t, OnLoad, mainViewModel, iuidatas);
                }
                
            //}
            return null;
        }

        //public void OpenPanelAsync(string package, string type, System.Action onFinished)
        //{
        //    throw new System.NotImplementedException();
        //}

        public void ClosePanel<T>(bool childType = false) where T : UIBase
        {
            List<UIBase> toClose = new List<UIBase>();
            for (int i = managedUIList.Count - 1; i >= 0;i--)
            {
                var obj = managedUIList[i];
                if(obj.GetType()==typeof(T) || (childType && obj.GetType().IsSubclassOf(typeof(T)))){
                    toClose.Add(obj);
                }
            }
            toClose.ForEach((ui) =>
            {
                ui.Close();
            });

        }

        public void ClosePanel(UIFrame.UIBase inst)
        {
            if (managedUIList.Contains(inst))
            {
                inst.OnClosed(()=>
                {

                });
                inst.Dispose();
                managedUIList.Remove(inst);
            }
            if (pendingUIList.Contains(inst))
            {
                pendingUIList.Remove(inst);
            }

            var type = inst.GetType();
            if (managedUIMap.ContainsKey(type))
            {
                managedUIMap[type].Remove(inst);
            }

            var vm = inst.GetMainViewModel(); // UIDataRepo.Instance.GetViewModelWithPanelType(type, false, inst);
            if (vm != null)
                UIDataRepo.Instance.UnregistViewModel(type, inst.GetMainViewModel());
            else
                Debug.Log("$$ vm is null name:" + type.FullName);
        }

        public void CloseAll()
        {
            List<UIBase> ui2Remove = new List<UIBase>();
            managedUIList.ForEach((inst) =>
            {
                if (inst != null)
                {
                    inst.OnClosed(() =>
                    {

                    });
                    inst.Dispose();
                    if (inst.gObject != null)
                    {
                        inst.gObject.Dispose();
                    }

                    ui2Remove.Add(inst);



                    var type = inst.GetType();
                    if (managedUIMap.ContainsKey(type))
                    {
                        managedUIMap[type].Remove(inst);
                    }

                    var vm = inst.GetMainViewModel(); // UIDataRepo.Instance.GetViewModelWithPanelType(type, false, inst);
                    if (vm != null)
                        UIDataRepo.Instance.UnregistViewModel(type, inst.GetMainViewModel());
                    else
                        Debug.Log("$$ vm is null name:" + type.FullName);
                }
            });


            ui2Remove.ForEach((inst) =>
            {
                managedUIList.Remove(inst);
            });

        }

        private void ClearPackage()
        {
            if (pendingUIList.Count == 0)
            {
                //Debug.Log("$$ prepare to clear package...");
                List<UIPackage> allPak = new List<UIPackage>(packagesWithId.Values);
                var pakToMove = allPak.FindAll((pak) =>
                {
                    //搜索所以ui是否使用了当前package
                    bool isExist = managedUIList.Exists((ui) =>
                        {
                        //var pakInfos = ui.gObject.GetType().GetCustomAttributes(typeof(UIAttributes.UIPackageIDAttribute), false);
                        //return pakInfos.Length > 0 && (pakInfos[0] as UIAttributes.UIPackageIDAttribute).ID == pak.id;
                        return (GetPakIDAttFromType(ui.UIType).ID == pak.id) || (ui.IsLoaded && ui.gObject.packageItem.owner.id == pak.id);
                    });
                //移除未使用的
                return !isExist;
                });

                pakToMove.ForEach((pak) =>
                {
                    packagesWithId.Remove(pak.id);
                    UIPackage.RemovePackage(pak.id);
                    Debug.Log("$$ Unload Package:" + pak.name);
                });
            }
        }

    }

}