using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Kenboi.Data
{
    public class MyApi
    {

        public string BaseUrl { get; set; }

        public RestClient Client;
        public RestRequest Request;
        public IRestResponse Response;

        public Action<string> OnHttpErrorAction { private get; set; } 

        private string _accessToken;


        public MyApi(string baseUrl = null, string accessToken = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            BaseUrl = baseUrl ?? string.Empty;
            _accessToken = accessToken ?? string.Empty;

            Client = new RestClient(BaseUrl);
        }


        public void OnHttpError(string error)
        {
            OnHttpErrorAction?.Invoke(error);
        }

        public void SendRequestAsync(string param, string route, Method method, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror, Dictionary<string, string> headers)
        {
            CreateRequest(param, route, method, headers);
            ExecuteAsync(onsuccess, onerror);
        }

        public void SendRequestAsync(Dictionary<string, string> param, string route, Method method, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror, Dictionary<string, string> headers)
        {
            CreateRequest(param, route, method, headers);
            ExecuteAsync(onsuccess, onerror);
        }

        public void SendRequestAsync<T>(T param, string route, Method method, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror, Dictionary<string, string> headers)
        {
            CreateRequest(param, route, method, headers);
            ExecuteAsync(onsuccess, onerror);
        }
        public void SendRequest<T>(T param, string route, Method method, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror, Dictionary<string, string> headers)
        {
            CreateRequest(param, route, method, headers);
            Execute(onsuccess, onerror);
        }


        public void SendRequest(string param, string route, Method method, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror, Dictionary<string, string> headers)
        {
            CreateRequest(param, route, method, headers);
            Execute(onsuccess, onerror);
        }

        public void SendRequest(Dictionary<string, string> param, string route, Method method, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror, Dictionary<string, string> headers)
        {
            CreateRequest(param, route, method, headers);
            Execute(onsuccess, onerror);
        }

        private void AddHeaders(Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> item in headers)
                {
                    Request.AddHeader(item.Key, item.Value);
                }
            }
        }
        private void AddParameters(Dictionary<string, string> parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> item in parameters)
                {
                    Request.AddParameter(item.Key, item.Value);
                }
            }
        }

        private void CreateRequest(object param, string route, Method method, Dictionary<string, string> headers)
        {
            Request = new RestRequest(route, method);
            AddHeaders(headers);

            if (param is Dictionary<string, string> dictionary)
            {
                AddParameters(dictionary);
            }
            else if (param is string)
            {
                Request.AddParameter("text/plain", param, ParameterType.RequestBody);
            }
            else if (param is JObject || param is JArray)
            {
                //Cast the object to either jarray string or jobject string
                string par = param is JObject o ? o.ToString() : ((JArray)param).ToString();
                Request.AddParameter("application/json", par, ParameterType.RequestBody);
            }
            else
            {
                Request.AddObject(param);
            }

            Request.AddHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(_accessToken))
            {
                Request.AddHeader("Authorization", _accessToken);
            }
        }


        private void ExecuteAsync(Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror)
        {
            Client.ExecuteAsync(Request, response =>
            {
                ProcessRequest(response, onsuccess, onerror);
                LogRequest(response);
            });
        }

        private void Execute(Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror)
        {
            IRestResponse response = Client.Execute(Request);
            //on successfull request
            ProcessRequest(response, onsuccess, onerror);
            LogRequest(response);

        }

        private void ProcessRequest(IRestResponse response, Action<string, HttpStatusCode> onsuccess, Action<string, HttpStatusCode> onerror)
        {
            //on successfull request
            if ((response.StatusCode >= HttpStatusCode.OK && response.StatusCode < HttpStatusCode.BadRequest) || (int)response.StatusCode == 422)
            {
                onsuccess.Invoke(response.Content, response.StatusCode);
            }
            else if (response.StatusCode == HttpStatusCode.NotAcceptable)
            {
                onerror.Invoke(response.Content, response.StatusCode);
            }
            //on failed request exception cought
            else if (response.ErrorException != null)
            {
                onerror.Invoke("-An exception occurred while processing request: " + response.ErrorMessage, response.StatusCode);

            }  //Failed but no exception found
            else
            {
                onerror.Invoke("-> Server responded with unsuccessful status code: " + response.StatusDescription, response.StatusCode);

            }
        }

        private void LogRequest(IRestResponse response)
        {
            var requestToLog = new
            {
                resource = Request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = Request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = Request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = Client.BuildUri(Request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            string log =
                $"Request: {JsonConvert.SerializeObject(requestToLog)}, {Environment.NewLine}Response: {JsonConvert.SerializeObject(responseToLog)}";

            if (!(response.StatusCode >= HttpStatusCode.OK && response.StatusCode < HttpStatusCode.BadRequest))
            {
                OnHttpError(log);
            }

            Trace.Write(
                $"Request completed in {1000} ms, \nRequest: {JsonConvert.SerializeObject(requestToLog)}, \nResponse: {JsonConvert.SerializeObject(responseToLog, Formatting.Indented)}");
        }

        public void UpdateToken(string token)
        {
            _accessToken = token;
        }
    }
}
