using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class IViewModelExtension
{
    public static UIFrame.UIDataRepo GetDataRepo(this UIFrame.IViewModel viewmodel)
    {
        return UIFrame.UIDataRepo.Instance;
    }

    /// <summary>
    /// 扩展方法 从viewmodel中快捷获取用户信息
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    /*
    public static CMB.Models.UserData GetUserData(this UIFrame.IViewModel viewModel)
    {
        return Globals.Instance.UserData;
    }

    public static T GetConfig<T>(this UIFrame.IViewModel viewModel) where T : class, Google.Protobuf.IMessage
    {
        return DataConfigWrapper.GetConfigTable<T>();
    }

    public static List<T> GetTable<T>(this UIFrame.IViewModel viewmodel) where T : class, Google.Protobuf.IMessage
    {
        var prop = typeof(T).GetProperty("ConfigTables");
        if (prop != null)
        {
            var msg = DataConfigWrapper.GetConfigTable<T>();
            var propValue = prop.GetValue(msg, null);
            if (propValue != null)
            {
                var prop1 = propValue.GetType().GetProperty("Values");
                if (prop1 != null)
                {
                    var values = prop1.GetValue(propValue, null);
                    var typedValues = values as ICollection<T>;
                    if (typedValues != null)
                    {
                        List<T> list = new List<T>(typedValues);
                        return list;
                    }
                }
            }

        }
        return null;
    }
*/
    public static void AutoDispose<T>(this T viewModel) where T : UIFrame.IViewModel
    {
        AutoDispose(viewModel, typeof(T));
    }

    public static void AutoDispose(this UIFrame.IViewModel viewModel, System.Type type = null)
    {
        if (type == null)
            type = viewModel.GetType();
        var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(viewModel);
            if (fieldValue != null)
            {
                var fieldType = fieldValue.GetType();
                if (fieldValue != null && (field.FieldType.IsSubclassOf(typeof(System.IDisposable)) || fieldType.IsSubclassOf(typeof(System.IDisposable)) || fieldType.GetInterface("System.IDisposable") != null))
                {
                    var methods = fieldType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    System.Reflection.MethodInfo method = null;
                    for (int i = 0, c = methods.Length; i < c; i++)
                    {
                        if ("Dispose" == methods[i].Name && methods[i].GetParameters().Length == 0)
                            method = methods[i];
                    }
                    //Debug.Log("$$ will dispose field name:" + field.Name);
                    if (method != null)
                    {
                        method.Invoke(fieldValue, null);
                    }
                }
            }
        }
    }

    public static UniRx.ReactiveProperty<T> ToReact<T>(this T data) where T : class // UIFrame.IModel
    {
        var react = new UniRx.ReactiveProperty<T>(data);
        return react;
    }
}