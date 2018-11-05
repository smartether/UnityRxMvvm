//#define _Q_EVENT_SYSTEM_
//#define _TIMER_SUPPORT_
using System;
using System.Collections;
using System.Collections.Generic;
//using QFramework;
using UnityEngine;

using UIHANDLE = FairyGUI.GObject;

//using UniRx;
namespace UIFrame
{
    using UniRx;

    /// <summary>
    /// Panel的UIBase 制定ViewModel类型 UIManager自己会到仓库拿已有的ViewModel
    /// </summary>
    /// <typeparam name="T">FairyGUI组件类型</typeparam>
    /// <typeparam name="T1">FairyGUI组件UniRx支持wrapper</typeparam>
    /// <typeparam name="T2">ViewModel类型</typeparam>
    public class UIBase<T, T1, T2> : UIBase<T, T1> where T : UIHANDLE where T1 : class where T2 : class, IViewModel
    {
        public static System.Type GetViewModelType()
        {
            return typeof(T2);
        }
        /// <summary>
        /// 默认构造函数 用于创建panel 指定viewmodel类型
        /// </summary>
        public UIBase() : base() // base(null, true)
        {

        }

        /// <summary>
        /// 默认构造函数 用来创建template
        /// </summary>
        /// <param name="gObjectToAttach"></param>
        /// <param name="isPanel"></param>
        public UIBase(UIHANDLE gObjectToAttach = null) : base(gObjectToAttach)
        {

        }

        /// <summary>
        /// 默认构造函数 用来创建template
        /// </summary>
        /// <param name="gObjectToAttach"></param>
        /// <param name="isPanel"></param>
        public UIBase(UIHANDLE gObjectToAttach, IViewModel viewModel) : base(gObjectToAttach, viewModel)
        {

        }

        /// <summary>
        /// 为已经创建的组件组件创建controller
        /// </summary>
        /// <param name="t">fairyGUI控件</param>
        /// <param name="vm">ViewModel</param>
        public UIBase(T t, IViewModel vm) : base(t, vm)
        {
            _gobject = t;
            mainViewModel = vm;
        }

        /// <summary>
        ///为已经创建的组件组件创建controller
        /// </summary>
        /// <param name="t">fairyGUI控件</param>
        public UIBase(T t) : base(t)
        {
            _gobject = t;
        }

        /// <summary>
        /// 带类型的Viewmodel
        /// </summary>
        public T2 MainViewModel
        {
            get
            {
                return mainViewModel as T2;
            }
        }

        public override System.Type MainViewModelType
        {
            get
            {
                return typeof(T2);
            }
        }

    }

    /// <summary>
    /// template的UIBase
    /// </summary>
    /// <typeparam name="T">FairyGUI组件类型</typeparam>
    /// <typeparam name="T1">FairyGUI组件UniRx支持wrapper</typeparam>
    public class UIBase<T, T1> : UIBase where T : UIHANDLE where T1 : class
    {
        /// <summary>
        /// 默认构造函数 用来创建panel
        /// </summary>
        public UIBase() : base(null, true)
        {
            SetMainViewModel(mainViewModel);
            //CreateInstance();
        }
        /// <summary>
        /// 默认构造函数 用来创建template
        /// </summary>
        /// <param name="gObjectToAttach"></param>
        /// <param name="isPanel"></param>
        public UIBase(UIHANDLE gObjectToAttach = null, IViewModel mainViewModel = null) : base(gObjectToAttach, false)
        {
            SetMainViewModel(mainViewModel);
            //CreateInstance();
        }

        /// <summary>
        /// 为已经创建的组件组件创建controller
        /// </summary>
        /// <param name="t">fairyGUI控件</param>
        /// <param name="vm">ViewModel</param>
        public UIBase(T t, IViewModel vm) : base(null, false)
        {
            _gobject = t;
            SetMainViewModel(mainViewModel);
        }

        /// <summary>
        ///为已经创建的组件组件创建controller
        /// </summary>
        /// <param name="t">fairyGUI控件</param>
        public UIBase(T t) : base(null, false)
        {
            _gobject = t;
        }




