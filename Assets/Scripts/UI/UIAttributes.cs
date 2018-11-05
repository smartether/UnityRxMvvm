using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
    public class UIAttributes
    {
        /**
         * 标记导出组件的包ID和名称
         */
        [System.AttributeUsage(System.AttributeTargets.All)]
        public class UIPackageIDAttribute : System.Attribute{
            public UIPackageIDAttribute(string id, string name = ""){
                ID = id;
                Name = name;
            }
            public string ID;
            public string Name;
        }

        /**
         * 标记组件的id和类型
         */
        [System.AttributeUsage(System.AttributeTargets.All)]
        public class UIPackageItemIDAttribute : System.Attribute{
            public UIPackageItemIDAttribute(string id, System.Type t)
            {
                ID = id;
                type = t;
            }
            public string ID;
            public System.Type type;
        }

        /**
         * 标记组件元素的ID
         */
         [System.AttributeUsage(System.AttributeTargets.Property)]
        public class UIDisplayItemIDAttribute: System.Attribute{
            public UIDisplayItemIDAttribute(string id)
            {
#if UNITY_EDITOR
                ID = id;
#endif

            }
#if UNITY_EDITOR
            public string ID;
#endif
        }


        [System.AttributeUsage(System.AttributeTargets.All)]
        public class UIPackageItemExtensionInfoAttribute : System.Attribute{
            
        }

        /*
         * 为组件元素生成unirx代理的配置
         */
        [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true)]
        public class UIBindTypeInfoAttribute : System.Attribute{
            public UIBindTypeInfoAttribute(System.Type type){
                BindType = type;
            }
            public System.Type BindType;
        }


        /**
         * 生成绑定代码时需要，暂时未实现
         */
        //use CallerMemberNameAttribute if .net 4.5, string otherwise.
        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class UIBindPropertyInfoAttribute : System.Attribute{
            //生成绑定代码用途 响应式绑定或者简单赋值
            public enum EBindMethod
            {
                Normal, //直接赋值
                React   //响应式
            }
            public UIBindPropertyInfoAttribute(System.Type type, string propertyName, int index = 0, EBindMethod BindMethod = EBindMethod.React)
            {
                this.Index = index;
                this.BindMethod = BindMethod;
                BindPropertyType = type;
                BindPropertyName = propertyName;

            }
            public UIBindPropertyInfoAttribute(string propertyName)
            {
                BindMethod = EBindMethod.Normal;
                BindPropertyType = null;
                BindPropertyName = propertyName;
            }
            public EBindMethod BindMethod;
            public System.Type BindPropertyType;
            public string BindPropertyName;
            //相同名称属性的绑定方法需要指定某一个
            public int Index;
        }

        /**
         * 通过ViewModel创建对应UI的注解，用于分离其他代码对ctrl的引用
         */
        [System.AttributeUsage(System.AttributeTargets.Class)]
        public class TargetCtrlAttribute : System.Attribute
        {
            public TargetCtrlAttribute(System.Type type)
            {
                this.Type = type;
            }

            public TargetCtrlAttribute(string typeName)
            {
                this.TypeName = typeName;
            }
            //Type
            public System.Type Type;
            //Type FullName
            public string TypeName;
        }
    }
}