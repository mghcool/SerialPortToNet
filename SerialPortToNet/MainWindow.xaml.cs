using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Panuon.WPF.UI;
using SerialPortToNet.Model;
using SerialPortToNet.ViewModel;

namespace SerialPortToNet
{
    public partial class MainWindow : WindowX
    {
        /// <summary>
        /// 绑定的数据源
        /// </summary>
        private MainWindowVM _mainWindowVM;

        /// <summary>
        /// 串口列表（key，value：串口名，描述）
        /// </summary>
        private readonly Dictionary<string, string> _portList = new();


        private SerialPort _serialPort = new SerialPort();
        private TcpListener? _tcpListener;
        private Socket? _tcpClient;

        /// <summary>
        /// TextBox可以显示的最大字符长度
        /// </summary>
        private const int _textBoxMaxShowLength = 40960;

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowVM = (MainWindowVM)this.DataContext;
            _serialPort.DataReceived += SerialPortReceiveHandler;
        }

        private void WindowX_Loaded(object sender, RoutedEventArgs e)
        {
            SearchSerialPort();
        }

        private void WindowX_Closed(object sender, EventArgs e)
        {
            if (_mainWindowVM.EditEnable)
            {
                try
                {
                    _serialPort.Close();
                    _tcpListener?.Stop();
                    _tcpClient?.Disconnect(false);
                }
                catch { }
            }
        }

