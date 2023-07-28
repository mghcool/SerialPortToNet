using System.Text;
using System.Windows;

namespace SerialPortToNet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 注册编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
