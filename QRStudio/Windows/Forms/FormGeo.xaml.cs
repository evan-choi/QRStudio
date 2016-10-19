using System.Windows;

namespace QRStudio.Forms
{
    public partial class FormGeo : FormBase
    {
        public FormGeo()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var gw = new GeolocationWindow();
            bool? result = gw.ShowDialog();

            if (result.HasValue && result.Value)
            {
                gx.Value = gw.Result.geometry.location.lat;
                gy.Value = gw.Result.geometry.location.lng;
            }
        }
    }
}