        //组件快捷入口
        protected T c
        {
            get
            {
                return TypedObject;
            }
        }

        protected T TypedObject
        {
            get
            {

                return this._gobject as T;
            }
        }

        T1 rx = default(T1);
        public T1 Rx
        {
            get
            {
                if (Equals(rx, null))
                {
                    rx = System.Activator.CreateInstance(typeof(T1), this, gObject as T) as T1;
                }
                return rx;
            }
        }

        public override Type UIType
        {
            get
            {
                return typeof(T);
            }
        }

        public System.Action<T> OnLoadedWithView = null;

        protected override void createObjectCallback(UIHANDLE result)
        {
            if (isGoingClose)
            {
                result.visible = false;
                result.displayObject.visible = false;
                //GameObject.Destroy(result.displayObject.gameObject);
                result.Dispose();
                return;
            }
            else
            {
                _gobject = result;
                this.PostBind();
                this.isLoaded = true;

                if (gObjectToAttach != null && gObjectToAttach.displayObject != null && gObjectToAttach.displayObject.gameObject != null)
                {
                    FairyGUI.GList list = null;
                    FairyGUI.GComponent com = null;
                    FairyGUI.GGroup group = null;
                    if ((list = gObjectToAttach.asList) != null)
                    {
                        list.AddChild(this.gObject);
                    }
                    else if ((com = gObjectToAttach.asCom) != null)
                    {
                        com.AddChild(this.gObject);
                    }
                    else if ((group = gObjectToAttach.asGroup) != null)
                    {
                        //支持往group中添加元素
                        this.gObject.group = group;
                    }
                }


                if (OnLoadedWithView != null)
                {
                    OnLoadedWithView(_gobject as T);
                    OnLoadedWithView = null;
                }
                if (OnLoaded != null)
                {
                    OnLoaded();
                    OnLoaded = null;
                }
                if (OnLoadGObject != null)
                {
                    OnLoadGObject(gObject);
                }
                if (_gobject == null)
                {
                    Debug.Log("$$ gobject is null");
                }

            }
        }


    }

    public class UIBase : IDisposable //QFramework.IUIComponents, QFramework.IUIBehaviour, 
    {
        protected bool isGoingClose = false;
        protected bool isLoaded = false;
        protected bool disposed = false;
        private bool lateUpdateRegisted = false;
        private bool fixedUpdateRegisted = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gObjectToAttach">创建后需要挂在到的父节点</param>
        /// <param name="isPanel">是否为Panel</param>
        public UIBase(UIHANDLE gObjectToAttach = null, bool isPanel = true)
        {
            this._isPanel = isPanel;
            this._gObjectToAttach = gObjectToAttach;
        }

        //创建FairyGUI的对象 同步或者异步创建都支持
        public void CreateUIInstance(bool async = true, bool fromPool = false)
        {
            UIManager.Instance.LoadPackage(UIType, () =>
            {
                if (async)
                {
                    UIManager.Instance.CreateInstanceAsync(UIType, this.createObjectCallback);
                }
                else
                {
                    FairyGUI.GList list = null;
                    if (gObjectToAttach != null)
                    {
                        list = gObjectToAttach.asList;
                    }
                    UIManager.Instance.CreateInstanceSync(UIType, this.createObjectCallback, fromPool, list);
                }
            });
        }

        protected virtual void createObjectCallback(UIHANDLE result)
        {

        }

        public virtual System.Type UIType
        {
            get;
            private set;
        }

        public virtual int SortOrder
        {
            get { return 0; }
            set { }
        }

        private void LoadPackage(System.Type type, System.Action cb)
        {
            UIManager.Instance.LoadPackage(type, cb);
        }

        public virtual System.Type MainViewModelType
        {
            get
            {
                if (mainViewModel != null)
                    return mainViewModel.GetType();
                else
                    return typeof(IViewModel);
            }
        }

        protected IViewModel mainViewModel = null;
        public IViewModel GetMainViewModel()
        {
            return mainViewModel;
        }
        public void SetMainViewModel(IViewModel vm)
        {
            mainViewModel = mainViewModel ?? vm;
        }
        protected IViewModel[] optionViewModel = null;
        public void SetOptionViewModel(IViewModel[] vm)
        {
            optionViewModel = optionViewModel ?? vm;
        }

