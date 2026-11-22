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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BikeStoreDBEntities db = new BikeStoreDBEntities();
        private List<Products> Products;
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            ApplyRolePermissions();
        }
        private void LoadData()
        {
            lblFio.Text = CurrentUser.Fio + " (" + CurrentUser.RoleName + ")";

            Products = db.Products
                .Include("Categories")
                .Include("Manufacturers")
                .Include("Suppliers")
                .ToList();

            dgProducts.ItemsSource = Products;
        }
        private void ApplyRolePermissions()
        {
            if (CurrentUser.IsManager || CurrentUser.IsAdmin)
                btnOrders.Visibility = Visibility.Visible;

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
            else if ((decimal)product.Discount > 15)
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
        private void DgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!CurrentUser.IsAdmin) return;
            var selected = dgProducts.SelectedItem as Products;
            if (selected == null) return;
            MessageBox.Show("Редактирование товара: " + selected.ProductName);
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Форма добавления товара будет в Модуле 3");
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
            MessageBox.Show("Список заказов будет в Модуле 4");
        }
    }
}
