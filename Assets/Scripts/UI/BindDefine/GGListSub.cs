//#define _CREATE_LISTITEM_FROMPOOL_
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using UniRx;
    using UniRx_Ext;

    [UIFrame.UIAttributes.UIBindTypeInfo(typeof(FairyGUI.GList))]
    public struct GListSub : IUnirxBind
    {
        public FairyGUI.GList gObject;
        public UIBase uiBase;

        public GListSub(UIBase uiBase, FairyGUI.GObject o){
            this.uiBase = uiBase;
            this.gObject = o as FairyGUI.GList;
        }

        public UIBase GetUIBase()
        {
            return uiBase;
        }

        public FairyGUI.GObject GetGObject()
        {
            return gObject;
        }

        //public void ObserverAdd<T, T1>(UniRx.IReadOnlyReactiveCollection<T1> list) where T : UIBase
        //{
        //    var g = gObject;
        //    var sub = list.ObserveAdd().Subscribe((add) =>
        //    {
        //        var item = System.Activator.CreateInstance(typeof(T), add.Value, g) as UIBase;
                
        //    });
        //    uiBase.AddDisposable(sub);
            
        //}
        
        /// <summary>
        /// 绑定列表控件和数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="list"></param>
        public void ObserverListAll<T, T1>(UniRx.IReactiveCollection<T1> list, bool autoWidth = false) where T : UIBase where T1: IViewModel
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
                var ctrl = System.Activator.CreateInstance(type, g,o.Value) as  T;
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
                Debug.Log("$$ will remove item...");
                var item = g.GetChildAt(o.Index);
#if _CREATE_LISTITEM_FROMPOOL_
                g.RemoveChildToPoolAt(o.Index);
#else
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
#endif


            });
            uiBase.AddDisposable(subRemove);

            //TODO Maybe bug  try:g.container.GetChildAt
            var subMove = list.ObserveMove().Subscribe((o) =>
            {
                //var child = g.GetChildAt(o.OldIndex);
                //g.SetChildIndex(child, o.NewIndex);
                g.SwapChildrenAt(o.OldIndex, o.NewIndex);
            });

            var subReset = list.ObserveReset().Subscribe((unit) =>
            {
#if _CREATE_LISTITEM_FROMPOOL_
                g.RemoveChildrenToPool();
#else
                g.RemoveChildren();
                foreach (var o in u.TempKv)
                {
                    if (o.Key is T1 && list.Contains((T1)o.Key))
                    {
                        var uiBase1 = o.Value as UIBase;
                        if (uiBase1 != null)
                            uiBase1.Close();
                    }
                }
                //u.TempKv.Clear();
#endif

            });

            uiBase.AddDisposable(subMove);
        }

        private static void adjustWidth(UIBase ctrl, FairyGUI.GList g)
        {
            try
            {
                var width = ctrl.gObject.width;
                var containerWidth = g.viewWidth;
                var itemWidth = containerWidth / g.numItems;
                for (int i = 0; i < g.numItems; i++)
                {
                    g.GetChildAt(i).width = itemWidth;
                }

            }
            catch (System.NullReferenceException e)
            {

            }
        }

        //用于绑定可切换的ReactiveCollection 每次切换都会重新填充list 只建议使用在少量数据的表中
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="reactCollection">可被监听的reactCollection容器</param>
        /// <param name="isDeepReact">false ： 容器切换了 刷新全部   true： 容器切换了 刷新全部  容器内元素变化了 也刷新全部</param>
        /// <param name="autoWidth"></param>
        public void ObservePage<T,T1>(IObservable<IReactiveCollection<T1>> reactCollection, bool isDeepReact = false, bool autoWidth = false) where T : UIBase where T1 : IViewModel
        {
            System.Type type = typeof(T);
            bool isImpleIViewModelCtrl = typeof(T1).IsSubclassOf(typeof(IViewModelCtrl));
            var g = gObject;
            var u = uiBase;
            var anyChange = isDeepReact? reactCollection.Select((a) => a.ObserveItemChanged()).Switch() : reactCollection;
            var sub = anyChange.Subscribe((list) =>
            {
                Debug.Log("$$ list changed... list count:" + list.Count);
                //清空list
#if _CREATE_LISTITEM_FROMPOOL_
                g.RemoveChildrenToPool();
#else
                g.RemoveChildren();
                foreach (var o in u.TempKv)
                {
                    if (o.Key is T1 && list.Contains((T1)o.Key))
                    {
                        var uiBase1 = o.Value as UIBase;
                        if (uiBase1 != null)
                            uiBase1.Close();
                    }
                }
#endif
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
                    ctrl.AddToPanel(u);
                }

            });
            uiBase.AddDisposable(sub);
            {
                var it = reactCollection.ToReadOnlyReactiveProperty().Value.GetEnumerator();
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
                    ctrl.AddToPanel(u);
                }
            }


        }
        /*
        //未实现 当页面切换时 自动重新填充list里面的信息 暂时未实现 有很多使用Tab分页的时候再做 由selectedPage作为驱动 去切换页面数据和Tab选中状态
        [System.Obsolete]
        public void ObservePage<T>(string pageController, IntReactiveProperty selectedPage, IList<IReactiveCollection<T>> pageData)
        {
            var g = gObject;
            var sub = selectedPage.Subscribe((idx) =>
            {
                var pageCtrl = g.GetController(pageController);
                if (pageCtrl != null)
                {
                    pageCtrl.SetSelectedIndex(idx);
                }
            });


            uiBase.AddDisposable(sub);
        }


        //未实现 当页面切换时 自动重新填充list里面的信息 暂时未实现 有很多使用Tab分页的时候再做 由pageController作为驱动 去切换页面数据和Tab选中状态
        [System.Obsolete]
        public void ObservePageFromPageCtrl<T>(string pageController, IntReactiveProperty selectedPage, IList<IReactiveCollection<T>> pageData)
        {
            var g = gObject;
            var pageCtrl = g.GetController(pageController);
            if (pageCtrl != null)
            {
                pageCtrl.onChanged.Add(() =>
                {
                    selectedPage.Value = pageCtrl.selectedIndex;
                });
            }
        }
        */

        public void OnClickItem(System.Action<int> onClick)
        {
            var g = gObject;
            g.onClickItem.Add((item) =>
            {
                var childIdx = g.GetChildIndex(item.data as FairyGUI.GObject);
                if (onClick != null)
                {
                    onClick(childIdx);
                }
            });
        }

        private static void diffSyncList(IList<int> selections, IList<int> selectedItemsIdx)
        {
            //删除中不在被选中的选项
            for (int i = selectedItemsIdx.Count - 1; i >= 0; i--)
            {
                if (!selections.Contains(selectedItemsIdx[i]))
                {
                    selectedItemsIdx.Remove(selectedItemsIdx[i]);
                }
            }
            //添加新被选中的选项
            for (int i = 0, c = selections.Count; i < c; i++)
            {
                if (!selectedItemsIdx.Contains(selections[i]))
                {
                    selectedItemsIdx.Add(selections[i]);
                }
            }
        }

        //同步选项信息到viewmodel 多选  manualState手动设置按钮状态 单选或者复选按钮 点下去 再移开 状态还是选中状态 但是list无法获取
        //manual 需要添加selection 控制器
        public void SelectedItemsIdx(IReactiveCollection<int> selectedItemsIdx, bool manualState = true)
        {
            var g = gObject;
            g.onClickItem.Add((item) =>
            {
                var selections = g.GetSelection();
                diffSyncList(selections, selectedItemsIdx);
                if (manualState)
                {
                    for(int i = 0; i < g.numItems; i++)
                    {
                        var com = g.GetChildAt(i).asCom;
                        if (com != null)
                        {
                            var ctrl = com.GetController("selection");
                            if(ctrl != null)
                            {
                                ctrl.SetSelectedPage("up");
                            }
                        }
                    }
                    foreach (var idx in selectedItemsIdx)
                    {
                        var child = g.GetChildAt(g.ItemIndexToChildIndex(idx));
                        var com = g.GetChildAt(idx).asCom;
                        if (com != null)
                        {
                            var ctrl = child.asCom.GetController("selection");
                            if (ctrl != null)
                            {
                                ctrl.SetSelectedPage("down");
                            }
                        }
                    }
                }
                
            });
            {
                var selections = g.GetSelection();
                diffSyncList(selections, selectedItemsIdx);
            }
        }
        //同步选项信息到viewmodel 单选
        public void SelectedItemIdx(IReactiveProperty<int> selectedItemIdx)
        {
            var g = gObject;
            g.onClickItem.Add((item) =>
            {
                selectedItemIdx.Value = g.selectedIndex;
            });
        }

        public void OnClick(UniRx.ReactiveCommand cmd)
        {
            gObject.onClick.Add(() => {
                cmd.Execute();
                
            });
        }

        /// <summary>
        /// 绑定item被点击的事件
        /// </summary>
        /// <param name="cmd"></param>
        public void OnSelect(UniRx.ReactiveCommand<FairyGUI.EventContext> cmd)
        {
            var g = gObject;
            gObject.onClickItem.Add((ctx) => {
                cmd.Execute(ctx);
                
            });

        }

        /// <summary>
        /// 绑定item被点击事件 获取被点击物体index
        /// </summary>
        /// <param name="cmd"></param>
        public void OnSelectIndex(UniRx.ReactiveCommand<int> cmd)
        {
            var g = gObject;
            gObject.onClickItem.Add((ctx) => {
                var index = g.GetChildIndex(g.touchItem);
                cmd.Execute(index);
            });
        }

        public void SetSelectIndex(UniRx.IObservable<int> selectedIdx)
        {
            var g = gObject;
            var sub = selectedIdx.Subscribe((idx) =>
            {
                g.selectedIndex = idx;
            });
            uiBase.AddDisposable(sub);
        }

    }

}