using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using BikeShop.Helpers;
using System.Windows.Media;

namespace BikeShop.Windows
{
    public partial class ProductEditWindow : Window
    {
        private BikeStoreDBEntities db = new BikeStoreDBEntities();
        private Products _currentProduct; 
        private string _newPhotoPath;     
        private string _oldPhotoFileName;

        public ProductEditWindow(Products product)
        {
            InitializeComponent();
            _currentProduct = product;

            LoadComboBoxes();

            if (_currentProduct != null)
            {
                Title = "Редактирование товара";
                LoadProductData();
            }
            else
            {
                Title = "Добавление нового товара";
                imgProduct.Source = new BitmapImage(new Uri("/images/picture.png", UriKind.Relative));
            }
        }
        private void LoadComboBoxes()
        {
            cmbCategory.ItemsSource = db.Categories.ToList();
            cmbManufacturer.ItemsSource = db.Manufacturers.ToList();
            cmbSupplier.ItemsSource = db.Suppliers.ToList();
        }

        private void LoadProductData()
        {
            txtSKU.Text = _currentProduct.SKU;
            txtName.Text = _currentProduct.ProductName;
            txtPrice.Text = _currentProduct.Price.ToString();
            txtDiscount.Text = _currentProduct.Discount.ToString();
            txtQuantity.Text = _currentProduct.Quantity.ToString();
            txtUnit.Text = _currentProduct.Unit;
            txtDescription.Text = _currentProduct.Description;

            cmbCategory.SelectedValue = _currentProduct.CategoryID;
            cmbManufacturer.SelectedValue = _currentProduct.ManufacturerID;
            cmbSupplier.SelectedValue = _currentProduct.SupplierID;

            _oldPhotoFileName = _currentProduct.Photo;
            if (!string.IsNullOrEmpty(_currentProduct.Photo))
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", _currentProduct.Photo);
                if (File.Exists(fullPath))
                {
                    imgProduct.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                }
                else
                {
                    imgProduct.Source = new BitmapImage(new Uri("/images/picture.png", UriKind.Relative));
                }
            }
            else
            {
                imgProduct.Source = new BitmapImage(new Uri("/images/picture.png", UriKind.Relative));
            }
        }
        private void BtnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Изображения|*.jpg;*.jpeg;*.png";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    BitmapImage resized = ResizeImage(dlg.FileName, 300, 200);
                    imgProduct.Source = resized;

                    string newFileName = "img_" + Guid.NewGuid().ToString().Substring(0, 8) + ".png";
                    string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", newFileName);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(resized));
                    using (FileStream stream = new FileStream(savePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    _newPhotoPath = newFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обработке изображения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private BitmapImage ResizeImage(string filePath, int width, int height)
        {
            BitmapImage original = new BitmapImage(new Uri(filePath));
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawImage(original, new System.Windows.Rect(0, 0, width, height));
            }
            renderBitmap.Render(drawingVisual);

            BitmapImage result = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(stream);
                stream.Position = 0;
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
            }
            return result;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSKU.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Артикул и Наименование обязательны для заполнения!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Replace('.', ','), out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть числом и не может быть отрицательной!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDiscount.Text, out int discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть целым числом от 0 до 100!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Количество на складе не может быть отрицательным!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_currentProduct == null)
                {
                    _currentProduct = new Products();
                    db.Products.Add(_currentProduct);
                }

                _currentProduct.SKU = txtSKU.Text.Trim();
                _currentProduct.ProductName = txtName.Text.Trim();
                _currentProduct.Price = price;
                _currentProduct.Discount = discount;
                _currentProduct.Quantity = quantity;
                _currentProduct.Unit = txtUnit.Text.Trim();
                _currentProduct.Description = txtDescription.Text.Trim();

                _currentProduct.CategoryID = cmbCategory.SelectedValue as int?;
                _currentProduct.ManufacturerID = cmbManufacturer.SelectedValue as int?;
                _currentProduct.SupplierID = cmbSupplier.SelectedValue as int?;

                if (_newPhotoPath != null)
                {
                    if (!string.IsNullOrEmpty(_oldPhotoFileName) && _oldPhotoFileName != _newPhotoPath)
                    {
                        string oldFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", _oldPhotoFileName);
                        if (File.Exists(oldFullPath))
                            File.Delete(oldFullPath);
                    }
                    _currentProduct.Photo = _newPhotoPath;
                }

                db.SaveChanges();

                MessageBox.Show("Товар успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message + "\nПроверьте правильность введенных данных.", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
