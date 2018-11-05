using UnityEngine;
using System.Collections;

namespace UIFrame
{
    using UniRx;
    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GButton))]
    public struct GButtonSub : IUnirxBind
    {
        FairyGUI.GButton gObject;
        UIBase uiBase;
        public GButtonSub(UIBase uiBase, FairyGUI.GObject o)
        {
            this.uiBase = uiBase;
            this.gObject = o.asButton;
        }

        public UIBase GetUIBase()
        {
            return uiBase;
        }

        public FairyGUI.GObject GetGObject()
        {
            return gObject;
        }

        public void Title(UniRx.IObservable<string> title){
            var g = gObject;
            var dis = title.Subscribe((str) =>
            {
                g.title = str;
            });
            this.uiBase.AddDisposable(dis);
        }

        public void Color(UniRx.IObservable<UnityEngine.Color> color)
        {
            var g = gObject;
            var sub = color.Subscribe((c) =>
            {
                g.titleColor = c;
            });
            uiBase.AddDisposable(sub);
        }

        public void ColorImage(IObservable<Color> color)
        {
            var g = gObject;
            var sub = color.Subscribe((c) =>
            {
                //g.SetButtonColor(c);
                g.color = c;
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

        //public void State(UniRx.IObservable<int> State)
        //{
        //    var g = gObject;
        //    var sub = State.Subscribe((s) =>
        //    {
        //        var ctrl = g.GetController("button");
        //        UnityEngine.Assertions.Assert.IsNotNull(ctrl);
        //        if (ctrl != null && ctrl.pageCount > s)
        //        {
        //            ctrl.SetSelectedIndex(s);
        //        }
        //    });
        //    uiBase.AddDisposable(sub);
        //}

        public void OnClick(UniRx.ReactiveCommand cmd)
        {
            gObject.displayObject.onClick.Add(
                () => {
                    cmd.Execute();
                    }
                );
        }
        
        public void OnClick(System.Action cmd)
        {
            gObject.displayObject.onClick.Add(
                () =>
                {
                    cmd.Invoke();
                });
        }

        public void OnClickBtn(System.Action cmd)
        {
            gObject.onClick.Clear();
            gObject.onClick.Add(
                () =>
                {
                    cmd.Invoke();
                });
        }
        
        public void OnTouchBegin(UniRx.ReactiveCommand cmd)
        {
            gObject.displayObject.onTouchBegin.Add(() => {cmd.Execute();});
        }
        
        public void OnTouchEnd(UniRx.ReactiveCommand cmd)
        {
            gObject.displayObject.onTouchEnd.Add(() => {cmd.Execute();});
        }
        
        public void Touchable(UniRx.IObservable<bool> o)
        {
            var g = gObject;
            var sub = o.Subscribe((b) =>
            {
                g.touchable = b;
            });
            uiBase.AddDisposable(sub);
        }
        
        public void OnChanged(UniRx.ReactiveCommand cmd)
        {
            gObject.onChanged.Add(() =>
            {
                cmd.Execute();
            });
        }

        public void Controller(string name, IObservable<int> selectedIndex)
        {
            var g = gObject;
            var sub = selectedIndex.Subscribe((index) =>
            {
                g.GetController(name).SetSelectedIndex(index);
            });
            this.uiBase.AddDisposable(sub);
        }

        public void StateIsDisabled(UniRx.IObservable<bool> isDisabled)
        {
            var g = gObject;
            var sub = isDisabled.Subscribe((b) =>
            {
                var btnCtrl = g.GetController("button");
                if(btnCtrl != null)
                {
                    btnCtrl.SetSelectedIndex(b?2:0);
                }
            });
            this.uiBase.AddDisposable(sub);
        }

        public void StateIsSelectedDisabled(UniRx.IObservable<bool> isDisabled)
        {
            var g = gObject;
            var sub = isDisabled.Subscribe((b) =>
            {
                if (b)
                {
                    g.relatedController.SetSelectedPage("selectedDisabled");
                }
            });
            this.uiBase.AddDisposable(sub);
        }

        public void Interactive(UniRx.IObservable<bool> interactive, bool hasDefaultValue = false, bool defaultValue = false)
        {
            var g = gObject;
            var sub = interactive.Subscribe((b) =>
            {
                g.touchable = b;
            });
            if (hasDefaultValue)
            {
                g.touchable = defaultValue;
            }
            this.uiBase.AddDisposable(sub);
        }

        public void Gray(UniRx.IObservable<bool> gray, bool hasDefaultValue = false, bool defaultValue = false, bool grayChildren = false)
        {
            var g = gObject;
            var sub = gray.Subscribe((b) =>
            {
                if (grayChildren)
                {
                    for (int i = 0, c = g.numChildren; i < c; i++)
                    {
                        var child = g.GetChildAt(i);
                        if (child != null)
                        {
                            child.grayed = b;
                        }
                    }
                }
                else
                {
                    g.grayed = b;
                }
            });
            if (hasDefaultValue)
            {
                if (grayChildren)
                {
                    for (int i = 0, c = g.numChildren; i < c; i++)
                    {
                        var child = g.GetChildAt(i);
                        if (child != null)
                        {
                            child.grayed = defaultValue;
                        }
                    }
                }
                else
                {
                    g.grayed = defaultValue;
                }
            }
            this.uiBase.AddDisposable(sub);
        }
    }

}