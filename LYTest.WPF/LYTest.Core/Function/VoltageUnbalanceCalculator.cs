using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Core.Function
{
    public class VoltageUnbalanceCalculator
    {
        public class VoltageUnbalanceFortmat
        {
            public VoltageUnbalanceFortmat(double _total, double _negative, double _zero)
            {
                total = _total;
                negative = _negative;
                zero = _zero;

            }
            /// <summary>
            /// 总
            /// </summary>
            public double total;
            /// <summary>
            /// 负序
            /// </summary>
            public double negative;
            /// <summary>
            ///零序
            /// </summary>
            public double zero;
        }
        /// <summary>
        /// 计算三相实际电压的不平衡度
        /// </summary>
        /// <param name="voltages">三相电压实际值（单位：V）</param>
        /// <param name="angles">三相电压角度（单位：度）</param>
        /// <param name="isLineVoltage">是否为线电压（默认false，按相电压处理）</param>
        public static VoltageUnbalanceFortmat Calculate(
            double[] voltages,
            double[] angles,
            bool isLineVoltage = false)
        {
            // 参数验证
            if (voltages == null || angles == null || voltages.Length != 3 || angles.Length != 3)
            {
                throw new ArgumentException("需要3个电压值和3个角度参数");
            }

            // 若输入为线电压，转换为相电压（假设星形连接）
            if (isLineVoltage)
            {
                for (int i = 0; i < 3; i++)
                {
                    voltages[i] /= Math.Sqrt(3); // 线电压转相电压
                }
            }

            // 生成三相电压复数
            Complex[] phases = new Complex[3];
            for (int i = 0; i < 3; i++)
            {
                double radians = angles[i] * Math.PI / 180;
                phases[i] = Complex.FromPolarCoordinates(voltages[i], radians);
            }

            // 定义旋转算子
            Complex a = Complex.FromPolarCoordinates(1, 120 * Math.PI / 180);
            Complex a2 = Complex.FromPolarCoordinates(1, 240 * Math.PI / 180);

            // 计算序分量
            Complex v1 = (phases[0] + a * phases[1] + a2 * phases[2]) / 3; // 正序
            Complex v2 = (phases[0] + a2 * phases[1] + a * phases[2]) / 3; // 负序
            Complex v0 = (phases[0] + phases[1] + phases[2]) / 3;          // 零序

            // 计算不平衡度
            double negativeUnbalance = (v2.Magnitude / v1.Magnitude) * 100;
            double zeroUnbalance = (v0.Magnitude / v1.Magnitude) * 100;
            double totalUnbalance = Math.Sqrt(Math.Pow(negativeUnbalance, 2) + Math.Pow(zeroUnbalance, 2));

            return new VoltageUnbalanceFortmat(totalUnbalance, negativeUnbalance, zeroUnbalance);
        }


        /// <summary>
        /// 计算三相电流不平衡度
        /// </summary>
        /// <param name="currents">电流幅值数组（单位：A）</param>
        /// <param name="angles">电流角度数组（单位：度）</param>
        /// <param name="isLineCurrent">是否为线电流（三角形接法需特殊处理）</param>
        /// <returns>(总不平衡度, 负序不平衡度, 零序不平衡度) 百分比</returns>
        public static VoltageUnbalanceFortmat CalculateCur(
            double[] currents,
            double[] angles,
            bool isLineCurrent = false)
        {
            // 参数验证
            if (currents == null || angles == null || currents.Length != 3 || angles.Length != 3)
            {
                throw new ArgumentException("需要3个电流值和3个角度参数");
            }

            // 生成三相电流复数
            Complex[] phases = new Complex[3];
            for (int i = 0; i < 3; i++)
            {
                double radians = angles[i] * Math.PI / 180;
                phases[i] = Complex.FromPolarCoordinates(currents[i], radians);
            }

            // 三角形接法线电流转相电流（需根据接线方式选择）
            if (isLineCurrent)
            {
                // 线电流转相电流公式：I_phase = I_line / √3 （假设对称系统）
                // 注意：非对称系统需要更复杂的转换，此处为简化处理
                for (int i = 0; i < 3; i++)
                {
                    phases[i] /= Math.Sqrt(3);
                }
            }

            // 定义旋转算子
            Complex a = Complex.FromPolarCoordinates(1, 120 * Math.PI / 180);
            Complex a2 = Complex.FromPolarCoordinates(1, 240 * Math.PI / 180);

            // 计算序分量
            Complex i1 = (phases[0] + a * phases[1] + a2 * phases[2]) / 3;  // 正序
            Complex i2 = (phases[0] + a2 * phases[1] + a * phases[2]) / 3;  // 负序
            Complex i0 = (phases[0] + phases[1] + phases[2]) / 3;            // 零序

            // 计算不平衡度百分比
            double negativeUnbalance = (i2.Magnitude / i1.Magnitude) * 100;
            double zeroUnbalance = (i0.Magnitude / i1.Magnitude) * 100;
            double totalUnbalance = Math.Sqrt(
                Math.Pow(negativeUnbalance, 2) +
                Math.Pow(zeroUnbalance, 2));

            //return (totalUnbalance, negativeUnbalance, zeroUnbalance);
            return new VoltageUnbalanceFortmat(totalUnbalance, negativeUnbalance, zeroUnbalance);

        }

    }
}
