using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QRStudio.Forms
{
    public class FormBase : UserControl, IQRForm
    {
        public string Generate()
        {
            StringBuilder sb = new StringBuilder();

            Generate(this.Content as FrameworkElement, ref sb);

            return sb.ToString();
        }

        private void Generate(FrameworkElement fe, ref StringBuilder sb)
        {

            try
            {
                int count = VisualTreeHelper.GetChildrenCount(fe);

                for (int i = 0; i < count; i++)
                {
                    var c = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;

                    if (c == null)
                        continue;

                    if (c.Tag != null)
                    {
                        string tag = (string)c.Tag;

                        if (tag.StartsWith("#"))
                        {
                            string header = tag.Split('#')[1];

                            switch (header)
                            {
                                case "Raw":
                                    sb.Append((c as TextBox).Text);
                                    break;

                                case "GEO_x":
                                case "GEO_y":
                                    string subHeader = header.Split('_')[1];
                                    double value = (c as Control.DoubleUpDown).Value.Value;

                                    if (subHeader == "x")
                                    {
                                        sb.Append($"GEO:{value},");
                                    }
                                    else
                                    {
                                        sb.Append(value);
                                    }
                                    break;

                                case "BODY":
                                    if (sb.ToString().StartsWith("SMSTO:"))
                                    {

                                    }

                                    break;

                            }
                        }
                        else if (tag.StartsWith("$"))
                        {
                            if (tag.StartsWith("$_"))
                            {
                                sb.Append($":{(c as TextBox).Text}");
                            }
                            else
                            {
                                sb.Append($"{tag.Substring(1)}:{(c as TextBox).Text}");
                            }
                        }
                        else
                        {
                            sb.Append($"{tag}{(tag.Length > 0 ? ":" : "")}{(c as TextBox).Text};");
                        }
                    }

                    Generate(c, ref sb);
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}
