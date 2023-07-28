using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SerialPortToNet
{
    /// <summary>
    /// 可观察到属性的对象的基类
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// 比较给定属性的当前值和新值，如果值发生了变化，则引发<see cref="PropertyChanging"/>事件，更新完值后引发<see cref="PropertyChanged"/>事件。
        /// </summary>
        /// <typeparam name="T">更改的属性的类型</typeparam>
        /// <param name="field">存储属性值的字段</param>
        /// <param name="newValue">属性发生变化后的值</param>
        /// <param name="propertyName">(可选)被更改的属性的名称</param>
        /// <returns></returns>
        protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

            field = newValue;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            return true;
        }
    }
}
