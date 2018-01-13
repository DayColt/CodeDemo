using LinkHandler.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace LinkHandler
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseConnector connector;
        private LinksTableModel[] unhandledLinks = null;
        private LinkAnalyzer analyzer;

        public MainWindow()
        {
            InitializeComponent();
            connector = new DatabaseConnector();
            analyzer = new LinkAnalyzer();
            analyzer.OnNewData += Analyzer_OnNewData;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connector.Dispose();
        }

        private void Analyzer_OnNewData(string obj)
        {
            ResultLB.Dispatcher.Invoke(new Action(() => ResultLB.Items.Add(obj))); // Для синхронизации потока в UI элементе
        }

        /// <summary>
        /// Выводит все записи из таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowAllBtn_Click(object sender, RoutedEventArgs e)
        {
            DatabaseLB.Items.Clear();
            LinksTableModel[] records = await connector.GetAllRecordsAsync("Links");
            for (int i = 0; i < records.Length; i++) DatabaseLB.Items.Add(records[i].ToString());
        }

        /// <summary>
        /// Выводит только те, что не обработаны
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowUnhandledBtn_Click(object sender, RoutedEventArgs e)
        {
            DatabaseLB.Items.Clear();
            LinksTableModel[] records = await connector.GetUnhandledRecordsAsync("Links");
            for (int i = 0; i < records.Length; i++) DatabaseLB.Items.Add(records[i].ToString());
            unhandledLinks = records;
        }

        /// <summary>
        /// Запускает обработку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HanldeBtn_Click(object sender, RoutedEventArgs e)
        {
            HanldeBtn.IsEnabled = false;
            if (unhandledLinks == null) unhandledLinks = await connector.GetUnhandledRecordsAsync("Links");
            if (unhandledLinks.Length == 0)
            {
                MessageBox.Show("Nothing to handle", "Database Info", MessageBoxButton.OK, MessageBoxImage.Information);
                HanldeBtn.IsEnabled = true;
                return;
            }
            Task<bool>[] tasks = new Task<bool>[unhandledLinks.Length];

            for (int i = 0; i < unhandledLinks.Length; i++)
            {
                Task<bool> task = analyzer.HandleHtmlAsync(unhandledLinks[i].Link, QueryTB.Text);
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);

            List<int> IDs = new List<int>();
            for (int i = 0; i < tasks.Length; i++)
            {
                unhandledLinks[i].IsHandled = tasks[i].Result;
                if (unhandledLinks[i].IsHandled == true) IDs.Add(unhandledLinks[i].ID);
            }
            await connector.SetAsHandledAsync(IDs.ToArray(), "Links");
            HanldeBtn.IsEnabled = true;
            unhandledLinks = null;
            MessageBox.Show("All Done!", "Database Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
