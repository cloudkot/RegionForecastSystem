using System;
using System.Data.SqlClient;
using System.Windows;

namespace RegionForecastSystem // Это имя должно совпадать с x:Class в XAML (до точки)
{
    public partial class LoginWindow : Window // Имя класса = LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent(); // Ошибка исчезнет, если XAML и C# синхронизированы
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль!");
                return;
            }

            try
            {
                string query = "SELECT Role FROM Users WHERE Username = @user AND Password = @pass";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@user", username),
                    new SqlParameter("@pass", password)
                };

                object result = DatabaseService.ExecuteScalar(query, parameters);

                if (result != null)
                {
                    string role = result.ToString();

                    // ВРЕМЕННО КОММЕНТИРУЕМ, ТАК КАК MAINWINDOW ПОКА НЕТ
                    // MainWindow mainWindow = new MainWindow(role);
                    // mainWindow.Show();
                    // this.Close();

                    MessageBox.Show($"Успешный вход! Ваша роль: {role}", "Успех");
                }
                else
                {
                    ShowError("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}