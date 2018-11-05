using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using System;
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GGroup))]
    public struct GGroupSub : IUnirxBind
    {
        FairyGUI.GGroup gObject;
        UIBase uiBase;

        public GGroupSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o as FairyGUI.GGroup;
        }
        public UIBase GetUIBase()
        {
            return uiBase;
        }

        public FairyGUI.GObject GetGObject()
        {
            return gObject;
        }

        //[UIAttributes.UIBindPropertyInfo("name")]
        public void Name(UniRx.IObservable<string> o){
            var g = gObject;
            var sub = o.Subscribe((str) =>
            {
                g.name = str;
            });
            uiBase.AddDisposable(sub);
        }

        public void OnClick(UniRx.ReactiveCommand cmd)
        {
            var g = gObject;
            var sub = cmd.Subscribe((u) =>
            {
                g.onClick.Add(() => cmd.Execute());
            });
            uiBase.AddDisposable(sub);
        }
       
        public void Alpha(UniRx.IObservable<float> alpha)
        {
            var g = gObject;
            var sub = alpha.Subscribe((a) =>
            {
                g.alpha = a;
            });
            uiBase.AddDisposable(sub);
        }

        public void Visible(UniRx.IObservable<bool> visible)
        {
            var g = gObject;
            var sub = visible.Subscribe((a) =>
            {
                g.visible = a;
            });
            uiBase.AddDisposable(sub);
        }
        
        public void VisibleDelay(UniRx.IObservable<float> delay)
        {
            var g = gObject;
            g.visible = false;
            IDisposable subInner = null;
            var sub = delay.Subscribe(x =>
            {
                var d = Observable.Timer(TimeSpan.FromSeconds(x));
                   
                subInner = d.Subscribe(num =>
                {
                    g.visible = true;
                    Debug.Log("Make it visible at:"+x);
                });
            });
            uiBase.AddDisposable(subInner);
            uiBase.AddDisposable(sub);
        }


        /// <summary>
        /// 绑定列表控件和数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="list"></param>
        public void ObserverListAll<T, T1>(UniRx.IReactiveCollection<T1> list, bool autoWidth = false) where T : UIBase where T1 : IViewModel
        {
            System.Type type = typeof(T);
            System.Type vmType = typeof(T1);
            bool isImpleIViewModelCtrl = vmType.IsSubclassOf(typeof(IViewModelCtrl));
            bool isImpleIViewModelPosition = vmType.IsSubclassOf(typeof(IViewModelStyle));
            var g = gObject;
            var u = uiBase;
            var observeAdd = list.ObserveAdd();
            var subAdd = observeAdd.Subscribe((o) =>
            {
                if (isImpleIViewModelCtrl)
                {
                    type = (o.Value as IViewModelCtrl).CtrlType;
                }
                var ctrl = System.Activator.CreateInstance(type, g, o.Value) as T;
                ctrl.SetMainViewModel(o.Value as IViewModel);
                ctrl.CreateUIInstance(false);

                if (isImpleIViewModelPosition)
                {
                    var vmPos = (o.Value as IViewModelStyle);
                    var pos = vmPos.LocalPosition;
                    ctrl.gObject.SetPosition(pos.x, pos.y, pos.z);
                }
                //g.AddSelection(g.numItems, true);
                u.TempKv[o.Value] = ctrl;
                ctrl.AddToPanel(u);
            });
            var it = list.GetEnumerator();
            while (it.MoveNext())
            {
                if (isImpleIViewModelCtrl)
                {
                    type = (it.Current as IViewModelCtrl).CtrlType;
                }

                var ctrl = System.Activator.CreateInstance(type, g, it.Current) as T;
                ctrl.SetMainViewModel(it.Current as IViewModel);
                ctrl.CreateUIInstance(false);
                if (isImpleIViewModelPosition)
                {
                    var vmPos = (it.Current as IViewModelStyle);
                    var pos = vmPos.LocalPosition;
                    ctrl.gObject.SetPosition(pos.x, pos.y, pos.z);
                }
                //g.AddSelection(g.numItems, true);
                u.TempKv[it.Current] = ctrl;
                ctrl.AddToPanel(uiBase);
            }

            uiBase.AddDisposable(subAdd);

        }
        
    }


}