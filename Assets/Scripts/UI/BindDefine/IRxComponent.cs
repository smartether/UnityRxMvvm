using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIFrame
{
    using UniRx;

    public interface IRxComponent
    {
        UIBase UiBase { get;}
        FairyGUI.GObject GObject { get;}
    }

    public static class IRxComponentExtension
    {
        //对按钮控制器进行状态绑定
        public static void State(this IRxComponent rxCom, UniRx.IObservable<int> state)
        {
            State(rxCom, "button", state);
        }

        //对任意控制器进行状态绑定
        public static void State(this IRxComponent rxCom,string controllerName, UniRx.IObservable<int> state)
        {
            var sub = state.Subscribe((s) =>
            {
                var controller = rxCom.GObject.asCom.GetController(controllerName);
                if (controller != null)
                {
                    controller.SetSelectedIndex(s);
                }
            });
            rxCom.UiBase.AddDisposable(sub);
        }


        public static void State(this IRxComponent s, string controllerName, IObservable<string> state, bool hasDefault = false, string defaultState = "")
        {
            var g = s.GObject;
            var sub = state.Subscribe((st) =>
            {
                g.asCom.GetController(controllerName).SetSelectedPage(st);
            });
            if (hasDefault)
            {
                g.asCom.GetController(controllerName).SetSelectedPage(defaultState);
            }
            s.UiBase.AddDisposable(sub);
        }

        public static void Interactive(this IRxComponent s, IObservable<bool> interactive, bool hasDefaultValue = false, bool defaultValue = false)
        {
            var g = s.GObject;
            var ui = s.UiBase;
            var objSub = new GObjectSub(ui,g);
            objSub.Interactive(interactive, hasDefaultValue, defaultValue);
        }

    }
}
