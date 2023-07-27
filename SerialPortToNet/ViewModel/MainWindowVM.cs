using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SerialPortToNet.Model;

namespace SerialPortToNet.ViewModel
{
    public class MainWindowVM : INotifyPropertyChanged
    {
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
        /// 选中的串口索引
        /// </summary>
        public int CheckedPortNameIndex { get; set; }
        public int CheckedBaudRate { get; set; } = 9600;
        public int CheckedDataBit { get; set; } = 8;
        public Parity CheckedParity{ get; set; } = Parity.None;
        public int CheckedStopBitsIndex { get; set; } = 1;
        #endregion
        #endregion

        #region 网络配置
        public string[] NetworkModeItems { get; set; } = new string[] { "TCP服务器", "TCP客户端" };

        public string NetAddress { get; set; } = "255.255.255.255";

        public int NetPort { get; set; } = 60000;

        /// <summary>
        /// 网络模式，服务端或客户端
        /// </summary>
        public int CheckedNetworkModeIndex { get; set; } = 0;
        #endregion

        /// <summary>
        /// 数据编码模式选项
        /// </summary>
        public DataEncodingMode[] EncodingModeItems { get; set; } = new DataEncodingMode[] { DataEncodingMode.HEX, DataEncodingMode.ASCII, DataEncodingMode.UTF8 };

        public DataEncodingMode CheckedNet2SPortEncodingMode { get; set; } = DataEncodingMode.HEX;
        public DataEncodingMode CheckedSPort2NetEncodingMode { get; set; } = DataEncodingMode.HEX;

        /// <summary>
        /// 网络->串口自动换行
        /// </summary>
        public bool NetToSerialPortNewLine { get; set; } = false;

        /// <summary>
        /// 串口->网络自动换行
        /// </summary>
        public bool SerialPortToNetNewLine { get; set; } = false;


        private string _currentConnection = "无";
        /// <summary>
        /// 当前网络连接信息
        /// </summary>
        public string CurrentConnection
        {
            get => _currentConnection;
            set
            {
                _currentConnection = value;
                OnPropertyChanged(nameof(CurrentConnection));
            }
        }

        private bool _editEnable = true;
        /// <summary>
        /// 编辑使能
        /// </summary>
        public bool EditEnable
        {
            get => _editEnable;
            set
            {
                _editEnable = value;
                OnPropertyChanged(nameof(EditEnable));
            }
        }

        private string _netToSerialPortData = string.Empty;
        public string NetToSerialPortData
        {
            get => _netToSerialPortData;
            set 
            { 
                _netToSerialPortData = value;
                OnPropertyChanged(nameof(NetToSerialPortData));
            }
        }

        private string _serialPortToNetData = string.Empty;
        public string SerialPortToNetData
        {
            get => _serialPortToNetData;
            set
            {
                _serialPortToNetData = value;
                OnPropertyChanged(nameof(SerialPortToNetData));
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
