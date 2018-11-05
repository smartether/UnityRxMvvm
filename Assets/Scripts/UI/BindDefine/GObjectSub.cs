using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using System;
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GObject))]
    public struct GObjectSub : IUnirxBind
    {
        FairyGUI.GObject gObject;
        UIBase uiBase;

        public GObjectSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o;
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

        public void Text(UniRx.IObservable<string> text)
        {
            var g = gObject;
            var sub = text.Subscribe((str) =>
            {
                g.text = str;
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
        public void OnClick(System.Action cmd)
        { 
            gObject.onClick.Add(
                () => {
                    cmd();
                    
                }
                );     
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

        public void Gray(UniRx.IObservable<bool> gray)
        {
            var g = gObject;
            var sub = gray.Subscribe((v) =>
            {
                g.grayed = v;
            });
            uiBase.AddDisposable(sub);
        }
    }


}