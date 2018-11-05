using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace UIFrame
{
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GMovieClip))]
    public struct GMoviceSub : IUnirxBind
    {
        public FairyGUI.GMovieClip gObject;
        public UIBase uiBase;

        public GMoviceSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o.asMovieClip;
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

        public void PlayVisible(UniRx.IObservable<bool> o)
        {
            var g = gObject;
            var sub = o.Subscribe((b) =>
            {
                g.visible = b;
                if (g.visible)
                {
                    g.playing = true;
                }
                else
                {
                    g.playing = false;
                    g.frame = -1;
                }
            });
            uiBase.AddDisposable(sub);
        }

        public void PlayClip(UniRx.IObservable<bool> o,float delay)
        {
            var g = gObject;
            g.SetPlaySettings(0, -1, 1, -1);
            g.playing = false;
            IDisposable subInner = null;
            var sub = o.Subscribe((b) =>
            {
                g.visible = b;
                if (g.visible)
                {
                    var d = Observable.Timer(TimeSpan.FromSeconds(delay));
                   
                    subInner = d.Subscribe(num =>
                    {
                        g.playing = true;
                        g.frame = 0;
                        Debug.Log("Play Clip");
                    });
                    
                }
                else
                {
                    g.playing = false;
                    g.frame = -1;
                }
            });
            uiBase.AddDisposable(subInner);
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

        public void SpriteIdx(UniRx.IObservable<int> index)
        {
            var g = gObject;
            var sub = index.Subscribe((idx) =>
            {
                g.frame = idx;
            });
            uiBase.AddDisposable(sub);
        }

    }
    
}