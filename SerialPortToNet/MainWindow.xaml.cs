using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Panuon.WPF.UI;
using SerialPortToNet.ViewModel;
using static System.Net.Mime.MediaTypeNames;

namespace SerialPortToNet
{
    public partial class MainWindow : WindowX
    {
        private MainWindowVM _mainWindowVM;

        /// <summary>
        /// 串口列表（key，value：串口名，描述）
        /// </summary>
        private readonly Dictionary<string, string> _portList = new();

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowVM = (MainWindowVM)this.DataContext;
        }

        private void WindowX_Loaded(object sender, RoutedEventArgs e)
        {
            SearchSerialPort();

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    _mainWindowVM.NetToSerialPortData += $"{DateTime.Now} 1234567890\r\n";
                    _mainWindowVM.SerialPortToNetData += $"{DateTime.Now} abcdefgh\r\n";
                    Dispatcher.InvokeAsync(() =>
                    {
                        TbxNet2SPort.ScrollToEnd();
                        TbxSPort2Net.ScrollToEnd();
                    });
                }
            });
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
            if (_mainWindowVM.EditEnable)
            {
                _mainWindowVM.EditEnable = false;
                BtnStart.Content = "\uE9EA 停止";
                BtnStart.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F56059"));
                ButtonHelper.SetShadowColor(BtnStart, (Color)ColorConverter.ConvertFromString("#F56059"));
            }
            else
            {
                _mainWindowVM.EditEnable = true;
                BtnStart.Content = "\uE9E9 启动";
                BtnStart.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#36AAF7"));
                ButtonHelper.SetShadowColor(BtnStart, (Color)ColorConverter.ConvertFromString("#36AAF7"));
            }
        }

        // 网络->串口清空内容
        private void BtnNetToSerialPortClear_Click(object sender, RoutedEventArgs e)
        {

        }

        // 串口->网络清空内容
        private void BtnSerialPortToNetClear_Click(object sender, RoutedEventArgs e)
        {
            
        }

        Regex _numbetRegex = new Regex("[^0-9]+");
        // 波特率通过事件限制输入数字
        private void CobxBaudRate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numbetRegex.IsMatch(e.Text);
        }
    }
}
