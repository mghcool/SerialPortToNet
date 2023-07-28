using System;
using System.Text;
using System.Threading.Tasks;
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
            this.DispatcherUnhandledException += (sender, e) =>
            {
                // 处理UI线程上的未处理异常
                MessageBox.Show(e.Exception.Message, "UI线程异常");
                e.Handled = true;
                this.Shutdown();
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                // 处理非UI线程上的未处理异常
                Exception exception = (Exception)e.ExceptionObject;
                MessageBox.Show(exception.Message, "非UI线程异常");
                this.Shutdown();
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                // 处理异步异常
                MessageBox.Show(e.Exception.Message, "非UI线程异常");
                e.SetObserved();
                this.Shutdown();
            };
            // 注册编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
