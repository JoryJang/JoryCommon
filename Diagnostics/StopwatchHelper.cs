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

        public static double Measure(Action action)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                sw.Stop(); // 无论业务代码是否报错，都确保秒表停下来
            }
            return sw.Elapsed.TotalMilliseconds; // 返回 double，保留小数，避免极快操作显示 0ms
        }
    }
}
