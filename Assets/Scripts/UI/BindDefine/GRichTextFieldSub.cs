using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using UniRx;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GRichTextField))]
    public struct GRichTextFieldSub : IUnirxBind
    {
        public FairyGUI.GRichTextField gObject;
        public UIBase uiBase;

        public GRichTextFieldSub(UIBase uiBase, FairyGUI.GRichTextField o){
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

        //注解生成
        [UIFrame.UIAttributes.UIBindPropertyInfoAttribute(typeof(string), "text")]
        public void Text(UniRx.IObservable<string> text, bool BindToInputTextField = false, bool BindToRichTextField = false)
        {
            var g = gObject;
            var sub = text.Subscribe((str) =>
            {
                str = string.IsNullOrEmpty(str) ? string.Empty : str;
                if (BindToInputTextField)
                {
                    g.text = str;
                }
                else if (BindToRichTextField)
                {
                    g.asRichTextField.text = str;
                }
                else
                {
                    g.text = str;
                }
            });
            uiBase.AddDisposable(sub);
        }

        public void FetchText(UniRx.ReactiveProperty<string> text)
        {
            var g = gObject;
            var textInput = g.asTextInput;
            if(textInput != null)
            {
                textInput.onChanged.Add((evt) =>
                {
                    var sender = evt.sender as FairyGUI.GTextInput;
                    sender.editable = false;
                    text.Value = sender.text;
                    sender.editable = true;
                });
            }
        }

        public void Editable(UniRx.IObservable<bool> editable)
        {
            var g = gObject;
            var sub = editable.Subscribe((b) =>
            {
                g.richTextField.touchable = true;
            });
            uiBase.AddDisposable(sub);
        }

        public void Text(string text)
        {
            gObject.text = text;
        }

        public void OnClick(UniRx.ReactiveCommand cmd)
        {
            gObject.onClick.Add(() => {cmd.Execute(); });
        }

    }

}