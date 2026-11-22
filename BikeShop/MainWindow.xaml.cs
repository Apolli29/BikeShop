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
        private List<Products> allproducts;
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            ApplyRolePermissions();
        }
        private void LoadData()
        {
            lblFio.Text = CurrentUser.Fio + " (" + CurrentUser.RoleName + ")";

            allProducts = db.Products
                .Include("Categories")
                .Include("Manufacturers")
                .Include("Suppliers")
                .ToList();

            dgProducts.ItemsSource = allProducts;
        }
        private void ApplyRolePermissions()
        {
            if (CurrentUser.IsManager || CurrentUser.IsAdmin)
                btnOrders.Visibility = Visibility.Visible;

            if (CurrentUser.IsAdmin)
                btnAdd.Visibility = Visibility.Visible;
        }
    }
}
