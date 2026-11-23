using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RegionForecastSystem
{
    // Класс для хранения строки из DataGrid
    public class ColumnDefinition
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public partial class TableConstructorWindow : Window
    {
        public TableConstructorWindow()
        {
            InitializeComponent();
        }

        private void BtnCreateTable_Click(object sender, RoutedEventArgs e)
        {
            string tableName = TxtTableName.Text.Trim();

            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Введите имя таблицы!", "Ошибка");
                return;
            }

            var columns = new List<ColumnDefinition>();
            foreach (var item in ColumnsGrid.Items)
            {
                if (item is ColumnDefinition col && !string.IsNullOrEmpty(col.ColumnName))
                {
                    columns.Add(col);
                }
            }

            if (columns.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну колонку!", "Ошибка");
                return;
            }

            StringBuilder sql = new StringBuilder($"CREATE TABLE {tableName} (\n");

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                sql.Append($"    {col.ColumnName} {col.DataType}");
                if (col.IsPrimaryKey) sql.Append(" PRIMARY KEY");

                if (i < columns.Count - 1) sql.Append(",\n");
            }
            sql.Append("\n);");

            string finalSql = sql.ToString();

            if (AppConfig.UseFakeDatabase)
            {
                MessageBox.Show($"SQL-запрос успешно сгенерирован (режим эмуляции БД):\n\n{finalSql}", "Успех");
            }
            else
            {
                try
                {
                    DatabaseService.ExecuteNonQuery(finalSql);
                    MessageBox.Show($"Таблица '{tableName}' успешно создана в БД!", "Успех");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка создания таблицы: " + ex.Message, "Ошибка");
                }
            }
        }
    }
}