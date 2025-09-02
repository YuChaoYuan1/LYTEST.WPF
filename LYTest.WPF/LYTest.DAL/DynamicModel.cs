using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace LYTest.DAL
{
    /// <summary>
    /// 动态属性类
    /// </summary>
    public class DynamicModel : DynamicObject
    {
        /// 存储在后台的数据源
        /// <summary>
        /// 存储在后台的数据源
        /// </summary>
        private Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();

        #region 重写get,set
        /// 重写Set方法,通过set方法访问时不会添加新的属性
        /// <summary>
        /// 重写Set方法,通过set方法访问时不会添加新的属性
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (propertyDictionary.Keys.Contains(binder.Name))
            {
                propertyDictionary[binder.Name] = value;
            }
            return true;
        }
        /// 重写get方法
        /// <summary>
        /// 重写get方法
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return propertyDictionary.TryGetValue(binder.Name, out result);
        }
        #endregion

        #region 用户访问
        /// 设置属性值,会添加新的属性
        /// <summary>
        /// 设置属性值,会添加新的属性
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetProperty(string propertyName, object value)
        {
            if (propertyDictionary.Keys.Contains(propertyName))
            {
                if (propertyDictionary[propertyName] != value)
                {
                    if (value is string)
                    {
                        propertyDictionary[propertyName] = value.ToString().Trim();
                    }
                    else
                    {
                        propertyDictionary[propertyName] = value;
                    }
                }
            }
            else
            {
                if (value is string)
                {
                    value = ((string)value).Trim();
                }
                propertyDictionary.Add(propertyName, value);
            }
        }
        /// 获取属性值
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetProperty(string propertyName)
        {
            if (propertyName == null) return null;
            if (propertyDictionary.Keys.Contains(propertyName))
            {
                object obj = propertyDictionary[propertyName];
                if (obj is string)
                {
                    obj = (obj as string).Trim();
                }
                return obj;
            }
            else
            {
                return 999;
            }
        }
        public void RemoveProperty(string propertyName)
        {
            if (propertyDictionary.ContainsKey(propertyName))
                propertyDictionary.Remove(propertyName);
        }
        #endregion

        /// 获取所有属性
        /// <summary>
        /// 获取所有属性
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllProperyName()
        {
            return propertyDictionary.Keys.ToList();
        }
    }
}
