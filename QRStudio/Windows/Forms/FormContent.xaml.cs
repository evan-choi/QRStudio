using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace QRStudio.Forms
{
    public partial class FormContent : FormBase
    {
        public FormContent()
        {
            InitializeComponent();

            contentBox.TextChanged += ContentBox_TextChanged;
        }

        private void ContentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int bytes = Encoding.Default.GetByteCount(contentBox.Text);

            contentLabel.Content = bytes;
            contentLabel.Foreground = (bytes > 512 ? Brushes.Red : Brushes.Black);
        }
    }
}
