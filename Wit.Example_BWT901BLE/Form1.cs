using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Wit.SDK.Modular.Sensor.Modular.DataProcessor.Constant;
using Wit.SDK.Modular.WitSensorApi.Modular.BWT901BLE;
using Wit.SDK.Device.Device.Device.DKey;
using Wit.Bluetooth.WinBlue.Utils;
using Wit.Bluetooth.WinBlue.Interface;

namespace Wit.Example_BWT901BLE
{   
    /// <summary>
    /// 程序主窗口
    /// 说明：
    /// 1.本程序是维特智能开发的BWT901BLE九轴传感器示例程序
    /// 2.适用示例程序前请咨询技术支持,询问本示例程序是否支持您的传感器
    /// 3.使用前请了解传感器的通信协议
    /// 4.本程序只有一个窗口,所有逻辑都在这里
    /// </summary>
    public partial class Form1 : Form
    {

        /// <summary>
        /// 蓝牙管理器
        /// </summary>
        private IWinBlueManager WitBluetoothManager = WinBlueFactory.GetInstance();

        /// <summary>
        /// 找到的设备
        /// </summary>
        private Dictionary<string, Bwt901ble> FoundDeviceDict = new Dictionary<string, Bwt901ble>();

        /// <summary>
        /// 控制自动刷新数据线程是否工作
        /// </summary>
        public bool EnableRefreshDataTh { get; private set; }

        /// <summary>
        /// 构造器
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // 设置传感器回传速率

            // 设置传感器带宽

            // 开启数据刷新线程
            Thread thread = new Thread(RefreshDataTh);
            thread.IsBackground = true;
            EnableRefreshDataTh = true;
            thread.Start();
        }

        /// <summary>
        /// 窗体关闭时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭刷新数据线程
            EnableRefreshDataTh = false;
            // 关闭蓝牙搜索
            stopScanButton_Click(null, null);
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 开始搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startScanButton_Click(object sender, EventArgs e)
        {
            // 清除找到的设备
            FoundDeviceDict.Clear();

            // 关闭之前打开的设备
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;
                bWT901BLE.Close();
            }

