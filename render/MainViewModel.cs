using render.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using render.Models;
using System.Diagnostics;
using SkiaSharp;

namespace render
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IFileDialogService _fileService;
        private readonly IMessageBoxService _messageService;
        private readonly ImageRenderer _renderer;

        private string _objPath;
        private string _texturePath;
        private ImageSource _resultImage;

        public ICommand OpenCommand { get; }
        public ICommand RerenderCommand { get; }

        public ImageSource ResultImage
        {
            get => _resultImage;
            set
            {
                _resultImage = value;
                OnPropertyChanged();
            }
        }

        // 渲染进度 [0–100]
        private double _progress;
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }


        public string TextBox1 { get; set; }
        public string TextBox2 { get; set; }
        public string TextBox3 { get; set; }
        public double SliderValue { get; set; }

        // 标记是否正在渲染
        private bool _isRendering;
        public bool IsRendering
        {
            get => _isRendering;
            set { 
                _isRendering = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(CurrentImageSource));
                OnPropertyChanged(nameof(IsButtonEnabled));
            }
        }

        public bool IsButtonEnabled => !IsRendering /*&& !string.IsNullOrWhiteSpace(TextBox1)
            && !string.IsNullOrWhiteSpace(TextBox2) && !string.IsNullOrWhiteSpace(TextBox3)*/;

        public ImageSource CurrentImageSource =>
        new BitmapImage(new Uri(IsRendering
            ? "pack://application:,,,/Assets/Images/Merrli_Sulliek.png"
            : "pack://application:,,,/Assets/Images/Merrli_field.png"));

        public MainViewModel(IFileDialogService fileService,
                            IMessageBoxService messageService,
                            ImageRenderer renderer)
        {
            _fileService = fileService;
            _messageService = messageService;
            _renderer = renderer;

            OpenCommand = new RelayCommand(async () => await ExecuteOpenAsync(), () => !IsRendering);
            RerenderCommand = new RelayCommand(async () => await ExecuteRender(), CanExecuteRender);

        }

        private bool CanExecuteRender()
        {
            return !IsRendering /*&& !string.IsNullOrWhiteSpace(TextBox1) && !string.IsNullOrWhiteSpace(TextBox2) && !string.IsNullOrWhiteSpace(TextBox3)*/;
        }

        private async Task ExecuteOpenAsync()
        {
            IsRendering = true;
            Progress = 0;
            
            try
            {
                // 选择OBJ文件
                _objPath = _fileService.OpenFileDialog("选择OBJ文件", "OBJ Files|*.obj");
                if (string.IsNullOrEmpty(_objPath)) return;

                // 询问是否加载贴图
                if (_messageService.ShowConfirmation("是否加载贴图？"))
                {
                    _texturePath = _fileService.OpenTextureDialog("选择贴图文件",
                        "Image Files|*.jpg;*.png;*.bmp");
                }

                var progressReporter = new Progress<double>(p => Progress = p);
                // 执行渲染
                //var bitmap = _renderer.Render(_objPath, _texturePath);
                Class1 Class2a = new Class1(20f, 20f, 20f, 500.0);
                var bitmap = await Task.Run(() => _renderer.Render(_objPath, _texturePath, Class2a, progressReporter));
                Debug.WriteLine("bitmap got");
                IsRendering = false;
                Progress = 100;
                ResultImage = ConvertToImageSource(bitmap);
                (OpenCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误: {ex.Message}");
                Debug.WriteLine("Exception catched");
                IsRendering = false;
                Progress = 0;
            }
        }

        private async Task ExecuteRender()
        {
            IsRendering = true;
            Progress = 0;
            try
            {
                var progressReporter = new Progress<double>(p => Progress = p);
                Class1 class3a = new Class1(float.Parse(TextBox1), float.Parse(TextBox2), float.Parse(TextBox3), (float)SliderValue);
                var bitmap = await Task.Run(() => _renderer.Render(_objPath, _texturePath, class3a, progressReporter));
                Debug.WriteLine("bitmap got");
                IsRendering = false;
                Progress = 100;
                ResultImage = ConvertToImageSource(bitmap);
                (RerenderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误: {ex.Message}");
                Debug.WriteLine("Exception catched");
                IsRendering = false;
                Progress = 0;
            }
        }

        public static BitmapSource ConvertToImageSource(SKBitmap skBitmap)
        {
            Debug.WriteLine("ConvertToImageSource called");
            var image = SKImage.FromBitmap(skBitmap);
            var data = image.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream(data))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        /*private BitmapImage ConvertToImageSource(System.Drawing.Bitmap bitmap)
        {
            Debug.WriteLine("ConvertToImageSource called");
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }*/

        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
