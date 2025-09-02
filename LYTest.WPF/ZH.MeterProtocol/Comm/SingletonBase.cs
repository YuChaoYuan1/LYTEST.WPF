﻿using System;

namespace LYTest.MeterProtocol.Comm
{
    /// <summary>
    /// 要求有采用默认无参构造的单例基类
    /// </summary>
    /// <typeparam name="T">派生类类型</typeparam>
    public class SingletonBase<T>
        where T : new()
    {

        private static T instance;
        private static readonly object syncRoot = new object();
        /// <summary>
        /// 获取对象的单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }
                return instance;
            }
        }


    }
}
