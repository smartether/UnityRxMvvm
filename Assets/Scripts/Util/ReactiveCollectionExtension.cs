using System;
using System.Collections.Generic;
using System.Text;

namespace UniRx_Ext
{
    using UniRx;
    public static class ReactiveCollectionExtension
    {
        public struct CollectionChangedEvent<T>
        {
            public enum ChangedAction
            {
                None,
                Normal,     //Add，Remove，Move，Replace，Reset
                Add,
                Remove,
                Move,
                Replace,
                Reset,
                CountChanged,
            }
            public int Index { get; private set; }
            public T Value { get; private set; }
            public ChangedAction Action{ get; private set;}

            public CollectionChangedEvent(int index, T value, ChangedAction action)
            {
                Index = index;
                Value = value;
                this.Action = action;
            }
        }

        //未实现 
        [System.Obsolete]
        public static UniRx.Subject<CollectionChangedEvent<T>> ObserveAll<T>(this ReactiveCollection<T> reactCollection)
        {
            reactCollection.ObserveAdd().CombineLatest(reactCollection.ObserveRemove(), reactCollection.ObserveMove(), reactCollection.ObserveReplace(), reactCollection.ObserveReset(), reactCollection.ObserveCountChanged(),
                (add, remove, move, replace, reset, countChanged) =>
                {
                    return new CollectionChangedEvent<T>(0,add.Value, CollectionChangedEvent<T>.ChangedAction.Add);
                });
            return new UniRx.Subject<CollectionChangedEvent<T>>();
        }

        //用于重新搜索全表
        public static UniRx.IObservable<IReactiveCollection<T>> ObserveItemChanged<T>(this IReactiveCollection<T> reactCollection)
        {
             return reactCollection.ObserveAdd().CombineLatest(reactCollection.ObserveRemove(), reactCollection.ObserveMove(), reactCollection.ObserveReplace(), reactCollection.ObserveReset(),
                (add, remove, move, replace, reset) =>
                {
                    return reactCollection;
                });
        }
    }
}
