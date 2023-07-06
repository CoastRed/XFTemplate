using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using XFFrameWork.WPF.Shell.Extensions;

namespace XFFrameWork.WPF.Shell.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;

        private string _title = "Prism Application";
        
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        #region 命令

        public DelegateCommand<string> NavigateCommand
        {
            get
            {
                return new DelegateCommand<string>(window =>
                {
                    this.regionManager.Regions[PrismRegionManager.MainWindowContentRegion].RequestNavigate(window);
                });
            }
        }

        #endregion

    }
}
