using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BikeShop.Converters
{

        public class ImagePathConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                string path = value as string;

                if (string.IsNullOrEmpty(path))
                    return "/images/picture.png";

                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", path);
                if (File.Exists(fullPath))
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(fullPath, UriKind.Absolute);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    return img;
                }
                return "/images/picture.png";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public class DiscountToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return Visibility.Collapsed;
                int discount = System.Convert.ToInt32(value);
                return discount > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public class FinalPriceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var product = value as Products;
                if (product == null) return "0 руб.";

                decimal price = product.Price;
                int discount = (int)product.Discount;

                if (discount > 0)
                {
                    decimal finalPrice = price - (price * discount / 100);
                    return finalPrice.ToString("0.00") + " руб.";
                }
                return price.ToString("0.00") + " руб.";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
}