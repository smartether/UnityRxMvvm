using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace UIFrame
{
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GGraph))]
    public struct GGraphSub : IUnirxBind
    {
        public FairyGUI.GGraph gObject;
        public UIBase uiBase;

        public GGraphSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o.asGraph;
        }

        public UIBase GetUIBase()
        {
            return uiBase;
        }

        public FairyGUI.GObject GetGObject()
        {
            return gObject;
        }


        public void OnClick(UniRx.ReactiveCommand cmd)
        {
            gObject.displayObject.onClick.Add(() => {cmd.Execute(); });
        }

        public void OnClick(System.Action cmd)
        {
            if (gObject.displayObject != null)
            {
                gObject.displayObject.onClick.Add(() => {
                    cmd();
                    
                });
                
            }
            else
            {
                gObject.onClick.Add(() => {
                    cmd();
                    
                });
            }
        }
        
        public void OnClick(System.Action<EventContext> cmd)
        {
            if (gObject.displayObject != null)
            {
                gObject.displayObject.onClick.Add(c => cmd(c));
                
            }
            else
            {
                gObject.onClick.Add(c => cmd(c));
                
            }
        }

        public void Visible(UniRx.IObservable<bool> o)
        {
            var g = gObject;
            var sub = o.Subscribe((b) =>
            {
                g.visible = b;
            });
            uiBase.AddDisposable(sub);
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

    }
    
}