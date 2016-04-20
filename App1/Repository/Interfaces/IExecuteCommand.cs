namespace App1.Repository.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IExecuteCommand
    {
        void ExecuteNonQuery(string query);

        void ExecuteNonQuery(string query, params object[] parameters);

        void AddTable();

        void AddColumn(string columnName, string columnType);

        bool ExistColumn(string columnName);

        bool ExistColumn(string columnName, Type type);

        bool TableExists();

        Task<bool> ExistDatabase();
    }
}
