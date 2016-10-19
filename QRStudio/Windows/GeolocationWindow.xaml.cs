using MahApps.Metro.Controls;
using QRStudio.Google;

namespace QRStudio
{
    public partial class GeolocationWindow : MetroWindow
    {
        private GeographyResult sResult;
        public GeoAddress Result { get; private set; }

        public GeolocationWindow()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            sResult = await Geography.Search(tbSearch.Text);

            if (sResult.status == "OK")
            {
                lvList.Items.Clear();

                foreach (var r in sResult.results)
                {
                    var postal = r.FromType("postal_code");
                    var county = r.FromType("country");

                    lvList.Items.Add(new GeolocationItem()
                    {
                        RawData = r,

                        Address = r.formatted_address
                            .Replace(county.long_name, "")
                            .Trim(),
                        Postal = postal.long_name
                    });
                }
            }
        }

        private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lvList.SelectedItems.Count > 0)
            {
                Result = (lvList.SelectedItem as GeolocationItem).RawData;

                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }

            this.Close();
        }
    }

    class GeolocationItem
    {
        internal GeoAddress RawData;

        public string Address { get; set; }
        public string Postal { get; set; }
    }
}
