using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace LinkHandler.Core
{
    class DatabaseConnector : IDisposable
    {
        private const string connString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\Database\HandlerDatabase.mdb;Persist Security Info=True";
        private OleDbConnection connection;

        /// <summary>
        /// Инициализирует соединение с БД
        /// </summary>
        public DatabaseConnector()
        {
            connection = new OleDbConnection(connString);
            connection.Open();
        }

        /// <summary>
        /// Возвращает все записи из таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public LinksTableModel[] GetAllRecords(string tableName)
        {
            List<LinksTableModel> result = QueryExecutor($"SELECT * FROM {tableName}");
            return result.ToArray();
        }

        /// <summary>
        /// Асинхронно возвращает все записи из таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<LinksTableModel[]> GetAllRecordsAsync(string tableName)
        {
            LinksTableModel[] result = await Task.Run(() => GetAllRecords(tableName));
            return result;
        }

        /// <summary>
        /// Возвращает все необработанные записи из таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public LinksTableModel[] GetUnhadledRecords(string tableName)
        {
            List<LinksTableModel> result = QueryExecutor($"SELECT * FROM {tableName} WHERE {tableName}.Handled = False");
            return result.ToArray();
        }

        /// <summary>
        /// Асинхронно возвращает все необработанные записи из таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<LinksTableModel[]> GetUnhandledRecordsAsync(string tableName)
        {
            LinksTableModel[] result = await Task.Run(() => GetUnhadledRecords(tableName));
            return result;
        }

        /// <summary>
        /// Помечает данные записи как обработанные
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="tableName"></param>
        public void SetAsHandled(int[] IDs, string tableName)
        {
            for (int i = 0; i < IDs.Length; i++)
            {
                SetQueryExecutor($"UPDATE {tableName} SET {tableName}.Handled = True WHERE {tableName}.ID = {IDs[i]}");
            }
        }

        /// <summary>
        /// Асинхронно помечает данные записи как обработанные
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task SetAsHandledAsync(int[] IDs, string tableName)
        {
            await Task.Run(() => SetAsHandled(IDs, tableName));
        }

        /// <summary>
        /// Закрывает соединение, освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }

        private List<LinksTableModel> QueryExecutor(string query)
        {
            List<LinksTableModel> result = new List<LinksTableModel>();
            using (OleDbCommand command = new OleDbCommand())
            {
                command.CommandText = query;
                command.Connection = connection; // Initialize command

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new LinksTableModel()
                        {
                            ID = Convert.ToInt32(reader[0]),
                            Link = reader[1].ToString(),
                            IsHandled = Convert.ToBoolean(reader[2])
                        });
                    }
                }
            }
            return result;
        }

        private void SetQueryExecutor(string query)
        {
            using (OleDbCommand command = new OleDbCommand())
            {
                command.CommandText = query;
                command.Connection = connection; // Initialize command
                command.ExecuteNonQuery();
            }
        }
    }
}
