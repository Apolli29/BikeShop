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
using System.Windows.Shapes;
using BikeShop.Helpers;

namespace BikeShop.Windows
{
    /// <summary>
    /// Логика взаимодействия для OrderEditWindow.xaml
    /// </summary>
    public class PickupPointDisplay
    {
        public int PointID { get; set; }
        public string FullAddress { get; set; }
    }

    public partial class OrderEditWindow : Window
    {
        private BikeStoreDBEntities db = new BikeStoreDBEntities();
        private OrderDisplay _currentOrder;

        public OrderEditWindow(OrderDisplay order)
        {
            InitializeComponent();
            _currentOrder = order;
            LoadComboBoxes();

            if (_currentOrder != null)
            {
                Title = "Редактирование заказа №" + _currentOrder.OrderNumber;
                LoadOrderData();
            }
            else
            {
                Title = "Добавление нового заказа";
                dpOrderDate.SelectedDate = DateTime.Today;
                var maxNumber = db.Orders.Any() ? db.Orders.Max(o => o.OrderNumber) : 0;
            }
        }

        private void LoadComboBoxes()
        {
            cmbProduct.ItemsSource = db.Products.ToList();

            var pointsFromDb = db.PickupPoints.ToList();

            var points = pointsFromDb.Select(p => new PickupPointDisplay
            {
                PointID = p.PointID,
                FullAddress = $"{p.ZipCode}, г. {p.City}, ул. {p.Street}, д. {p.House}"
            }).ToList();

            cmbPickupPoint.ItemsSource = points;
        }
        private void LoadOrderData()
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == _currentOrder.OrderID);
            if (order == null) return;

            var firstDetail = db.OrderDetails
                .FirstOrDefault(d => d.OrderID == order.OrderID);
            if (firstDetail != null)
            {
                cmbProduct.SelectedValue = firstDetail.ProductID;
            }

            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == order.Status)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }

            cmbPickupPoint.SelectedValue = order.PointID;

            dpOrderDate.SelectedDate = order.OrderDate;
            dpDeliveryDate.SelectedDate = order.DeliveryDate;

            txtPickupCode.Text = order.PickupCode.ToString();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Выберите артикул товара!", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbPickupPoint.SelectedItem == null)
            {
                MessageBox.Show("Выберите адрес пункта выдачи!", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpOrderDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату заказа!", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPickupCode.Text.Trim(), out int pickupCode))
            {
                MessageBox.Show("Код получения должен быть числом!", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int productId = (int)cmbProduct.SelectedValue;
                int pointId = ((PickupPointDisplay)cmbPickupPoint.SelectedItem).PointID;
                string status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();
                DateTime orderDate = dpOrderDate.SelectedDate.Value;
                DateTime? deliveryDate = dpDeliveryDate.SelectedDate;

                if (deliveryDate == null)
                {
                    MessageBox.Show("Выберите дату доставки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime deliveryDateValue = deliveryDate.Value;

                if (_currentOrder == null)
                {
                    var maxNumber = db.Orders.Any() ? db.Orders.Max(o => o.OrderNumber) : 0;
                    var newOrder = new Orders
                    {
                        OrderNumber = maxNumber + 1,
                        UserID = CurrentUser.Id,
                        PointID = pointId,
                        OrderDate = orderDate,
                        DeliveryDate = deliveryDateValue,
                        PickupCode = pickupCode,
                        Status = status
                    };
                    db.Orders.Add(newOrder);
                    db.SaveChanges();

                    var detail = new OrderDetails
                    {
                        OrderID = newOrder.OrderID,
                        ProductID = productId,
                        Quantity = 1
                    };
                    db.OrderDetails.Add(detail);
                }
                else
                {
                    var order = db.Orders.FirstOrDefault(o => o.OrderID == _currentOrder.OrderID);
                    if (order == null) return;

                    order.PointID = pointId;
                    order.OrderDate = orderDate;
                    order.DeliveryDate = deliveryDateValue;
                    order.PickupCode = pickupCode;
                    order.Status = status;

                    var oldDetail = db.OrderDetails.FirstOrDefault(d => d.OrderID == order.OrderID);
                    if (oldDetail != null)
                    {
                        db.OrderDetails.Remove(oldDetail);
                    }
                    var newDetail = new OrderDetails
                    {
                        OrderID = order.OrderID,
                        ProductID = productId,
                        Quantity = 1
                    };
                    db.OrderDetails.Add(newDetail);
                }

                db.SaveChanges();
                MessageBox.Show("Заказ успешно сохранен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении заказа: " + ex.Message,
                    "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}