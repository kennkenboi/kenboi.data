using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenboi.Data;
using Kenboi.Data.Utils;
using MySql.Data.MySqlClient;

namespace Kenboi.Data
{

    public class OurSql
    {

        private readonly MySqlConnection _connection;
        public string Error { get; set; }

        public Action<string> OnMySqlErrorAction {private get; set; }

        public OurSql(string username, string server, string password, string database)
        {
            string connStr = $"server={server};user={username};database={database};port=3306;password={password}";
            _connection = new MySqlConnection(connStr);
        }

        public MyResponse ExecuteSelect(string query)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            if (OpenConnection())
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    Dictionary<string, string> val = new Dictionary<string, string>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        val.Add(dataReader.GetName(i), dataReader[i] + "");
                    }
                    list.Add(val);
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                CloseConnection();


            }

            return new MyResponse()
            {
                Error = Error,
                Response = list,
                Success = Error == null
            };
        }

        public MyResponse ExecuteSelectGetDataTable(string query, bool withChooseColumn = true)
        {
            DataTable dataTable = new DataTable();
            if (OpenConnection())
            {

                MySqlDataAdapter adapter = new MySqlDataAdapter(query, _connection);
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(adapter);

                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet, "DATASET");
                if (withChooseColumn)
                {
                    dataTable.Columns.Add(new DataColumn("Choose", typeof(bool)));
                }

                adapter.Fill(dataTable);
                if (withChooseColumn)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        row["Choose"] = true;
                    }
                }

                //close Connection
                CloseConnection();

            }

            return new MyResponse()
            {
                Error = Error,
                Response = dataTable,
                Success = Error == null
            };
        }

        public MyResponse ExecuteNonQuery(string query, bool isInsert = false)
        {
            string lastInsertId = string.Empty;
            ;
            if (OpenConnection())
            {
                MySqlCommand myCommand = new MySqlCommand(query, _connection);
                myCommand.ExecuteNonQuery();

                //get insert id
                if (isInsert)
                {
                    lastInsertId = $"{myCommand.LastInsertedId}";
                }

                CloseConnection();
            }

            return new MyResponse()
            {
                Error = Error,
                Response = lastInsertId,
                Success = Error == null
            };
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Error = "Cannot connect to server.  Contact administrator";
                        break;

                    case 1045:
                        Error = "Invalid username/password, please try again";
                        break;
                }
                Error = ex.Message;
                OnMySqlError(Error);
                return false;
            }
        }

        protected void OnMySqlError(string error)
        {
            OnMySqlErrorAction?.Invoke(error);
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                OnMySqlError(ex.Message);
                return false;
            }
        }

    }
}
