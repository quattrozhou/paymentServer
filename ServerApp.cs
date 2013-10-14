using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Threading.Tasks;
using PaymentServer;

namespace PaymentServer
{

    public static class ServerApp
    {
        public enum clientOutgoingCodeEnum
        {
            /* Outgoing transaction message codes sent to mobile devices and web clients */

            OUT_CODE_INVALID = -1,
            OUT_CODE_LOGIN_SUCCESS = 0,
            OUT_CODE_LOGIN_FAILURE = 1,

            //all new codes should be placed above this line
            OUT_CODE_MAX
        };

        public enum clientIncomingCodeEnum
        {
            /* Incoming transaction message codes received from mobile devices and web clients */

            IN_CODE_INVALID = -1,
            IN_CODE_LOGIN_REQ = 0,

            //all new codes should be placed above this line
            IN_CODE_MAX
        };

        public static void handleRequest(HttpProcessor p, StreamReader inputData, string method){
            JsonObjectCollection headers;
            JsonObjectCollection messageType;
            JsonObjectCollection user;
            JsonObjectCollection merchant;
            JsonObjectCollection customer;
            JsonObjectCollection transactions;

            //Define outgoing JSON message structures 
            headers = new JsonObjectCollection();
            headers.Name = "headers";
            headers.Add(new JsonStringValue("Accept-Encoding", "gzip,deflate,sdch"));
            headers.Add(new JsonStringValue("Cookie", "_gauges_unique_month=1; _gauges_unique_year=1; _gauges_unique=1"));
            headers.Add(new JsonStringValue("Accept-Language", "en-CA,en-GB,en-US;q=0.8,en;q=0.6"));
            headers.Add(new JsonStringValue("Accept", "application/json, text/json"));
            headers.Add(new JsonStringValue("Host", "paymentserver.dynu.com"));
            headers.Add(new JsonStringValue("Referer", "https://paymentserver.dynu.com"));
            headers.Add(new JsonStringValue("Connection", "close"));
            headers.Add(new JsonStringValue("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"));

            JsonNumericValue code = new JsonNumericValue("code", -1);
            JsonBooleanValue request = new JsonBooleanValue("request", false);
            JsonBooleanValue response = new JsonBooleanValue("response", false);
            JsonStringValue details = new JsonStringValue("details", "");
            messageType = new JsonObjectCollection();
            messageType.Name = "messageType";
            messageType.Add(code);
            messageType.Add(request);
            messageType.Add(response);
            messageType.Add(details);

            JsonObjectCollection account = new JsonObjectCollection();
            account.Name = "account";
            account.Add(new JsonStringValue("bankCode", ""));
            account.Add(new JsonStringValue("accountNum", ""));
            account.Add(new JsonStringValue("accountPWD", ""));
            account.Add(new JsonNumericValue("acctBalance", -1));

            JsonObjectCollection hardwareInfo = new JsonObjectCollection();
            hardwareInfo.Name = "hardwareInfo";
            hardwareInfo.Add(new JsonNumericValue("POSHWID", -1));
            hardwareInfo.Add(new JsonStringValue("currentDK", ""));
            hardwareInfo.Add(new JsonStringValue("nextDK", ""));

            JsonObjectCollection userID = new JsonObjectCollection();
            userID.Name = "userID";
            userID.Add(new JsonStringValue("username", ""));
            userID.Add(new JsonStringValue("password", ""));

            user = new JsonObjectCollection();
            user.Name = "user";
            user.Add(new JsonStringValue("userType", ""));
            user.Add(new JsonStringValue("transactionHistory", ""));
            user.Add(account);
            user.Add(hardwareInfo);
            user.Add(userID);

            merchant = new JsonObjectCollection();
            merchant.Name = "merchant";
            merchant.Add(new JsonNumericValue("merchantID", -1));
            merchant.Add(new JsonStringValue("merchantName", ""));

            customer = new JsonObjectCollection();
            customer.Name = "customer";
            customer.Add(new JsonStringValue("custUsername", ""));
            customer.Add(new JsonStringValue("custPWD", ""));


            JsonObjectCollection transactionDate = new JsonObjectCollection();
            transactionDate.Name = "transactionDate";
            transactionDate.Add(new JsonNumericValue("year", -1));
            transactionDate.Add(new JsonNumericValue("month", -1));
            transactionDate.Add(new JsonNumericValue("day", -1));

            JsonObjectCollection transactionTime = new JsonObjectCollection();
            transactionTime.Name = "transactionTime";
            transactionTime.Add(new JsonNumericValue("hour", -1));
            transactionTime.Add(new JsonNumericValue("minute", -1));
            transactionTime.Add(new JsonNumericValue("second", -1));

            JsonObjectCollection merchantID = new JsonObjectCollection();
            merchantID.Name = "merchantID";
            merchantID.Add(new JsonStringValue("username", ""));

            transactions = new JsonObjectCollection();
            transactions.Name = "transactions";
            transactions.Add(new JsonNumericValue("transactionID", -1));
            transactions.Add(new JsonNumericValue("debitAmount", -1));
            transactions.Add(new JsonNumericValue("creditAmount", -1));
            transactions.Add(new JsonNumericValue("balance", -1));
            transactions.Add(new JsonNumericValue("receiptNo", -1));
            transactions.Add(transactionDate);
            transactions.Add(transactionTime);
            transactions.Add(merchantID);

            //create JSON object
            JsonObjectCollection defineResponse = new JsonObjectCollection();
            

            if (method == "GET")
            {
                Console.WriteLine("GET request: {0}", p.http_url);

                //build response content from already defined JSON Objects
                defineResponse.Insert(0, headers);
                defineResponse.Add(messageType);
                defineResponse.Add(user);
                defineResponse.Add(merchant);
                defineResponse.Add(customer);
                defineResponse.Add(transactions);
            
            }

            /* 
             * Handle 'POST' message. Requests form the mobile application are handled here 
             */
            if (method == "POST")
            {
                Console.WriteLine("POST request: {0}", p.http_url);

                //parse the input data
                string data = inputData.ReadToEnd();
                JObject received = JObject.Parse(data);
                int transactionCode = (int)received.SelectToken("code");
                Console.WriteLine("Transaction code: {0}", transactionCode);

                //Determine anad handle the received transaction code
                switch (transactionCode)
                {
                    case ((int)clientIncomingCodeEnum.IN_CODE_LOGIN_REQ):
                        string username = (string)received.SelectToken("custUsername");
                        string password = (string)received.SelectToken("custPWD");
                        Console.WriteLine("custUsename: {0}", username);
                        Console.WriteLine("custPWD: {0}", password);

                        //JT: The authentication logic shoud come here. For now we will authenticate all requests
                        if (true)  //JT:HACK - This should evaluate logic that checks if the user is authentiicated or not
                        {
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_SUCCESS));
                            messageType = insert(messageType, response,  new JsonBooleanValue("response", true));
                            messageType = insert(messageType, details,  new JsonStringValue("details", "Success"));
                        }
                        else{
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_FAILURE));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Invalid username and passowrd combination"));
                        }
                        //build response content from already defined JSON Objects               
                        defineResponse.Insert(0, headers);
                        defineResponse.Add(messageType);

