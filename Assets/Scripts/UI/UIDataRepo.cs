using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UIFrame
{
    /// <summary>
    /// 每个panel(或者某个大界面的controller）都需要单独建立一个viewmodel
    /// 创建一个panel 管理器会从仓库中获取viewmodel注入到panel中
    /// 子节点的controller想要获取上级界面的viewmodel可以通过仓库获取
    /// </summary>
    public class UIDataRepo : QFramework.QSingleton<UIDataRepo>
    {
        private UIDataRepo()
        {

        }

        /// <summary>
        /// 不同ViewModel类型各自的实例
        /// </summary>
        private readonly Dictionary<System.Type, List<IViewModel>> TypedViewModels = new Dictionary<System.Type, List<IViewModel>>();
        /// <summary>
        /// 每个Panel模块对应的一个ViewModel
        /// </summary>
        //最近一个特点类型的 ViewModels 如果有多个实例存放在TypedViewModels中
        private readonly Dictionary<System.Type, IViewModel> ViewModels = new Dictionary<System.Type, IViewModel>();

        private readonly Dictionary<System.Type, List<System.Reflection.FieldInfo>> cachedFieldInfo = new Dictionary<System.Type, List<System.Reflection.FieldInfo>>();


        /// <summary>
        /// 注册viewmodel对应一个panel类型
        /// </summary>
        /// <typeparam name="T1">panel类型</typeparam>
        /// <param name="vm">viewmodel</param>
        public void RegistViewModel(System.Type type, IViewModel vm)// where T: IViewModel
        {
            Debug.Log("$$ registViewModel:" + type.FullName);
            //if (!ViewModels.ContainsKey(type))
            //{
            //    ViewModels[type] = vm;
            //}
            ViewModels[type] = vm;
            var vmType = vm.GetType();
            if(!TypedViewModels.ContainsKey(vmType))
            {
                TypedViewModels[vmType] = new List<IViewModel>();
            }
            TypedViewModels[vmType].Add(vm);
        }

        public void UnregistViewModel<T1>(IViewModel vm)
        {

            var type = typeof(T1);
            UnregistViewModel(type, vm);
        }

        public void UnregistViewModel(System.Type type, IViewModel vm)
        {
            if (ViewModels.ContainsKey(type) && ViewModels[type] == vm)
            {
                var dis = ViewModels[type] as System.IDisposable;
                if (dis != null)
                {
                    dis.Dispose();
                }
                else
                {
                    ViewModels[type].AutoDispose(ViewModels[type].GetType());
                }
                ViewModels.Remove(type);
            }

            var vmType = vm.GetType();
            if (TypedViewModels.ContainsKey(vmType) && TypedViewModels[vmType] != null)
            {
                //for (int i = 0; i < TypedViewModels[vmType].Count; i++)
                if(TypedViewModels[vmType].Contains(vm))
                {
                    var typedViewModel = TypedViewModels[vmType].Find((cell)=> cell == vm);
                    var dis = typedViewModel as System.IDisposable;
                    if (dis != null)
                    {
                        dis.Dispose();
                        
                    }
                    else
                    {
                        typedViewModel.AutoDispose(typedViewModel.GetType());
                    }
                }
                TypedViewModels[vmType].Remove(vm);
                if (TypedViewModels[vmType].Count == 0)
                    TypedViewModels.Remove(vmType);
            }
        }

        /// <summary>
        /// 获取一个指定类型panel的viewmodel 并且指定viewmodl类型
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //public T GetViewModelWithPanelType<T1, T>() where T : class, IViewModel
        //{
        //    var type = typeof(T1);
        //    if (ViewModels.ContainsKey(type))
        //    {
        //        return ViewModels[type] as T;
        //    }
        //    return default(T);
        //}

        /// <summary>
        ///获取一个指定类型panel的viewmodel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IViewModel GetViewModelWithPanelType<T>(bool autoGenerate = false, UIBase ctrl = null) where T : UIBase
        {
            var type = typeof(T);
            return GetViewModelWithPanelType(type, autoGenerate, ctrl);
        }


        /// <summary>
        ///获取一个指定类型panel的viewmodel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IViewModel GetViewModelWithPanelType(System.Type type, bool autoGenerate = false, UIBase ctrl = null)
        {
            if (ViewModels.ContainsKey(type))
            {
                return ViewModels[type];
            }
            else if (autoGenerate)
            {
                var mainViewModel = System.Activator.CreateInstance(ctrl.MainViewModelType);
                RegistViewModel(type,mainViewModel as IViewModel);
                return mainViewModel as IViewModel;
            }
            return default(IViewModel);
        }


        /// <summary>
        /// 获取一个panel级别的viewmodel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetViewModel<T>() where T : class, IViewModel
        {
            var type = typeof(T);
            return (T)GetViewModel(type);
        }

        /// <summary>
        /// 获取一个panel级别的viewmodel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetViewModels<T>() where T : class, IViewModel
        {
            var type = typeof(T);
             List<T> list = new List<T>();
            var res = GetViewModels(type);
            if (res != null)
            {
                res.ForEach((cell) =>
                {
                    list.Add((T)cell);
                });
            }
            return list;
        }

        /// <summary>
        /// 获取一个panel级别的viewmodel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IViewModel GetViewModel(System.Type type)
        {
            if (TypedViewModels.ContainsKey(type))
            {
                return TypedViewModels[type][0];
            }
            return null;
        }

        /// <summary>
        /// 获取一个panel级别的viewmodel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<IViewModel> GetViewModels(System.Type type)
        {
            if (TypedViewModels.ContainsKey(type))
            {
                return TypedViewModels[type];
            }
            return null;
        }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();

            //创建和添加模块Controller的ViewModel

        }
    }

}