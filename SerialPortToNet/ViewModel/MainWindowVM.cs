using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using SerialPortToNet.Model;

namespace SerialPortToNet.ViewModel
{
    /// <summary>
    /// 主窗体数据源
    /// </summary>
    public class MainWindowVM : ObservableObject
    {
        /// <summary>
        /// 主窗口标题
        /// </summary>
        public string Title => $"网络转串口工具v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";

        #region 串口配置
        #region 选项
        /// <summary>
        /// 波特率选项
        /// </summary>
        public int[] BaudRateItems { get; set; } = new int[] { 4800, 9600, 19200, 56700, 115200 };

        /// <summary>
        /// 数据位选项
        /// </summary>
        public int[] DataBitItems { get; set; } = new int[] { 5, 6, 7, 8 };

        /// <summary>
        /// 校验位选项
        /// </summary>
        public Parity[] ParityItems { get; set; } = new Parity[] { Parity.None, Parity.Odd, Parity.Even, Parity.Mark };

        /// <summary>
        /// 停止位选项
        /// </summary>
        public string[] StopBitsItems { get; set; } = new string[] { "None", "1", "2", "1.5" };

        /// <summary>
        /// 串口列表
        /// </summary>
        public ObservableCollection<string> PortNameItems { get; set; } = new();
        #endregion

        #region 选中项
        /// <summary>
        /// 选中的串口选项索引
        /// </summary>
        public int CheckedPortNameIndex { get; set; }

        /// <summary>
        /// 选中的串口波特率
        /// </summary>
        public int CheckedBaudRate { get; set; } = 9600;

        /// <summary>
        /// 选中的串口数据位
        /// </summary>
        public int CheckedDataBit { get; set; } = 8;

        /// <summary>
        /// 选中的串口校验
        /// </summary>
        public Parity CheckedParity { get; set; } = Parity.None;

        /// <summary>
        /// 选中的串口停止位选项索引
        /// </summary>
        public int CheckedStopBitsIndex { get; set; } = 1;
        #endregion
        #endregion

        #region 网络配置
        /// <summary>
        /// 网络服务类型
        /// </summary>
        public NetworkMode[] NetworkModeItems { get; set; } = new NetworkMode[] { NetworkMode.TCP服务器, NetworkMode.TCP客户端 };

        /// <summary>
        /// 网络地址
        /// </summary>
        public string NetAddress
        {
            get => _netAddress;
            set => SetProperty(ref _netAddress, value);
        }
        private string _netAddress = "0.0.0.0";

        /// <summary>
        /// 网络服务端口
        /// </summary>
        public int NetPort { get; set; } = 60000;

        /// <summary>
        /// 选中的网络服务模式
        /// </summary>
        public NetworkMode CheckedNetworkMode { get; set; } = NetworkMode.TCP服务器;

        /// <summary>
        /// IP地址输入框使能
        /// </summary>
        public bool NetAddressIsEnable
        {
            get => _netAddressIsEnable;
            set => SetProperty(ref _netAddressIsEnable, value);
        }
        private bool _netAddressIsEnable = false;
        #endregion

        #region 数据展示框
        /// <summary>
        /// 数据编码模式选项
        /// </summary>
        public EncodingMode[] EncodingModeItems { get; set; } = new EncodingMode[] { EncodingMode.HEX, EncodingMode.ASCII, EncodingMode.UTF8, EncodingMode.GBK };

        /// <summary>
        /// 选中的网络->串口数据编码模式
        /// </summary>
        public EncodingMode CheckedNet2SPortEncodingMode { get; set; } = EncodingMode.HEX;

        /// <summary>
        /// 选中的串口->网络数据编码模式
        /// </summary>
        public EncodingMode CheckedSPort2NetEncodingMode { get; set; } = EncodingMode.HEX;

        /// <summary>
        /// 网络->串口是否自动换行
        /// </summary>
        public bool NetToSerialPortNewLine { get; set; } = false;

        /// <summary>
        /// 串口->网络是否自动换行
        /// </summary>
        public bool SerialPortToNetNewLine { get; set; } = false;

        /// <summary>
        /// 网络->串口的展示数据
        /// </summary>
        public string NetToSerialPortData
        {
            get => _netToSerialPortData;
            set => SetProperty(ref _netToSerialPortData, value);
        }
        private string _netToSerialPortData = string.Empty;

        /// <summary>
        /// 串口->网络的展示数据
        /// </summary>
        public string SerialPortToNetData
        {
            get => _serialPortToNetData;
            set => SetProperty(ref _serialPortToNetData, value);
        }
        private string _serialPortToNetData = string.Empty;
        #endregion

        /// <summary>
        /// 当前网络连接信息
        /// </summary>
        public string CurrentConnection
        {
            get => _currentConnection;
            set => SetProperty(ref _currentConnection, value);
        }
        private string _currentConnection = "无";

        /// <summary>
        /// 编辑使能
        /// </summary>
        public bool EditEnable
        {
            get => _editEnable;
            set => SetProperty(ref _editEnable, value);
        }
        private bool _editEnable = true;
    }
}
