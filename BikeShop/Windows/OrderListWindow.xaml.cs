using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BikeShop.Helpers;

namespace BikeShop.Windows
{
    /// <summary>
    /// Логика взаимодействия для OrderListWindow.xaml
    /// </summary>
    public class OrderDisplay
    {
        public int OrderID { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string ClientName { get; set; }
        public string PickupAddress { get; set; }
        public int PickupCode { get; set; }
        public string Status { get; set; }
        public string ProductsList { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public partial class OrderListWindow : Window
    {
        private BikeStoreDBEntities db = new BikeStoreDBEntities();
        private OrderEditWindow _editWindow;
        private List<OrderDisplay> allOrders;

        public OrderListWindow()
        {
            InitializeComponent();
            LoadOrders();
            ApplyRolePermissions();
        }

        private void LoadOrders()
        {
            lblFio.Text = CurrentUser.FullName + " (" + CurrentUser.RoleName + ")";

            var orders = db.Orders
                .Include("Users")
                .Include("Users.Roles")
                .Include("PickupPoints")
                .Include("OrderDetails")
                .Include("OrderDetails.Products")
                .ToList();

            allOrders = orders.Select(o => new OrderDisplay
            {
                OrderID = o.OrderID,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                ClientName = o.Users != null ? o.Users.FullName : "Неизвестно",
                PickupAddress = FormatAddress(o.PickupPoints),
                PickupCode = o.PickupCode,
                Status = o.Status,
                ProductsList = FormatProducts(o.OrderDetails),
                DeliveryDate = o.DeliveryDate
            }).ToList();

            dgOrders.ItemsSource = allOrders;
        }

        private string FormatAddress(PickupPoints point)
        {
            if (point == null) return "—";
            return $"{point.ZipCode}, г. {point.City}, ул. {point.Street}, д. {point.House}";
        }

        private string FormatProducts(ICollection<OrderDetails> details)
        {
            if (details == null || details.Count == 0) return "—";
            var parts = details.Select(d =>
                $"{d.Products?.SKU ?? "?"} x{d.Quantity}");
            return string.Join(", ", parts);
        }

        private void ApplyRolePermissions()
        {
            if (CurrentUser.IsAdmin)
            {
                btnAddOrder.Visibility = Visibility.Visible;
                btnDeleteOrder.Visibility = Visibility.Visible;
            }
        }

        private void DgOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CurrentUser.IsAdmin) return;
            var selected = dgOrders.SelectedItem as OrderDisplay;
            if (selected == null) return;
            OpenEditWindow(selected);
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            OpenEditWindow(null);
        }
        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgOrders.SelectedItem as OrderDisplay;
            if (selected == null)
            {
                MessageBox.Show("Пожалуйста, выберите заказ в таблице для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить заказ №{selected.OrderNumber}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var details = db.OrderDetails.Where(d => d.OrderID == selected.OrderID).ToList();
                    foreach (var detail in details)
                    {
                        db.OrderDetails.Remove(detail);
                    }

                    var order = db.Orders.FirstOrDefault(o => o.OrderID == selected.OrderID);
                    if (order != null)
                    {
                        db.Orders.Remove(order);
                        db.SaveChanges();
                        MessageBox.Show("Заказ успешно удален.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadOrders();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении заказа: " + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenEditWindow(OrderDisplay order)
        {
            if (_editWindow != null && _editWindow.IsVisible)
            {
                MessageBox.Show("Окно редактирования уже открыто!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                _editWindow.Activate();
                return;
            }
            _editWindow = new OrderEditWindow(order);
            _editWindow.Owner = this;
            _editWindow.Closed += (s, args) =>
            {
                _editWindow = null;
                LoadOrders();
            };
            _editWindow.Show();
        }
    }
}