            WitBluetoothManager.OnDeviceFound += this.WitBluetoothManager_OnDeviceFound;
            WitBluetoothManager.StartScan();
        }

        /// <summary>
        /// 当搜索到蓝牙设备时会回调这个方法
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="deviceName"></param>
        private void WitBluetoothManager_OnDeviceFound(string mac, string deviceName)
        {
            // 名称过滤
            if (deviceName != null && deviceName.Contains("WT"))
            {
                if (!FoundDeviceDict.ContainsKey(mac))
                {
                    Bwt901ble bWT901BLE = new Bwt901ble(mac,deviceName);
                    FoundDeviceDict.Add(mac, bWT901BLE);
                    // 打开这个设备
                    bWT901BLE.Open();
                    bWT901BLE.OnRecord += BWT901BLE_OnRecord;
                }
            }
        }

        /// <summary>
        /// 当传感器数据刷新时会调用这里，您可以在这里记录数据
        /// </summary>
        /// <param name="BWT901BLE"></param>
        private void BWT901BLE_OnRecord(Bwt901ble BWT901BLE)
        {
            string text = GetDeviceData(BWT901BLE);
            Debug.WriteLine(text);
        }

        /// <summary>
        /// 停止搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopScanButton_Click(object sender, EventArgs e)
        {
            // 让蓝牙管理器停止搜索
            WitBluetoothManager.StopScan();
        }

        /// <summary>
        /// 设备状态发生时会调这个方法
        /// </summary>
        /// <param name="macAddr"></param>
        /// <param name="mType"></param>
        /// <param name="sMsg"></param>
        private void OnDeviceStatu(string macAddr, int mType, string sMsg)
        {
            if (mType == 20)
            {
                // 断开连接
                Debug.WriteLine(macAddr + "Disconnect");
            }

            if (mType == 11)
            {
                // 连接失败
                Debug.WriteLine(macAddr + "Connect failed");
            }

            if (mType == 10)
            {
                // 连接成功
                Debug.WriteLine(macAddr + "Successfully connected");
            }
        }

        /// <summary>
        /// 刷新数据线程
        /// Refresh Data Thread
        /// </summary>
        private void RefreshDataTh()
        {
            while (EnableRefreshDataTh)
            {
                // 多设备的展示数据
                string DeviceData = "";
                Thread.Sleep(100);
                // 刷新所有连接设备的数据
                for (int i = 0; i < FoundDeviceDict.Count; i++)
                {
                    var keyValue = FoundDeviceDict.ElementAt(i);
                    Bwt901ble bWT901BLE = keyValue.Value;
                    if (bWT901BLE.IsOpen())
                    {
                        DeviceData += GetDeviceData(bWT901BLE) + "\r\n";
                    }
                }
                DebugRichTextBox.Invoke(new Action(() =>
                {
                    DebugRichTextBox.Text = DeviceData;
                }));
            }
        }

        /// <summary>
        /// 获得设备的数据
        /// </summary>
        private string GetDeviceData(Bwt901ble BWT901BLE)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(BWT901BLE.GetDeviceName()).Append("\n");
            // 加速度
            // Acc
            builder.Append("AccX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AccX)).Append("g \t");
            builder.Append("AccY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AccY)).Append("g \t");
            builder.Append("AccZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AccZ)).Append("g \n");
            // 角速度
            // Gyro
            builder.Append("GyroX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AsX)).Append("°/s \t");
            builder.Append("GyroY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AsY)).Append("°/s \t");
            builder.Append("GyroZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AsZ)).Append("°/s \n");
            // 角度
            // Angle
            builder.Append("AngleX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleX)).Append("° \t");
            builder.Append("AngleY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleY)).Append("° \t");
            builder.Append("AngleZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleZ)).Append("° \n");
            // 磁场
            // Mag
            builder.Append("MagX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.HX)).Append("uT \t");
            builder.Append("MagY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.HY)).Append("uT \t");
            builder.Append("MagZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.HZ)).Append("uT \n");
            // 版本号
            // VersionNumber
            builder.Append("VersionNumber").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.VersionNumber)).Append("\n");
            return builder.ToString();
        }

        /// <summary>
        /// 加计校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void appliedCalibrationButton_Click(object sender, EventArgs e)
        {
            // 所有连接的蓝牙设备都加计校准
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }

                try
                {
                    // 解锁寄存器并发送命令
                    bWT901BLE.UnlockReg();
                    bWT901BLE.AppliedCalibration();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 设置回传速率200Hz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void returnRate200_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }
                try
                {
                    // 解锁寄存器并发送命令
                    bWT901BLE.UnlockReg();
                    bWT901BLE.SetReturnRate(0x0A);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 设置带宽256Hz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bandWidth256_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }
                try
                {
                    // 解锁寄存器并发送命令
                    bWT901BLE.UnlockReg();
                    bWT901BLE.SetBandWidth(0x00);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 开始磁场校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startFieldCalibrationButton_Click(object sender, EventArgs e)
        {
            // 开始所有连接的蓝牙设备的磁场校准
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }
                try
                {
                    // 解锁寄存器并发送命令
                    bWT901BLE.UnlockReg();
                    bWT901BLE.StartFieldCalibration();
                    MessageBox.Show("开始磁场校准,请绕传感器XYZ三轴各转一圈,转完以后点击【结束磁场校准】");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 结束磁场校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void endFieldCalibrationButton_Click(object sender, EventArgs e)
        {

            // 结束所有连接的蓝牙设备的磁场校准
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }
                try
                {
                    // 解锁寄存器并发送命令
                    bWT901BLE.UnlockReg();
                    bWT901BLE.EndFieldCalibration();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 事件处理器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void SensorCalibrationButton_Click(object sender, EventArgs e)
        {
            CalibrationForm calibrationForm = new CalibrationForm();
            calibrationForm.ShowDialog();
        }
    }
}
