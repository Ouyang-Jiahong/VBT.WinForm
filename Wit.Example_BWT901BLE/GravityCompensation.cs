using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wit.Example_BWT901BLE
{
    internal class GravityCompensation
    {
        public const double GRAVITY = 9.7915; // 定义湖南省长沙市的重力加速度，单位为 m/s^2  

        // 通过四元数旋转重力向量并进行加速度补偿  
        // acc: 传感器测量的原始加速度向量  
        // q: 表示旋转的四元数  
        public static double[] CompensateGravity(double[] acc, double[] q)
        {
            // 定义地球坐标系中的重力向量，方向竖直向下  
            double[] gravityEarth = { 0.0, 0.0, -GRAVITY };

            // 将重力向量从地球坐标系旋转到传感器坐标系
            double[] gravitySensor = RotateVector(gravityEarth, q);

            // 从测量的加速度中减去重力影响，得到补偿后的加速度  
            return new double[] { acc[0] - gravitySensor[0], acc[1] - gravitySensor[1], acc[2] - gravitySensor[2] };
        }

        // 使用四元数旋转一个向量  
        // v: 要进行旋转的向量  
        // q: 表示旋转的四元数  
        private static double[] RotateVector(double[] v, double[] q)
        {
            // 提取四元数的各个分量  
            double qw = q[0], qx = q[1], qy = q[2], qz = q[3];
            // 提取要旋转的向量的各个分量  
            double ix = v[0], iy = v[1], iz = v[2];

            // 定义交叉乘积的中间变量  
            double wx, wy, wz, xx, xy, xz, yy, yz, zz;

            // 计算交叉乘积结果  
            wx = 2 * (qy * iz - qz * iy);
            wy = 2 * (qz * ix - qx * iz);
            wz = 2 * (qx * iy - qy * ix);

            // 计算2倍四元数实部与向量的乘积  
            xx = 2 * qx * ix;
            xy = 2 * qx * iy;
            xz = 2 * qx * iz;

            yy = 2 * qy * iy;
            yz = 2 * qy * iz;
            zz = 2 * qz * iz;

            // 根据四元数旋转公式计算旋转后的向量  
            return new double[]
            {
            ix + wx * qw + (xy - wz * qw),  // 旋转后向量的X分量  
            iy + wy * qw + (xz + wy * qw),  // 旋转后向量的Y分量  
            iz + wz * qw + (yz - wx * qw)  // 旋转后向量的Z分量  
            };
        }
    }
}
