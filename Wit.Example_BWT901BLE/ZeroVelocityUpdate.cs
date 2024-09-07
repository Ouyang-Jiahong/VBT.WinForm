using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wit.Example_BWT901BLE
{
    internal class ZeroVelocityUpdate
    {
        // 静态常量，定义判断零速度所需的连续样本数阈值  
        public const int StaticSamplesThreshold = 10;
        // 静态常量，定义加速度的阈值，超过此值则认为非零速度  
        public const double StaticAccelerationThreshold = 0.35;
        // 静态常量，定义角速度的阈值，超过此值则认为非零速度  
        public const double StaticAngularVelocityThreshold = 0.15;

        private int samplesCount; // 记录连续符合零速度条件的样本数  

        // 构造函数，初始化samplesCount为0  
        public ZeroVelocityUpdate()
        {
            samplesCount = 0;
        }

        // Update方法，用来判断是否达到了零速度状态  
        // 参数acceleration和angularVelocity分别传入加速度和角速度数组  
        public bool Update(double[] acceleration, double[] angularVelocity)
        {
            // 检查输入的加速度和角速度数组长度是否都至少为3  
            if (acceleration.Length < 3 || angularVelocity.Length < 3)
            {
                return false; // 数组长度不足，返回false  
            }

            // 判断加速度和角速度的各分量是否都小于或等于阈值  
            if (Math.Abs(acceleration[0]) <= StaticAccelerationThreshold &&
                Math.Abs(acceleration[1]) <= StaticAccelerationThreshold &&
                Math.Abs(acceleration[2]) <= StaticAccelerationThreshold &&
                Math.Abs(angularVelocity[0]) <= StaticAngularVelocityThreshold &&
                Math.Abs(angularVelocity[1]) <= StaticAngularVelocityThreshold &&
                Math.Abs(angularVelocity[2]) <= StaticAngularVelocityThreshold)
            {
                samplesCount++; // 都小于或等于阈值，样本计数加一  
            }
            else
            {
                samplesCount = 0; // 否则，重置样本计数  
            }

            // 返回是否达到了连续样本数的阈值，即是否判定为零速度状态  
            return samplesCount >= StaticSamplesThreshold;
        }
    }
}
