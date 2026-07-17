using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jory.Common
{
    /// <summary>
    /// 定义视图模型的基类
    /// </summary>
    public abstract class ViewModelBase:INotifyPropertyChanged, IDisposable
    {
        #region 字段
        private Boolean m_disposed;
        #endregion

        #region 事件
        /// <summary>
        /// 属性值改变事件。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region 构造函数
        /// <summary>
        /// 实例化 ViewModelBase 类的新实例。
        /// </summary>
        protected ViewModelBase()
        {
        }
        #endregion

        #region 保护函数
        /// <summary>
        /// 释放资源时被调用。
        /// </summary>
        /// <param name="disposing">是否释放非托管资源。</param>
        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.m_disposed)
            {
                return;
            }
            if (disposing)
            {
            }
            this.m_disposed = true;
        }
        /// <summary>
        /// 引发 PropertyChanged 事件。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region 公共函数
        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 通知属性变化
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="entity"></param>
        public void NotifyPropertyChanged(String propertyName, Object entity)
        {
            if (PropertyChanged != null)
                PropertyChanged(entity, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
