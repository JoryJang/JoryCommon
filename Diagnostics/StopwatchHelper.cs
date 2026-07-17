using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jory.Common
{
    public class StopwatchHelper
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static long Execute(Action action)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            action();
            return sw.ElapsedMilliseconds;
        }
    }
}
