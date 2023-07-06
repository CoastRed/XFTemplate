using NLog;
using Prism.Ioc;
using System.Linq;
using System;
using System.Windows;
using XFFrameWork.WPF.Shell.Views;
using System.Diagnostics;
using System.Threading.Tasks;
using SqlSugar;
using System.IO;
using XFFrameWork.WPF.Shell.Models;
using Microsoft.Extensions.Configuration;

namespace XFFrameWork.WPF.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public App()
        {
            //判断软件是否已经启动
            System.Diagnostics.Process thisProc = System.Diagnostics.Process.GetCurrentProcess();
            string name = thisProc.ProcessName;
            var processCount = System.Diagnostics.Process.GetProcessesByName(name).ToList().Where(s => s.Id != Process.GetCurrentProcess().Id).Count();
            if (processCount >= 1)
            {
                MessageBox.Show("程序已经启动了，请勿重复启动");
                Environment.Exit(0);
            }
            this.RegisterEvents();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ILogger>(() => LogManager.GetCurrentClassLogger());

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build().Get<Config>();

            containerRegistry.RegisterSingleton<ISqlSugarClient>(opt =>
            {
                return new SqlSugarScope(new ConnectionConfig()
                {
                    ConnectionString = "",
                    DbType = SqlSugar.DbType.SqlServer,
                    IsAutoCloseConnection = true
                });
            });

        }


        #region 异常处理

        private void RegisterEvents()
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;//Task异常 

            //UI线程未捕获异常处理事件（UI主线程）
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        //Task线程内未捕获异常处理事件
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                Exception? exception = e.Exception as Exception;
                if (exception != null)
                {
                    this.logger.Error(exception);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
            finally
            {
                e.SetObserved();
            }
        }

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)      
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    this.logger.Error(exception);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
            finally
            {
                //ignore

            }
        }

        //UI线程未捕获异常处理事件（UI主线程）
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                this.logger.Error(e.Exception);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
            finally
            {
                e.Handled = true;
            }
        }

        #endregion

    }
}
