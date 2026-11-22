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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private BikeStoreDBEntities db = new BikeStoreDBEntities();
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblError.Text = "";
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Заполните все поля!";
                return;
            }

            var user = db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user == null)
            {
                lblError.Text = "Неверный логин или пароль!";
                return;
            }
            CurrentUser.Id = user.UserID;
            CurrentUser.Fio = user.FullName;
            CurrentUser.Login = user.Login;
            CurrentUser.RoleId = user.RoleID;
            CurrentUser.RoleName = user.Roles.RoleName;

            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Id = 0;
            CurrentUser.Fio = "Гость";
            CurrentUser.Login = "guest";
            CurrentUser.RoleId = 1;
            CurrentUser.RoleName = "Гость";

            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
