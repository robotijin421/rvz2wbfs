using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WiiConverterDesktop.Logic;
using WiiConverterDesktop.Services;

namespace WiiConverterDesktop.ViewModels
{
    public class ConversionItem : INotifyPropertyChanged
    {
        private string _status = "Pending";
        private double _progress = 0;

        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private ConversionEngine? _engine;
        private string _dolphinPath = "";
        private string _witPath = "";
        private string _outputPath = "";

        public MainViewModel()
        {
            _settingsService = new SettingsService();
            _dolphinPath = _settingsService.Settings.DolphinToolPath;
            _witPath = _settingsService.Settings.WitPath;
            _outputPath = _settingsService.Settings.OutputDirectory;

            Items = new ObservableCollection<ConversionItem>();
            ConvertCommand = new RelayCommand(async _ => await StartConversion());
        }

        public ObservableCollection<ConversionItem> Items { get; }
        public ICommand ConvertCommand { get; }

        public string DolphinPath
        {
            get => _dolphinPath;
            set { _dolphinPath = value; _settingsService.Settings.DolphinToolPath = value; _settingsService.Save(); OnPropertyChanged(); }
        }

        public string WitPath
        {
            get => _witPath;
            set { _witPath = value; _settingsService.Settings.WitPath = value; _settingsService.Save(); OnPropertyChanged(); }
        }

        public string OutputPath
        {
            get => _outputPath;
            set { _outputPath = value; _settingsService.Settings.OutputDirectory = value; _settingsService.Save(); OnPropertyChanged(); }
        }

        private async System.Threading.Tasks.Task StartConversion()
        {
            if (string.IsNullOrEmpty(DolphinPath) || string.IsNullOrEmpty(WitPath))
            {
                MessageBox.Show("Please set paths for DolphinTool and WIT in settings.");
                return;
            }

            if (string.IsNullOrEmpty(OutputPath))
            {
                MessageBox.Show("Please select an output directory.");
                return;
            }

            _engine = new ConversionEngine(DolphinPath, WitPath);

            foreach (var item in Items)
            {
                if (item.Status == "Complete") continue;

                try
                {
                    await _engine.ConvertRvzToWbfs(item.FilePath, OutputPath, (status, progress) =>
                    {
                        item.Status = status;
                        item.Progress = progress;
                    });
                    item.Status = "Complete";
                    item.Progress = 100;
                }
                catch (System.Exception ex)
                {
                    item.Status = "Error: " + ex.Message;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly System.Action<object?> _execute;
        public RelayCommand(System.Action<object?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event System.EventHandler? CanExecuteChanged;
    }
}
