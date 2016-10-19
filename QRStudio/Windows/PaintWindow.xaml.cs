using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QRStudio
{

    public partial class PaintWindow : MahApps.Metro.Controls.MetroWindow
    {

        #region [ Commands ]
        private ICommand unSelectCommand;
        public ICommand UnSelectCommand
        {
            get
            {
                return unSelectCommand ??
                    (unSelectCommand = new ActionCommand(() =>
                    {
                        qCvs.UnSelect();
                    }));
            }
        }

        private ICommand applyCommand;
        public ICommand ApplyCommand
        {
            get
            {
                return applyCommand ??
                    (applyCommand = new ActionCommand(() =>
                    {
                        qCvs.SetBrush(colorBox.Brush, chkOverride.IsChecked.Value);
                    }));
            }
        }

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return saveCommand ??
                    (saveCommand = new ActionCommand(() =>
                    {
                        SaveQRCode();
                    }));
            }
        }

        private ICommand createCommand;
        public ICommand CreateCommand
        {
            get
            {
                return createCommand ??
                    (createCommand = new ActionCommand(() =>
                    {
                        CreateQRCode();
                    }));
            }
        }
        #endregion

        public PaintWindow()
        {
            InitializeComponent();

            qCvs.Version = 0;

            colorBox.BrushChanged += ColorBox_BrushChanged;
            verUpDown.ValueChanged += VerUpDown_ValueChanged;

            chkVer.Checked += ChkVer_Checked;
            chkVer.Unchecked += ChkVer_Unchecked;

            this.Loaded += PaintWindow_Loaded;
        }

        private void PaintWindow_Loaded(object sender, RoutedEventArgs e)
        {
            qCvs.RenderMode = Control.QRCanvas.CanvasMode.Clipping;
            qCvs.Apply("QRStudio");
        }

        private void ChkVer_Unchecked(object sender, RoutedEventArgs e)
        {
            qCvs.Version = (int)verUpDown.Value;
        }

        private void ChkVer_Checked(object sender, RoutedEventArgs e)
        {
            qCvs.Version = 0;
        }

        private void VerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            qCvs.Version = (int)(double)e.NewValue;
        }

        private void ColorBox_BrushChanged(object sender, QRStudio.Control.BrushChangedEventArgs e)
        {
            qCvs.SetBrush(e.Brush, chkOverride.IsChecked.Value);
        }

        private void SaveQRCode()
        {
            var sfd = new SaveFileDialog()
            {
                Filter = "PNG파일(*.png)|*.png"
            };

            if (sfd.ShowDialog().Value)
            {
                var bmp = qCvs.ToBitmap();

                var encoder = new PngBitmapEncoder();
                var frame = BitmapFrame.Create(bmp);

                encoder.Frames.Add(frame);

                using (var file = File.OpenWrite(sfd.FileName))
                {
                    encoder.Save(file);
                }

                qCvs.Update();
            }
        }

        private async void CreateQRCode()
        {
            var sw = new SettingWindow();
            var result = sw.ShowDialog();

            if (result.HasValue && result.Value)
            {
                try
                {
                    qCvs.Apply(sw.RawContent);
                }
                catch
                {
                    await this.ShowMessageAsync("QRStudio", "데이터가 너무 큽니다. 인코드 버전을 올려주세요.");
                }
            }
        }
    }

    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

}