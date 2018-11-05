using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    public interface IModel
    {

    }

    public interface IViewModel :  IModel
    {
        
    }

    /// <summary>
    /// 一般用户在一个列表容器中创建不同样式的item 指定view类型用的
    /// </summary>
    public interface IViewModelCtrl : IViewModel
    {
        System.Type CtrlType { get; }
    }

    /// <summary>
    /// 用户初始化界面元素基本表现 位置 大小 颜色 等， 都在这里扩展, 在对应的BindDefine中为对应控件实现
    /// </summary>
    public interface IViewModelStyle
    {
        UnityEngine.Vector3 LocalPosition { get; }
    }

}
