using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using System;
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GSlider))]
    public struct GSliderSub : IUnirxBind
    {
        FairyGUI.GSlider gObject;
        UIBase uiBase;

        public GSliderSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o as FairyGUI.GSlider;
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

        public void OnChanged(System.Action<double> changed)
        {
            var g = gObject;
            gObject.onChanged.Add(()=>
            {
                changed(g.value);
            });
        }

        public void OnDragStart(System.Action<double> changed)
        {
            var g = gObject;
            gObject.onDragStart.Add(() =>
            {
                changed(g.value);
            });
        }

        public void OnDragEnd(System.Action<double> changed)
        {
            var g = gObject;
            gObject.onDragEnd.Add(() =>
            {
                changed(g.value);
            });
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

        public void Value(IObservable<float> value, bool publish = false)
        {
            var g = gObject;
            var sub = value.Subscribe((v) =>
            {
                g.value = v;
            });
            if (publish)
            {
                value.Publish();
            }
            uiBase.AddDisposable(sub);
        }
        
        //TODO view属性变化 让viewmodel获取 双向绑定时使用要注意防止死循环 需要处理一下
        public void OnFetchValue(FloatReactiveProperty value)
        {
            var g = gObject;
            g.onChanged.Add((ctx) =>
            {
                var sl = ctx.sender as FairyGUI.GSlider;
                value.Value = (float)sl.value;
            });
        }

        public void Max(IObservable<float> max)
        {
            var g = gObject;
            var sub = max.Subscribe((m) =>
            {
                g.max = m;
            });
            uiBase.AddDisposable(sub);
        }
    }


}