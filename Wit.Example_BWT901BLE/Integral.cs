using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wit.Example_BWT901BLE
{
    internal class Integral
    {
        private double value; // 积分值  
        private double lastTimestamp; // 上次积分的时间戳  

        // Integral类的无参构造函数，用于初始化积分对象  
        // 将积分值（value）初始化为0，上次积分的时间戳（lastTimestamp）也初始化为0  
        public Integral()
        {
            value = 0;
            lastTimestamp = 0;
        }

        // 积分计算方法，用于计算并返回指定时间区间内的积分值  
        // 参数：  
        //   startTimestamp - 积分开始的时间戳  
        //   endTimestamp - 积分结束的时间戳  
        //   initialValue - 积分计算的初始值  
        //   input - 输入值（例如速度、加速度等，用于计算积分增量）  
        public double Integrate(double startTimestamp, double endTimestamp, double initialValue, double input)
        {
            double dt = endTimestamp - startTimestamp; // 计算时间区间长度（时间差）  

            // 检查是否需要重置积分值  
            // 如果积分开始时间戳与上次记录的时间戳不同，说明开始了一次新的积分过程，因此将积分值重置为初始值  
            if (startTimestamp != lastTimestamp)
            {
                value = initialValue;
            }

            // 根据输入值和时间差计算积分增量，并累加到当前积分值上  
            value += input * dt;

            // 更新上次积分的时间戳  
            lastTimestamp = endTimestamp;

            // 返回当前的积分值  
            return value;
        }

        // 重置积分状态的方法  
        // 将积分值（value）重置为0，同时将上次积分的时间戳（lastTimestamp）也重置为0  
        // 用于在开始新的积分过程之前清除旧的积分数据  
        public void Reset()
        {
            value = 0; // 积分值重置为0  
            lastTimestamp = 0; // 上次积分时间戳重置为0  
        }
    }
}
