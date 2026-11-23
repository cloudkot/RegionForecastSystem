using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace RegionForecastSystem
{
    public partial class ManagerWindow : Window
    {
        private readonly string[] allowedTables = { "Regions", "MacroIndicators", "Demographics", "BudgetRevenues" };
        private SqlDataAdapter _dataAdapter;
        private DataTable _currentTable;
        private string _currentTableName;

        public ManagerWindow()
        {
            InitializeComponent();
            LoadTableList();
            LoadReportList();
            TxtAiInput.Text = "Регион: Московская область.\nНаселение: 7700000.\nВРП 2021: 1100000000000.\nВРП 2022: 1200000000000.\nВРП 2023: 1300000000000.\nБезработица: 3.1%.";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void LoadTableList()
        {
            CmbTables.ItemsSource = allowedTables;
            CmbTables.SelectedIndex = 0;
        }

        private void LoadReportList()
        {
            CmbReports.Items.Add("1. Инвестиции по регионам");
            CmbReports.Items.Add("2. Безработица по годам");
            CmbReports.Items.Add("3. Топ-5 по ВРП");
            CmbReports.Items.Add("4. Доходы бюджета");
            CmbReports.Items.Add("5. Предприятия по отраслям");
            CmbReports.SelectedIndex = 0;
        }

        private void CmbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTables.SelectedItem == null) return;
            _currentTableName = CmbTables.SelectedItem.ToString();

            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    string query = $"SELECT * FROM {_currentTableName}";
                    _dataAdapter = new SqlDataAdapter(query, conn);
                    new SqlCommandBuilder(_dataAdapter);
                    _currentTable = new DataTable();
                    _dataAdapter.Fill(_currentTable);
                    DataEditorGrid.ItemsSource = _currentTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_dataAdapter == null || _currentTable == null) return;
            try
            {
                _dataAdapter.Update(_currentTable);
                MessageBox.Show("Сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string query = "";
            switch (CmbReports.SelectedIndex)
            {
                case 0: query = "SELECT r.Name AS Region, SUM(i.Amount) AS TotalInvestment FROM Regions r JOIN Investments i ON r.Id = i.RegionId GROUP BY r.Name"; break;
                case 1: query = "SELECT Year, AVG(UnemploymentRate) AS AvgUnemployment FROM LaborMarket GROUP BY Year ORDER BY Year"; break;
                case 2: query = "SELECT TOP 5 RegionName, GRP FROM MacroIndicators ORDER BY GRP DESC"; break;
                case 3: query = "SELECT Year, SUM(Amount) AS TotalRevenue FROM BudgetRevenues GROUP BY Year ORDER BY Year"; break;
                case 4: query = "SELECT SectorName, COUNT(Id) AS EnterpriseCount FROM Enterprises GROUP BY SectorName"; break;
            }

            try
            {
                DataTable reportData = DatabaseService.ExecuteQuery(query);
                ReportGrid.ItemsSource = reportData.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private async void BtnRunAi_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtAiInput.Text.Trim())) return;

            AiProgress.Visibility = Visibility.Visible;
            AiResultPanel.Visibility = Visibility.Collapsed;
            BtnRunAi.IsEnabled = false;

            AiForecastResult result = await AiForecastService.GetForecastAsync(TxtAiInput.Text.Trim());

            if (result.IsSuccess)
            {
                TxtAiForecast.Text = result.ForecastText;
                TxtAiGrowth.Text = $"+{result.ProjectedGrpGrowthPercent:F1}%";
                TxtAiAbsolute.Text = result.ProjectedGrpAbsolute;
                ListRecommendations.ItemsSource = result.Recommendations;
                ListRisks.ItemsSource = result.Risks;
                AiResultPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage, "Ошибка AI");
            }

            AiProgress.Visibility = Visibility.Collapsed;
            BtnRunAi.IsEnabled = true;
        }
    }
}