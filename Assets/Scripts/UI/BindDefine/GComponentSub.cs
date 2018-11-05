using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using UniRx;

    [UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GComponent))]
    public struct GComponentSub : IUnirxBind
    {
        public FairyGUI.GComponent gObject;
        public UIBase uiBase;

        public GComponentSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o.asCom;
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
        public void Name(IObservable<string> o){
            var g = gObject;
            var sub = o.Subscribe((str) =>
            {
                g.name = str;
            });
            uiBase.AddDisposable(sub);
        }

        public void OnClick(ReactiveCommand cmd)
        {
            gObject.onClick.Add(() => {cmd.Execute(); });
        }
        
        public void OnClick(Action act)
        {
            gObject.onClick.Add(()=>act());
        }

        public void Visible(IObservable<bool> o)
        {
            var g = gObject;
            var sub = o.Subscribe((b) =>
            {
                g.visible = b;
            });
            uiBase.AddDisposable(sub);
        }

        public void Text(IObservable<string> text,string childName)
        {
            var g = gObject;
            var sub = text.Subscribe((c) =>
            {
                var tf = g.GetChild(childName).asTextField;
                if (tf!=null)
                {
                    tf.text = c;
                }
            });
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
            bool isImpleIViewModelCtrl = typeof(T1).IsSubclassOf(typeof(IViewModelCtrl));
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
                //g.AddSelection(g.numItems, true);
                u.TempKv[o.Value] = ctrl;
                if (autoWidth)
                {
                    adjustWidth(ctrl, g);
                }
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
                //g.AddSelection(g.numItems, true);
                u.TempKv[it.Current] = ctrl;
                if (autoWidth)
                {
                    adjustWidth(ctrl, g);
                }
                ctrl.AddToPanel(uiBase);
            }

            uiBase.AddDisposable(subAdd);


            var subRemove = list.ObserveRemove().Subscribe((o) =>
            {
                var item = g.GetChildAt(o.Index);
                g.RemoveChildAt(o.Index, true);
                //item.RemoveFromParent();
                //item.Dispose();
                if (u.TempKv.ContainsKey(o.Value))
                {
                    var uiBase1 = u.TempKv[o.Value] as UIBase;
                    uiBase1.Close();
                    u.TempKv.Remove(o.Value);
                    if (autoWidth)
                    {
                        adjustWidth(uiBase1, g);
                    }
                }
            });
            uiBase.AddDisposable(subRemove);

            //TODO Maybe bug  try:g.container.GetChildAt
            var subMove = list.ObserveMove().Subscribe((o) =>
            {
                var child = g.GetChildAt(o.OldIndex);
                g.SetChildIndex(child, o.NewIndex);
                //child.RemoveFromParent();
                //if (o.NewIndex <= o.OldIndex)
                //{
                //    g.SetChildIndex(child, o.NewIndex);
                //}
                //else
                //{
                //    g.SetChildIndex(child, o.NewIndex + 1);
                //}
            });
            uiBase.AddDisposable(subMove);
        }

        private static void adjustWidth(UIBase ctrl, FairyGUI.GComponent g)
        {
            try
            {
                var width = ctrl.gObject.width;
                var containerWidth = g.viewWidth;
                var itemWidth = containerWidth / g.numChildren;
                for (int i = 0; i < g.numChildren; i++)
                {
                    g.GetChildAt(i).width = itemWidth;
                }

            }
            catch (System.NullReferenceException e)
            {

            }
        }
    }

}