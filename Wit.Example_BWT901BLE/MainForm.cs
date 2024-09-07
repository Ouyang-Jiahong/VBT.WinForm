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
    /// </summary>
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent(); // 初始化窗体上的所有控件。  
        }

        /// <summary>
        /// 蓝牙管理器实例，用于管理蓝牙设备的搜索、连接和断开等操作。
        /// </summary>
        private IWinBlueManager WitBluetoothManager = WinBlueFactory.GetInstance();

        /// <summary>
        /// 存储已发现的BWT901BLE蓝牙设备及其对应的连接实例的字典。  
        /// 键为设备的MAC地址，值为对应的Bwt901ble实例。
        /// </summary>
        private Dictionary<string, Bwt901ble> FoundDeviceDict = new Dictionary<string, Bwt901ble>();

        /// <summary>
        /// 控制自动刷新数据线程是否工作
        /// </summary>
        public bool EnableRefreshDataTh { get; private set; }

        /// <summary>
        /// 当窗体加载时触发的事件处理程序。  
        /// 功能：
        ///     1. 初始化传感器设置；
        ///     2. 启动一个后台线程用于持续刷新并显示传感器数据。 
        /// </summary>
        private void MainFormLoad(object sender, EventArgs e)
        {
            // 初始化传感器设置
            InitializeDevices();

            // 启用数据刷新标志
            EnableRefreshDataTh = true;

            // 开启数据刷新线程，用于在一个富文本框内以 10Hz 的频率刷新显示传感器数据
            Thread dataRefreshThread = new Thread(RefreshDataTh);
            dataRefreshThread.IsBackground = true;
            dataRefreshThread.Start();
        }

        /// <summary>
        /// 当窗体关闭时触发的事件处理程序。
        /// </summary>
        private void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭刷新数据线程
            EnableRefreshDataTh = false;
            // 关闭蓝牙搜索
            stopScanButton_Click(null, null);
            // 终止当前进程
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 停止搜索
        /// </summary>
        private void stopScanButton_Click(object sender, EventArgs e)
        {
            // 让蓝牙管理器停止搜索
            WitBluetoothManager.StopScan();
        }

        /// <summary>
        /// 清除找到的设备，关闭之前打开的设备，并开始搜索设备
        /// </summary>
        private void StartScanButton_Click(object sender, EventArgs e)
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
        private void WitBluetoothManager_OnDeviceFound(string mac, string deviceName)
        {
            // 名称过滤
            if (deviceName != null && deviceName.Contains("WT"))
            {
                if (!FoundDeviceDict.ContainsKey(mac))
                {
                    Bwt901ble bWT901BLE = new Bwt901ble(mac, deviceName);
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
        private void BWT901BLE_OnRecord(Bwt901ble BWT901BLE)
        {
            // 在这里存储设备数据
            string text = GetDeviceData(BWT901BLE);
        }

        /// <summary>
        /// 设备状态发生时会调这个方法
        /// </summary>
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
        /// 数据刷新线程，用于在一个富文本框内以 10Hz 的频率刷新显示传感器数据
        /// 如果没有找到设备，这个线程也在一直运行，只不过采不到数据
        /// </summary>
        private void RefreshDataTh()
        {
            while (EnableRefreshDataTh)
            {
                /// <debug>
                /// 展示设备采集的数据
                /// </debug>
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
        /// 初始化所有已连接的设备，设置数据回传速率为200Hz，带宽为256Hz。  
        /// </summary>  
        private void InitializeDevices()
        {
            foreach (var device in FoundDeviceDict.Values)
            {
                if (!device.IsOpen())
                {
                    continue; // 如果设备未打开，则跳过  
                }

                try
                {
                    // 解锁寄存器  
                    device.UnlockReg();

                    // 设置数据回传速率为200Hz  
                    device.SetReturnRate(0x0A);

                    // 设置带宽为256Hz
                    device.SetBandWidth(0x00);
                }
                catch (Exception ex)
                {
                    // 如果在设置过程中捕获到异常，则显示一个消息框来通知用户错误信息  
                    MessageBox.Show($"初始化设备时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获得设备的数据
        /// 依次为
        ///     1. AccX
        ///     2. AccY
        ///     3. AccZ
        ///     4. GyroX
        ///     5. GyroY
        ///     6. GyroZ
        ///     7. AngleX
        ///     8. AngleY
        ///     9. AngleZ
        ///     10. MagX
        ///     11. MagY
        ///     12. MagZ
        ///     13. AccM
        ///     14. AsM
        ///     15. Q0
        ///     16. Q1
        ///     17. Q2
        ///     18. Q3
        /// </summary>
        private string GetDeviceData(Bwt901ble BWT901BLE)
        {
            StringBuilder builder = new StringBuilder();
            // 设备名称
            builder.Append(BWT901BLE.GetDeviceName()).Append(" ");
            // 加速度
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AccX)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AccY)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AccZ)).Append(" ");
            // 角速度
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AsX)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AsY)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AsZ)).Append(" ");
            // 角度
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleX)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleY)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleZ)).Append(" ");
            // 磁场
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.HX)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.HY)).Append(" ");
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.HZ)).Append(" ");
            // 加速度矢量和
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AccM)).Append(" ");
            // 角速度矢量和
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.AsM)).Append(" ");
            // 四 元 数 0
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.Q0)).Append(" ");
            // 四 元 数 1
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.Q1)).Append(" ");
            // 四 元 数 2
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.Q2)).Append(" ");
            // 四 元 数 3
            builder.Append(BWT901BLE.GetDeviceData(WitSensorKey.Q3)).Append(" ");
            
            return builder.ToString();
        }

        /// <summary>
        /// 加计校准
        /// </summary>
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
        /// 开始磁场校准
        /// </summary>
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

        private void StartTraining(object sender, EventArgs e)
        {
            // 创建变量
            double vx = 0;
            double vy = 0;
            double vz = 0;
            double px = 0;
            double py = 0;
            double pz = 0;
            double AccX = 0;
            double AccY = 0;
            double AccZ = 0;
            double GyroX = 0;
            double GyroY = 0;
            double GyroZ = 0;
            double AngleX = 0;
            double AngleY = 0;
            double AngleZ = 0;
            double MagX = 0;
            double MagY = 0;
            double MagZ = 0;
            double AccM = 0;
            double AsM = 0;
            double Q0 = 0; 
            double Q1 = 0;
            double Q2 = 0;
            double Q3 = 0;

            while (true)
            {
                // 获取数据字符串，并将数据分解存入数组
                // 依次为
                //     1. AccX
                //     2. AccY
                //     3. AccZ
                //     4. GyroX
                //     5. GyroY
                //     6. GyroZ
                //     7. AngleX
                //     8. AngleY
                //     9. AngleZ
                //     10. MagX
                //     11. MagY
                //     12. MagZ
                //     13. AccM
                //     14. AsM
                //     15. Q0
                //     16. Q1
                //     17. Q2
                //     18. Q3
                string DeviceData = "";
                Thread.Sleep(5);
                // 刷新所有连接设备的数据
                for (int i = 0; i < FoundDeviceDict.Count; i++)
                {
                    var keyValue = FoundDeviceDict.ElementAt(i);
                    Bwt901ble bWT901BLE = keyValue.Value;
                    if (bWT901BLE.IsOpen())
                    {
                        DeviceData += GetDeviceData(bWT901BLE);
                    }
                }
                // 使用Split方法按照空格拆分字符串  
                string[] dataArray = DeviceData.Split(' ');

                // 速度更新循环

            }
        }
    }
}