        public System.Action OnLoaded = null;
        public System.Action<object> OnLoadGObject = null;

        /// <summary>
        /// 用于List类型的数据绑定
        /// </summary>
        protected Dictionary<object, object> _tempKv;
        public Dictionary<object, object> TempKv
        {
            get
            {
                _tempKv = _tempKv ?? new Dictionary<object, object>();
                return _tempKv;
            }
        }

        Dictionary<object, IDisposable> _dispoablesMap = null;
        protected Dictionary<object, IDisposable> DispoablesMap
        {
            get
            {
                _dispoablesMap = _dispoablesMap ?? new Dictionary<object, IDisposable>();
                return _dispoablesMap;
            }
        }

        List<IDisposable> _disposables = null;
        protected List<IDisposable> Disposables
        {
            get
            {
                if (_disposables == null)
                {
                    _gobject.onRemovedFromStage.Add(() =>
                    {
                        Dispose();
                    });
                }
                _disposables = _disposables ?? new List<IDisposable>();
                return _disposables;

            }
        }

        /// <summary>
        /// 添加Disposable到UIBase来管理
        /// </summary>
        /// <param name="dp"></param>
        public void AddDisposable(IDisposable dp)
        {
            Disposables.Add(dp);
        }

        /// <summary>
        /// 添加Disposable到UIBase来管理 对应subscription
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dp"></param>
        public void AddDisposable(object key, IDisposable dp)
        {
            DispoablesMap[key] = dp;
        }

        //TODO
        //public void AddDisposableWithObj(object ob, IDisposable dp)
        //{

        //}

        public virtual bool IsRoot
        {
            get
            {
                return _parent == null;
            }
        }

        public UIBase Root
        {
            get
            {
                if (_parent != null)
                {
                    var tmp = this;
                    while (tmp.Parent != null)
                    {
                        tmp = tmp.Parent;
                    }
                    return tmp;
                }
                return null;
            }
        }

        UIBase _parent = null;

        /// <summary>
        /// 父节点UIBase
        /// </summary>
        public UIBase Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// 是否为Panel
        /// </summary>
        private bool _isPanel = false;
        protected bool IsPanel
        {
            get { return _isPanel; }
        }

        /// <summary>
        /// 可以全部交给FairyGUI管理 不需要自己管理节点归属
        /// </summary>
        /// <param name="parent"></param>
        public void AddToPanel(UIBase parent)
        {
            _parent = parent;

            if (parent.children == null)
            {
                parent.children = new List<UIBase>();
            }
            parent.children.Add(this);
            //parent.AddDisposable(this);
        }

        /// <summary>
        /// 当前UIBase的子节点
        /// </summary>
        private List<UIBase> children = null;
        public List<UIBase> Children
        {
            get { return children; }
        }
        public List<UIBase> ChildrenCopy
        {
            get { return new List<UIBase>(children); }
        }

        /// <summary>
        /// 新节点创建完成后会挂上去的节点
        /// </summary>
        private UIHANDLE _gObjectToAttach = null;
        public UIHANDLE gObjectToAttach
        {
            get
            {
                return _gObjectToAttach;
            }
        }

        protected UIHANDLE _gobject = null;
        public UIHANDLE gObject
        {
            get
            {
                return _gobject;
            }
        }

        public Transform Transform
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 关闭Panel或者消耗节点
        /// </summary>
        /// <param name="destroy"></param>
        public void Close(bool destroy = true)
        {
            isGoingClose = true;
            if (IsPanel)
            {
                UIManager.Instance.ClosePanel(this);
            }
            else
            {
                if (_parent != null && _parent.children != null)
                {
                    _parent.children.Remove(this);
                }
                Dispose();
            }
        }

        public virtual void Hide()
        {
            throw new NotImplementedException();
        }

        //public virtual void Init(IUIData uiData = null)
        //{
        //    throw new NotImplementedException();
        //}

