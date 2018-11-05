//#define _VIDEO_SUPPORT_
//#define _LOAD_FROM_AB_
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using FairyGUI;
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GLoader))]
    public struct GLoaderSub : IUnirxBind
    {
        FairyGUI.GLoader gObject;
        UIBase uiBase;


        public GLoaderSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o as FairyGUI.GLoader;
        }

        public UIBase GetUIBase()
        {
            return uiBase;
        }

        public GObject GetGObject()
        {
            return gObject;
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

        public void Visible(UniRx.IObservable<bool> o, bool publish = false, bool initValue = false)
        {
            var g = gObject;
            var sub = o.Subscribe((str) =>
            {
                g.visible = str;
            });
            if (publish)
            {
                g.visible = initValue;
                //o.Publish(initValue);
            }
            uiBase.AddDisposable(sub);
        }
        public void ImageFromRes(UniRx.IObservable<string> imageSrc, UniRx.IObservable<string> imageAlphaSrc = null)
        {
            var g = gObject;
            if (imageAlphaSrc == null)
            {
                var sub = imageSrc.Subscribe((str) =>
                {
#if _LOAD_FROM_AB_

#else
                    var img = Resources.Load<Texture2D>(str);
                    if (img != null)
                    {
                        g.texture = new NTexture(img);
                    }
#endif
                });
                GetUIBase().AddDisposable(sub);
            }
            else
            {
                var subAlpha = imageSrc.CombineLatest(imageAlphaSrc,(a,b)=> new Tuple<string,string>(a,b)).Subscribe((str) =>
                {
#if _LOAD_FROM_AB_

#else
                    var img = Resources.Load<Texture2D>(str.Item1);
                    UnityEngine.Assertions.Assert.IsNotNull(img, "## img at " + str.Item1 + " not found...");
                    var imgAlpha = Resources.Load<Texture2D>(str.Item2);
                    UnityEngine.Assertions.Assert.IsNotNull(img, "## imgAlpha at " + str.Item2 + " not found...");
#endif
                    NTexture texture = null;
                    if (img != null && imgAlpha == null)
                    {
                        texture = new NTexture(img);
                    }
                    else
                    {
                        texture = new NTexture(img,imgAlpha,1,1);
                    }
                    g.texture = texture;
                });
            }
        }

        public void URL(UniRx.IObservable<string> url)
        {
            var g = gObject;
            var sub = url.Subscribe((str) =>
            {
                g.url = str;
            });
            uiBase.AddDisposable(sub);
        }

        public void OnClick(UniRx.ReactiveCommand cmd)
        {
            gObject.onClick.Add(() => {cmd.Execute(); });
        }

#if _VIDEO_SUPPORT_
        public void PlayVideo(UniRx.IObservable<Texture> videoTexture)
        {
            var g = gObject;
            var sub = videoTexture.Subscribe((texture) =>
            {
                g.texture = new FairyGUI.NTexture(texture);
            });
            uiBase.AddDisposable(sub);
        }

        public void PlayVideo(UniRx.ReactiveCommand<bool> playCmd)
        {
            var g = gObject;
            var sub = playCmd.Subscribe((play) =>
            {
                g.playing = play;
            });
            uiBase.AddDisposable(sub);
        }

        public void PlayVideo(CustomController.VideoCtrl videoCtrl, FairyGUI.FillType fillType, UniRx.IObservable<string> videoPath, UniRx.IObservable<float> progress,bool isFullView = false)
        {
            var g = gObject;
            var sub = videoPath.CombineLatest(progress, (a, b) => new UniRx.Tuple<string, float>(a, b)).Sample(videoPath).Subscribe((t) =>
            {
                videoCtrl.Init(g, fillType,isFullView);
                videoCtrl.PlayVideo(t.Item1, t.Item2);
            });
            uiBase.AddDisposable(sub);
        }

        public void PlayVideoWithStartUrlAndPos(CustomController.VideoCtrl videoCtrl, FairyGUI.FillType fillType, UniRx.IObservable<Tuple<string,float>> videoPathAndStartPos,bool isFullView = false, bool autoVisible = true)
        {
            var g = gObject;
            var sub = videoPathAndStartPos.Subscribe((t)=>
            {
                if (videoCtrl.IsInited && videoCtrl.LastUrl == t.Item1)
                {
                    Debug.Log("$$ change second");
                    videoCtrl.SetSeconds(t.Item2);
                }
                if(videoCtrl.IsInited && videoCtrl.LastUrl != t.Item1)
                {
                    Debug.Log("$$ change url old:" + videoCtrl.LastUrl + " new:" + t.Item1);
                    videoCtrl.Rewind(true);
                    videoCtrl.PlayVideoWithStartPos(t.Item1, t.Item2, autoVisible);
                }
                else if (!videoCtrl.IsInited)
                {
                    Debug.Log("$$ init player");
                    videoCtrl.Init(g, fillType,isFullView);
                    videoCtrl.PlayVideoWithStartPos(t.Item1, t.Item2, autoVisible);
                }
            });
            uiBase.AddDisposable(sub);
        }

        private void playVideoWithStartPos(CustomController.VideoCtrl videoCtrl, FairyGUI.FillType fillType, UniRx.IObservable<string> videoPath, UniRx.IObservable<float> startPos)
        {
            var g = gObject;
            var sub = videoPath.CombineLatest(startPos, (a, b) => new UniRx.Tuple<string, float>(a, b)).Sample(videoPath).Subscribe((t) =>
            {
                videoCtrl.Init(g, fillType);
                videoCtrl.PlayVideoWithStartPos(t.Item1, t.Item2);
            });
            uiBase.AddDisposable(sub);
        }

        public void PauseVideo(CustomController.VideoCtrl videoCtrl, UniRx.IObservable<bool> isPause)
        {
            var g = gObject;
            bool isFirstSub = true;
            var sub = isPause.Subscribe((pause) =>
            {
                if (!isFirstSub)
                {
                    videoCtrl.Pause(pause);
                }
                isFirstSub = false;
            });
            uiBase.AddDisposable(sub);
        }

        public void ReadyToPlay(CustomController.VideoCtrl videoCtrl, UniRx.ReactiveCommand onReadyTo)
        {
            videoCtrl.OnReadyToPlay = () =>
            {
                onReadyTo.Execute();
            };
        }

        //TODO 设置进度 秒数
        public void Seconds(CustomController.VideoCtrl videoCtrl, UniRx.IObservable<float> seconds)
        {
            var g = gObject;
            var sub = seconds.Subscribe((s) =>
            {
                videoCtrl.SetSeconds(s);
            });
            uiBase.AddDisposable(sub);
        }

        //TODO 获取当前播放进度 秒数



#endif


    }

}