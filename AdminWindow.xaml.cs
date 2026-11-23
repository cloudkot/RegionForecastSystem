using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RegionForecastSystem
{
    public partial class AdminWindow : Window
    {
        private DataTable _allUsers;

        public AdminWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (Storyboard)FindResource("FadeIn");
            sb.Begin(MainRoot);
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = AppConfig.UseFakeDatabase
    ? FakeDatabaseService.ExecuteQuery("SELECT Id, Username, Role FROM Users")
    : DatabaseService.ExecuteQuery("SELECT Id, Username, Role FROM Users");
                UsersDataGrid.ItemsSource = _allUsers.DefaultView;
                UpdateCounters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWin = new LoginWindow();
            loginWin.Show();
            this.Close();
        }

        private void UpdateCounters()
        {
            if (_allUsers == null) return;
            TxtTotalUsers.Text = _allUsers.Rows.Count.ToString();
            TxtAdminsCount.Text = CountByRole("Admin").ToString();
            TxtManagersCount.Text = CountByRole("Manager").ToString();
        }

        private int CountByRole(string role)
        {
            int count = 0;
            foreach (DataRow row in _allUsers.Rows)
                if (row["Role"].ToString() == role) count++;
            return count;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allUsers == null) return;
            string filter = TxtSearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(filter))
                _allUsers.DefaultView.RowFilter = "";
            else
                _allUsers.DefaultView.RowFilter = $"Username LIKE '%{filter}%'";
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtNewUsername.Text.Trim();
            string password = TxtNewPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || CmbRole.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = ((ComboBoxItem)CmbRole.SelectedItem).Content.ToString();

            try
            {
                string query = "INSERT INTO Users (Username, Password, Role) VALUES (@user, @pass, @role)";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@user", username),
                    new SqlParameter("@pass", password),
                    new SqlParameter("@role", role)
                };

                DatabaseService.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Пользователь добавлен!", "Успех");
                TxtNewUsername.Clear();
                TxtNewPassword.Clear();
                CmbRole.SelectedIndex = -1;
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Внимание");
                return;
            }

            DataRowView row = (DataRowView)UsersDataGrid.SelectedItem;
            int userId = Convert.ToInt32(row["Id"]);
            string username = row["Username"].ToString();

            if (username == "admin")
            {
                MessageBox.Show("Нельзя удалить главного администратора.", "Запрещено");
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя '{username}'?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                DatabaseService.ExecuteNonQuery("DELETE FROM Users WHERE Id = @id",
                    new SqlParameter[] { new SqlParameter("@id", userId) });
                MessageBox.Show("Пользователь удален.", "Успех");
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void BtnOpenConstructor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Окно конструктора БД будет добавлено следующим шагом.", "В разработке");
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}