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
using BikeShop.Helpers;
using BikeShop.Windows;

namespace BikeShop
{
        public partial class MainWindow : Window
        {
            private BikeStoreDBEntities db = new BikeStoreDBEntities();
            private List<Products> allProducts;
            private ProductEditWindow _editWindow;

        public MainWindow()
            {
                InitializeComponent();
                LoadData();
                ApplyRolePermissions();
            }

            private void LoadData()
            {
                lblFio.Text = CurrentUser.FullName + " (" + CurrentUser.RoleName + ")";

                allProducts = db.Products
                    .Include("Categories")
                    .Include("Manufacturers")
                    .Include("Suppliers")
                    .ToList();

                ApplyFiltersAndSort();
            }

            private void ApplyRolePermissions()
            {
                if (CurrentUser.IsManager || CurrentUser.IsAdmin)
                    btnOrders.Visibility = Visibility.Visible;
                if (CurrentUser.IsManager || CurrentUser.IsAdmin)
                {
                    pnlControls.Visibility = Visibility.Visible;
                }

                if (CurrentUser.IsAdmin)
                    btnAdd.Visibility = Visibility.Visible;
            }
            private void DgProducts_LoadingRow(object sender, DataGridRowEventArgs e)
            {
                var product = e.Row.Item as Products;
                if (product == null) return;

                if (product.Quantity == 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightGray);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkGray);
                }
                else if (product.Discount > 15)
                {
                    e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#483D8B"));
                    e.Row.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Colors.White);
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                }
            }

            private void ApplyFiltersAndSort()
            {
                if (allProducts == null) return;

                IEnumerable<Products> query = allProducts;
                string searchText = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(p =>
                        (p.ProductName != null && p.ProductName.ToLower().Contains(searchText)) ||
                        (p.SKU != null && p.SKU.ToLower().Contains(searchText)) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchText)) ||
                        (p.Categories != null && p.Categories.CategoryName != null && p.Categories.CategoryName.ToLower().Contains(searchText)) ||
                        (p.Manufacturers != null && p.Manufacturers.ManufacturerName != null && p.Manufacturers.ManufacturerName.ToLower().Contains(searchText)) ||
                        (p.Suppliers != null && p.Suppliers.SupplierName != null && p.Suppliers.SupplierName.ToLower().Contains(searchText))
                    );
                }
                if (cmbFilterDiscount.SelectedItem is ComboBoxItem selectedFilter)
                {
                    string filterText = selectedFilter.Content.ToString();
                    if (filterText == "0 - 11,99%")
                        query = query.Where(p => p.Discount >= 0 && p.Discount <= 11);
                    else if (filterText == "12 - 18,99%")
                        query = query.Where(p => p.Discount >= 12 && p.Discount <= 18);
                    else if (filterText == "19% и более")
                        query = query.Where(p => p.Discount >= 19);
                }
                bool isAscending = true;
                if (cmbSortDir.SelectedItem is ComboBoxItem dirItem)
                {
                    if (dirItem.Content.ToString() == "По убыванию")
                        isAscending = false;
                }

                string sortBy = "По цене";
                if (cmbSortBy.SelectedItem is ComboBoxItem sortItem)
                {
                    sortBy = sortItem.Content.ToString();
                }

                if (sortBy == "По цене")
                    query = isAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                else if (sortBy == "По скидке")
                    query = isAscending ? query.OrderBy(p => p.Discount) : query.OrderByDescending(p => p.Discount);
                else if (sortBy == "По остатку")
                    query = isAscending ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity);

                dgProducts.ItemsSource = query.ToList();
            }

            private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            {
                ApplyFiltersAndSort();
            }

            private void CmbFilterDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ApplyFiltersAndSort();
            }

            private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ApplyFiltersAndSort();
            }

        private void DgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!CurrentUser.IsAdmin) return;

            var selected = dgProducts.SelectedItem as Products;
            if (selected == null) return;

            OpenEditWindow(selected);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenEditWindow(null);
        }
        private void OpenEditWindow(Products product)
        {
            if (_editWindow != null && _editWindow.IsVisible)
            {
                MessageBox.Show("Окно редактирования уже открыто! Закройте его, чтобы продолжить.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                _editWindow.Activate();
                return;
            }

            _editWindow = new ProductEditWindow(product);
            _editWindow.Owner = this;

            _editWindow.Closed += (s, args) =>
            {
                _editWindow = null;
                LoadData();
            };

            _editWindow.Show();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
            {
                CurrentUser.Clear();
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }

            private void BtnOrders_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Список заказов будет в Модуле 4", "Информация");
            }
        }
}