        // 刷新串口按钮
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            SearchSerialPort();
        }

        /// <summary>
        /// 搜索串口
        /// </summary>
        private void SearchSerialPort()
        {
            _mainWindowVM.PortNameItems.Clear();
            _portList.Clear();
            using var searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'");
            var hardInfos = searcher.Get();
            foreach (var hardInfo in hardInfos)
            {
                if (hardInfo.Properties["Name"].Value != null)
                {
                    string? deviceName = hardInfo.Properties["Name"].Value.ToString();
                    string port = deviceName!.Split(new char[] { '(', ')' })[1].Split('-')[0];
                    string? description = hardInfo.Properties["Description"].Value.ToString();
                    _mainWindowVM.PortNameItems.Add($"{port} -> {description}");
                    _portList.Add(port, description!);
                }
            }
        }

        // 启动按钮
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if(!IPAddress.TryParse(_mainWindowVM.NetAddress, out var address))
            {
                MessageBoxX.Show(this, "IP地址格式错误，请重新输入！", MessageBoxIcon.Warning);
                return;
            }
            
            if (_mainWindowVM.EditEnable)
            {
                try
                {
                    _serialPort.PortName = _portList.Keys.ToArray()[_mainWindowVM.CheckedPortNameIndex];
                    _serialPort.BaudRate = _mainWindowVM.CheckedBaudRate;
                    _serialPort.DataBits = _mainWindowVM.CheckedDataBit;
                    _serialPort.Parity = _mainWindowVM.CheckedParity;
                    _serialPort.StopBits = (StopBits)_mainWindowVM.CheckedStopBitsIndex;
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    MessageBoxX.Show(this, ex.Message, $"打开{_serialPort.PortName}失败", MessageBoxIcon.Error);
                    return;
                }
                if(_mainWindowVM.CheckedNetworkMode == 0)
                {
                    // 服务模式
                    
                    try
                    {
                        StartTcpListen();
                    }
                    catch (Exception ex)
                    {
                        _serialPort.Close();
                        MessageBoxX.Show(this, ex.Message, $"启动TCP服务失败", MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (_mainWindowVM.CheckedNetworkMode == NetworkMode.TCP客户端)
                {
                    // 客户端模式
                    _tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    var handler = PendingBox.Show(this, "正在连接...", "提示");
                    try
                    {
                        _tcpClient.Connect(address, _mainWindowVM.NetPort);
                        handler.Close();
                        _mainWindowVM.CurrentConnection = $"{_tcpClient.RemoteEndPoint}";
                        TcpClientReceiveHandler();
                    }
                    catch (Exception ex)
                    {
                        handler.Close();
                        _serialPort.Close();
                        MessageBoxX.Show(this, ex.Message, $"连接TCP服务失败", MessageBoxIcon.Error);
                        return;
                    }
                }
                // 更改界面
                _mainWindowVM.EditEnable = false;
                SetBtnStartStyle(true);
            }
            else
            {
                _mainWindowVM.EditEnable = true;
                SetBtnStartStyle(false);
                _serialPort.Close();
                _tcpListener?.Stop();
                _tcpListener = null;
                _tcpClient?.Disconnect(false);
            }
        }

        /// <summary>
        /// 设置启动按钮的样式
        /// </summary>
        /// <param name="isStart">true：设置为启动后的样式，false：设置为关闭后的样式</param>
        private void SetBtnStartStyle(bool isStart)
        {
            if (isStart)
            {
                BtnStart.Content = "\uE9EA 停止";
                BtnStart.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F56059"));
                ButtonHelper.SetShadowColor(BtnStart, (Color)ColorConverter.ConvertFromString("#F56059"));
            }
            else
            {
                BtnStart.Content = "\uE9E9 启动";
                BtnStart.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#36AAF7"));
                ButtonHelper.SetShadowColor(BtnStart, (Color)ColorConverter.ConvertFromString("#36AAF7"));
            }
        }

        // 网络->串口清空内容
        private void BtnNetToSerialPortClear_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowVM.NetToSerialPortData = string.Empty;
        }

        // 串口->网络清空内容
        private void BtnSerialPortToNetClear_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowVM.SerialPortToNetData = string.Empty;
        }

        // 波特率输入框通过事件限制输入数字
        private void CobxBaudRate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        // IP地址输入限制
        private void CobxIPAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);
        }

        // 网络模式选中项事件
        private void CobxNetMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_mainWindowVM is not null)
            {
                if (_mainWindowVM.CheckedNetworkMode == NetworkMode.TCP服务器)
                {
                    _mainWindowVM.NetAddressIsEnable = false;
                    _mainWindowVM.NetAddress = "0.0.0.0";
                }
                else if (_mainWindowVM.CheckedNetworkMode == NetworkMode.TCP客户端)
                {
                    _mainWindowVM.NetAddressIsEnable = true;
                    _mainWindowVM.NetAddress = "127.0.0.1";
                }
            }
        }


        /// <summary>
        /// 开启tcp监听线程
        /// </summary>
        private Task StartTcpListen()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _mainWindowVM.NetPort);
            _tcpListener.Start(1);
            return Task.Run(() =>
            {
                try
                {
                    _tcpClient = _tcpListener.AcceptSocket();
                    _mainWindowVM.CurrentConnection = $"{_tcpClient.RemoteEndPoint}";
                    TcpClientReceiveHandler();
                    _tcpListener.Stop();
                }
                catch { }
            });
        }

        /// <summary>
        /// tcp接收数据处理
        /// </summary>
        /// <returns></returns>
        private Task TcpClientReceiveHandler()
        {
            if (_tcpClient == null) return Task.CompletedTask;
            return Task.Run(() =>
            {
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    int receiveLength;
                    try
                    {
                        receiveLength = _tcpClient.Receive(buffer);
                    }
                    catch
                    {
                        break;
                    }
                    if (receiveLength == 0) break;

                    _serialPort.Write(buffer, 0, receiveLength);

                    // 组织字符串
                    byte[] receiveData = new byte[receiveLength];
                    Array.Copy(buffer, receiveData, receiveLength);
                    string str = string.Empty;
                    if(_mainWindowVM.NetToSerialPortData != string.Empty) str += _mainWindowVM.NetToSerialPortNewLine ? "\r\n" : " ";
                    str += BytesToString(receiveData, _mainWindowVM.CheckedNet2SPortEncodingMode);
                    // 显示
                    _mainWindowVM.NetToSerialPortData += str;
                    if(_mainWindowVM.NetToSerialPortData.Length > _textBoxMaxShowLength)
                    {
                        // 限制最长长度
                        _mainWindowVM.NetToSerialPortData = _mainWindowVM.NetToSerialPortData[^_textBoxMaxShowLength..];
                    }
                    Dispatcher.InvokeAsync(() => TbxNet2SPort.ScrollToEnd());
                }
                _tcpClient.Close();
                _tcpClient = null;
                _mainWindowVM.CurrentConnection = $"无";
                if(_mainWindowVM.CheckedNetworkMode == NetworkMode.TCP客户端 && !_mainWindowVM.EditEnable)
                {
                    // 当tcp连接断开时，如果是客户端模式，那么模拟点击一下停止按钮
                    Dispatcher.Invoke(() =>
                    {
                        BtnStart_Click(new(), new());
                        Toast("连接已断开！");
                    });
                }
                if(_mainWindowVM.CheckedNetworkMode == NetworkMode.TCP服务器 && !_mainWindowVM.EditEnable)
                {
                    // 重新开始监听
                    StartTcpListen();
                }
            });
        }

        /// <summary>
        /// 串口接收数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPortReceiveHandler(object sender, SerialDataReceivedEventArgs e)
        {
            int receiveLength = _serialPort.BytesToRead;
            byte[] buffer = new byte[receiveLength];
            _serialPort.Read(buffer, 0, receiveLength);
            try
            {
                _tcpClient?.Send(buffer);
            }
            catch
            {
                _tcpClient = null;
            }

            // 组织字符串
            string str = string.Empty;
            if (_mainWindowVM.SerialPortToNetData != string.Empty) str += _mainWindowVM.SerialPortToNetNewLine ? "\r\n" : " ";
            str += BytesToString(buffer, _mainWindowVM.CheckedSPort2NetEncodingMode);
            // 显示
            _mainWindowVM.SerialPortToNetData += str;
            if (_mainWindowVM.SerialPortToNetData.Length > _textBoxMaxShowLength)
            {
                // 限制最长长度
                _mainWindowVM.SerialPortToNetData = _mainWindowVM.SerialPortToNetData[^_textBoxMaxShowLength..];
            }
            Dispatcher.InvokeAsync(() => TbxSPort2Net.ScrollToEnd());
        }

        /// <summary>
        /// 字节数据根据编码方式转字符串
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="encodingMode"></param>
        /// <returns></returns>
        private string BytesToString(byte[] buffer, EncodingMode encodingMode)
        {
            string str;
            switch (encodingMode)
            {
                case EncodingMode.ASCII:
                    str = Encoding.ASCII.GetString(buffer); break;
                case EncodingMode.UTF8:
                    str = Encoding.UTF8.GetString(buffer); break;
                case EncodingMode.GBK:
                    str = Encoding.GetEncoding("GBK").GetString(buffer); break;
                default:
                    str = BitConverter.ToString(buffer).Replace('-', ' '); break;
            }
            return str;
        }
    }
}