                        Console.WriteLine("Line 3");
                        JObject resp = JObject.Parse(defineResponse.ToString());
                        Console.WriteLine("resp: \n{0}", defineResponse.ToString());
                        //
                        string respCode = (string)resp.SelectToken("code");
                        string respResponse = (string)resp.SelectToken("response");
                        string respDetails = (string)resp.SelectToken("details");
                        Console.WriteLine("code to mobile: {0}", respCode);
                        Console.WriteLine("reponse to mobile: {0}", respResponse);
                        Console.WriteLine("details to mobile: {0}", respDetails);
                        break;

                }
            }

            //finalize ougoing JSON message
            JsonObjectCollection completeResponse = new JsonObjectCollection();
            completeResponse.Add(defineResponse);

            //Write message to client
            byte[] message = JsonStringToByteArray(completeResponse.ToString());
            p.sslStream.Write(message);
        }

        public static void handleMobileRequest(HttpProcessor p, StreamReader inputData)
        {

        }
        public static void handleWebRequest(HttpProcessor p, StreamReader inputData)
        {

        }


        public static byte[] JsonStringToByteArray(string jsonString)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(jsonString.Substring(1, jsonString.Length - 2));
        }

        public static JsonObjectCollection insert(JsonObjectCollection obj, JsonObject item, JsonObject newItem){
            obj.Remove(item);
            obj.Add( newItem);
            return obj;
        }
    }
}
