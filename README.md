### 1. 程序设计方案

#### 1.1 数据处理模块算法说明

![image-20240816103335147](.\img\image-20240816103335147.png)

**算法 1: 重力加速度旋转和扣除算法**

- **输入**：IMU传感器的加速度a(t)、传感器坐标系旋转到东北天坐标系的欧拉角Roll（滚转角x轴）,Pitch（俯仰角y轴）, Yaw（偏航角z轴）（旋转顺序为：Z-Y-X）旋转矩阵为

  ![image-20240816105633198](.\img\image-20240816105633198.png)

- **输出**：东北天坐标系下，减去了重力加速度的动态加速度a(t)-g

- **算法步骤**：
  - 将加速度数据从传感器坐标系转换到东北天坐标系。
  - 在东北天坐标系下，将加速度减去重力加速度的分量，得到动态加速度。

**算法 2：运动检测算法**

- 输入：角速度omega(t)，减去了重力加速度的动态加速度a(t)-g

- 输出：判定标记d（如果在运动，则d=1；否则d=0）

- 算法步骤：
  - 待补充

**算法 3: 加速度积分器**

- 输入:减去了重力加速度的动态加速度a(t)-g
- 输出:存在漂移的速度v(t)

- 算法步骤：
  - 待补充

**算法 4:零速度更新(ZVU)算法**

- **输入**：判定标记d、存在漂移的速度v(t)、角速度omega(t)
- 输出：精确的速度v(t)
- 算法步骤：（判定标准有两个，一个是加速度导出值，一个是角速度导出值。是否需要用到角速度导出值还不知道，暂时先只用加速度导出值）
  - 在一定时间内检查加速度和角速度的阈值。
  - 确定设备静止时刻，并标记这些时刻的速度为零。

**算法 5: 速度积分器**

- **输入**：精确的速度v(t)
- 输出：精确的位移r
- **处理**：
  - 对速度数据积分得到位移。

#### 1.2 用户界面设计

- **主界面**：
  - 显示当前速度和位移的实时数据。
  - 图表显示速度-时间曲线和位移-时间曲线。
  - 按钮用于保存数据和配置蓝牙连接。

- **数据管理界面**：
  - 列表显示已保存的数据集。
  - 提供搜索和筛选功能。

- **蓝牙配置界面**：
  - 显示附近可发现的蓝牙设备。
  - 提供配对按钮和已配对设备列表。

#### 1.3 蓝牙连接模块

- **功能**：
  - 搜索附近的蓝牙设备。
  - 与指定蓝牙设备配对。
  - 通过蓝牙接收IMU传感器的数据。

- **实现**：
  - 使用.NET的`System.Device.Bluetooth`库进行蓝牙设备的搜索和连接。
  - 使用异步IO接收传感器数据。

#### 1.4 数据存储模块

- **存储介质**：
  - SQLite数据库用于存储传感器数据和应用程序配置。

- **功能**：
  - 保存和检索IMU传感器数据。
  - 维护用户配置（如蓝牙设备信息、数据保存路径等）。

- **实现**：
  - 使用Entity Framework Core或ADO.NET进行数据库操作。

### 2. 开发步骤

1. **设置开发环境**：安装Visual Studio，创建WinForms项目。
2. **实现数据处理模块**：编写算法转换坐标、扣除重力加速度、实现ZVU算法和积分计算。
3. **设计用户界面**：使用WinForms设计主界面和子界面。
4. **实现蓝牙连接模块**：编写蓝牙搜索、配对和接收数据的代码。
5. **实现数据存储模块**：设计数据库模型，实现数据存取逻辑。
6. **集成和测试**：集成所有模块，进行系统测试，确保各模块协同工作正常。

### 3. 注意事项

- **数据同步**：确保蓝牙接收到的数据能够实时或接近实时地处理并显示在UI上。
- **性能优化**：对大量数据处理时，考虑使用多线程或异步处理来提高应用程序的响应性和性能。
- **错误处理**：增加适当的错误处理逻辑，如蓝牙连接失败、数据解析错误等。
