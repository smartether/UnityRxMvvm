using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniRx;
namespace UIFrame {
    /// <summary>
    /// 为了减少gc  unirx封装结构都使用了struct 
    /// 通过对IUnirxBind接口的方法扩展实现 类似于class一样的继承功能
    /// 相同的绑定方法可以通过对IUnirxBind接口扩展实现
    /// </summary>
    public interface IUnirxBind {

        //void SubScribe(UniRx.ReactiveCommand cmd, System.Action<UniRx.Unit> onCmd);
        UIFrame.UIBase GetUIBase();
        FairyGUI.GObject GetGObject();
    }

    public static class IUnirxBindExtension
    {
        public static void SubScribe(this IUnirxBind gobjectSub, UniRx.ReactiveCommand cmd, System.Action<UniRx.Unit> onCmd)
        {
            var sub = cmd.Subscribe(onCmd);
            gobjectSub.GetUIBase().AddDisposable(sub);

        }

        public static void SubScribe(this IUnirxBind gobjectSub, UniRx.IReactiveCommand<UniRx.Unit> cmd, System.Action<IUnirxBind> onCmd)
        {
            var sub = cmd.Subscribe((u)=>
            {
                onCmd(gobjectSub);
            });
            gobjectSub.GetUIBase().AddDisposable(sub);

        }

        //public static void OnClose(this IUnirxBind gobjectSub, IViewStates viewState)
        //{
        //    gobjectSub.OnClick(viewState.WillClose);
        //}

        public static void OnClose(this IUnirxBind gobjectSub, IViewModel viewModel)
        {
            IViewStates viewStates = viewModel as IViewStates;
            if (viewStates != null)
            {
                gobjectSub.OnClick(viewStates.WillClose);
            }
        }

        public static void OnClick(this IUnirxBind gobjectSub, FairyGUI.EventCallback0 onClick)
        {
            gobjectSub.GetGObject().onClick.Add(() =>
            {
                onClick();
                
            });
        }

        public static void OnClick(this IUnirxBind gobjectSub, FairyGUI.EventCallback1 onClick)
        {
            gobjectSub.GetGObject().onClick.Add((arg) =>
            {
                onClick(arg);
                
            });
        }

        public static void OnClick(this IUnirxBind gobjectSub, UniRx.BoolReactiveProperty isClicked)
        {
            gobjectSub.GetGObject().onClick.Add(() => {
                isClicked.Value = true;
                
            });
        }
        public static void OnClick(this IUnirxBind gobjectSub, UniRx.IReactiveProperty<bool> isClicked)
        {
            gobjectSub.GetGObject().onClick.Add(
                () => {
                    isClicked.Value = true;
                    
                }
                );
        }

        public static void Name(this IUnirxBind s, UniRx.IObservable<string> o)
        {
            var sub = o.Subscribe((str) =>
            {
                s.GetGObject().name = str;
            });
            s.GetUIBase().AddDisposable(sub);
        }

        public static void Visible(this IUnirxBind s, UniRx.IObservable<bool> o, bool publish = false, bool initValue = false)
        {
            var sub = o.Subscribe((str) =>
            {
                s.GetGObject().visible = str;
            });
            if (publish)
            {
                s.GetGObject().visible = initValue;
                //o.Publish(initValue);
            }
            s.GetUIBase().AddDisposable(sub);
        }

        public static void Alpha(this IUnirxBind s, UniRx.IObservable<float> alpha)
        {
            var g = s.GetGObject();
            var sub = alpha.Subscribe((a) =>
            {
                g.alpha = a;
            });
            s.GetUIBase().AddDisposable(sub);
        }

        public static void Interactive(this IUnirxBind s, IObservable<bool> interactive)
        {
            var g = s.GetGObject();
            var sub = interactive.Subscribe((ia) =>
            {
                g.touchable = ia;
            });
            s.GetUIBase().AddDisposable(sub);
        }

        public static void Interactive(this IUnirxBind s, UniRx.IObservable<bool> interactive, bool hasDefaultValue = false, bool defaultValue = false)
        {
            var g = s.GetGObject();
            var sub = interactive.Subscribe((b) =>
            {
                g.touchable = b;
            });
            if (hasDefaultValue)
            {
                g.touchable = defaultValue;
            }
            s.GetUIBase().AddDisposable(sub);
        }

        public static void State(this IUnirxBind s, string controllerName, IObservable<int> state, bool hasDefault = false, int defaultState = 0)
        {
            var g = s.GetGObject();
            var sub = state.Subscribe((st) =>
            {
                var ctrl = g.asCom.GetController(controllerName);
                UnityEngine.Assertions.Assert.IsNotNull(ctrl);
                if (ctrl != null)
                {
                    ctrl.SetSelectedIndex(st);
                }
            });
            if (hasDefault)
            {
                var ctrl = g.asCom.GetController(controllerName);
                UnityEngine.Assertions.Assert.IsNotNull(ctrl);
                if (ctrl != null)
                {
                    ctrl.SetSelectedIndex(defaultState);
                }
            }
            s.GetUIBase().AddDisposable(sub);
        }
        
        public static void State(this IUnirxBind s, string controllerName, IObservable<string> state, bool hasDefault = false, string defaultState = "")
        {
            var g = s.GetGObject();
            var sub = state.Subscribe((st) =>
            {
                var ctrl = g.asCom.GetController(controllerName);
                UnityEngine.Assertions.Assert.IsNotNull(ctrl);
                if (ctrl != null)
                {
                    ctrl.SetSelectedPage(st);
                }
            });
            if (hasDefault)
            {
                var ctrl = g.asCom.GetController(controllerName);
                UnityEngine.Assertions.Assert.IsNotNull(ctrl);
                if (ctrl != null)
                {
                    ctrl.SetSelectedPage(defaultState);
                }
            }
            s.GetUIBase().AddDisposable(sub);
        }

    }
}