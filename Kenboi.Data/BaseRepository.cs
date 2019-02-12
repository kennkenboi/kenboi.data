using MySql.Data.MySqlClient;
using RestSharp;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;
using Kenboi.Data;
using Newtonsoft.Json;
using RestSharp.Serialization.Json;

namespace Kenboi.Data
{
    public abstract class BaseRepository
    {
        protected Task<MyResponse> FuncSendRequest<T>(T param, string route, Method method, bool isList = false, Dictionary<string, string> headers = null)
        {
           return Task.Run(() =>
            {
                dynamic response = null;
                HttpStatusCode statusCode = HttpStatusCode.Ambiguous;
                bool success = true;

                void OnSuccess(string val, HttpStatusCode code)
                {
                    statusCode = code;
                    if (isList)
                    {
                        response = JsonConvert.DeserializeObject<List<ExpandoObject>>(val);
                        return;
                    }

                    response = JsonConvert.DeserializeObject<ExpandoObject>(val);
                }

                void OnFail(string val, HttpStatusCode code)
                {
                    response = val;
                    statusCode = code;
                    success = false;
                }

                GetApiInstance().SendRequest(param, route, method, OnSuccess, OnFail, headers);

                return new MyResponse {Response = response, HttpStatusCode = statusCode, Success = success};
            });
        }

        public abstract MyApi GetApiInstance();

        protected Task<MyResponse> ExecuteSelect(string query, bool dataTable = false, bool withChooseColumn = true)
        {
            return Task.Run(() => dataTable ? BuildOurSql().ExecuteSelectGetDataTable(query, withChooseColumn) : BuildOurSql().ExecuteSelect(query));
        }

        protected Task<MyResponse> ExecuteNonQuery(string query, bool isInsert = false)
        {
            return Task.Run((() => BuildOurSql().ExecuteNonQuery(query, isInsert)));
        }

        public abstract OurSql BuildOurSql();

        protected string BuildInsertQuery(Dictionary<string, string> data, string table)
        {
            string columns = "";
            string values = "";
            foreach (KeyValuePair<string, string> keyVal in data)
            {
                columns += $"`{keyVal.Key}`,";
                values += $"'{EscapeString(keyVal.Value)}',";
            }
            //remove trailing commas
            values = values.Substring(0, values.Length - 1);
            columns = columns.Substring(0, columns.Length - 1);
            return $"INSERT INTO `{table}` ({columns}) VALUES ({values})";
        }

        protected string BuildUpdateQuery(Dictionary<string, string> data, string table)
        {
            //UPDATE table_name SET field1 = new-value1, field2 = new-value2
            string values = string.Empty;
            foreach (KeyValuePair<string, string> keyVal in data)
            {
                values += $"`{keyVal.Key}`='{EscapeString(keyVal.Value)}',";
            }
            //remove trailing commas
            values = values.Substring(0, values.Length - 1);
            return $"UPDATE `{table}` SET {values}";
        }

        protected string EscapeString(string str)
        {
            return MySqlHelper.EscapeString(str);
        }
    }



}