        //panel on close
        public virtual void OnClosed(Action onPanelClosed)
        {
            isGoingClose = true;
        }

        public virtual void Show()
        {
            throw new NotImplementedException();
        }


        public virtual void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                isGoingClose = true;
                if (_disposables != null)
                {
                    for (int i = 0, c = _disposables.Count; i < c; i++)
                    {
                        var disposable = _disposables[i];
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    _disposables.Clear();
                    _disposables = null;
                }

                if (children != null)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].Dispose();
                    }
                    children = null;
                }

                if (mainViewModel != null)
                {
                    var dis = mainViewModel as System.IDisposable;
                    if (dis != null)
                    {
                        dis.Dispose();
                    }
                }


                RemoveLoop();
                OnDispose();


                if (isLoaded)
                {
                    gObject.Dispose();
                }
                disposed = true;
            }
        }

        //添加其余需要处理的回收事物
        protected virtual void OnDispose()
        {

        }

        public virtual int ConfigSort
        {
            get
            {
                var type = GetType();
                if (UISortSetting.SortSetting.ContainsKey(type))
                {
                    return UISortSetting.SortSetting[type];
                }
                return 0;
            }
        }

        public int CurrentSort { get; set; }

        /// <summary>
        /// 用来设置控件属性或者绑定数据到控件属性
        /// </summary>
        public virtual void PostBind()
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="uidata"></param>
        /// <param name="uiDatas"></param>
        public virtual void Init(IViewModel uidata, params IViewModel[] uiDatas)
        {

        }

        public virtual void LateUpdate()
        {

        }


        protected virtual void FixedUpdate()
        {

        }


        public bool IsLoaded
        {
            get { return isLoaded; }
        }

#if _Q_EVENT_SYSTEM_

        /// <summary>
        /// controller中注册事件的接口 订阅句柄由被管理 不需要自己注销
        /// </summary>
        /// <param name="userEvent"></param>
        /// <param name="onEvent"></param>
        /// <returns></returns>
        public EventSubscription AddListener(UserEvents.Events userEvent, OnEvent onEvent)
        {
            QFramework.QEventSystem.RegisterEvent<UserEvents.Events>(userEvent, onEvent);
            var disposable = new EventSubscription(userEvent, onEvent);
            AddDisposable(disposable);
            return disposable;
        }
#endif

        /// <summary>
        /// unirx的订阅管理 订阅和ui生命周期一致时使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pub"></param>
        /// <param name="cb"></param>
        public void Subscribe<T>(UniRx.IObservable<T> pub, System.Action<T> cb)
        {
            var sub = pub.Subscribe(cb);
            AddDisposable(sub);
        }

#if _TIMER_SUPPORT_
        public void AddTimer(float delay, System.Action onFinish)
        {
            var dis = Timer.Timer.AddTimeListener(onFinish, delay, Timer.TimerListerType.TIMER_POURTIME);
            AddDisposable(dis);

        }
#endif

        public void AddLoop()
        {
            if (!lateUpdateRegisted)
            {
                Globals.Instance.OnLateUpdateEvent += LateUpdate;
                lateUpdateRegisted = true;
            }
        }


        public void AddFixedUpdateLoop()
        {
            if (!fixedUpdateRegisted)
            {
                Globals.Instance.OnFixedUpdateEvent += FixedUpdate;
                fixedUpdateRegisted = true;
            }
        }

        public void RemoveLoop()
        {
            if (lateUpdateRegisted)
            {
                Globals.Instance.OnLateUpdateEvent -= LateUpdate;
                lateUpdateRegisted = false;
            }
            if (fixedUpdateRegisted)
            {
                Globals.Instance.OnFixedUpdateEvent -= FixedUpdate;
                fixedUpdateRegisted = false;
            }
        }

        public void OpenPanelAsyncAndCloseSelf<T>() where T : UIBase
        {
            UIManager.Instance.OpenPanelAsync<T>((ui) => Close());
        }

        public void OpenPanelAsyncAndCloseSelf(System.Type type)
        {
            UIManager.Instance.OpenPanelAsync(type, (ui) => Close());
        }


        public void ConstructFromXML()
        {
        }

    }

}