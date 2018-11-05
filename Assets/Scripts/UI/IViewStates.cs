using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    using UniRx;
    public interface IViewStates
    {
        IReactiveProperty<bool> WillClose { get; set; }
        //IReactiveProperty<int> State { get; set; }
    }
}
