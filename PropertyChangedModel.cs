using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jory.Common
{
    public class PropertyChangedModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变的事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///  触发属性变更通知
        /// </summary>
        /// <param name="propertyName"></param>
        [Obsolete("请统一使用 OnPropertyChanged")]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
           OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// 仅当值不同时才赋值并通知
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }



        /// <summary>
        /// 提取属性名称。4.5及之后的才可以用
        /// </summary>
        /// <returns></returns>
        protected string ExtractCallMemberName()
        {
            StackFrame frame = new StackFrame(2);
            string name = frame.GetMethod().Name;
            if (name.Contains("set_"))
            {
                name = name.Replace("set_", "");
            }
            return name;
        }
    }
}
