﻿using System;
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
            JsonStringValue Accept_Encoding = new JsonStringValue("Accept-Encoding", "gzip,deflate,sdch");
            JsonStringValue Cookie = new JsonStringValue("Cookie", "_gauges_unique_month=1; _gauges_unique_year=1; _gauges_unique=1");
            JsonStringValue Accept_Language = new JsonStringValue("Accept-Language", "en-CA,en-GB,en-US;q=0.8,en;q=0.6");
            JsonStringValue Accept = new JsonStringValue("Accept", "application/json, text/json");
            JsonStringValue Host = new JsonStringValue("Host", "paymentserver.dynu.com");
            JsonStringValue Referer = new JsonStringValue("Referer", "https://paymentserver.dynu.com");
            JsonStringValue Connection = new JsonStringValue("Connection", "close");
            JsonStringValue User_Agent = new JsonStringValue("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36");
            headers = new JsonObjectCollection();
            headers.Name = "headers";
            headers.Add(Accept_Encoding);
            headers.Add(Cookie);
            headers.Add(Accept_Language);
            headers.Add(Accept);
            headers.Add(Host);
            headers.Add(Referer);
            headers.Add(Connection);
            headers.Add(User_Agent);

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

            JsonStringValue bankCode = new JsonStringValue("bankCode", "");
            JsonStringValue accountNum = new JsonStringValue("accountNum", "");
            JsonStringValue accountPWD = new JsonStringValue("accountPWD", "");
            JsonNumericValue acctBalance = new JsonNumericValue("acctBalance", -1);
            JsonObjectCollection account = new JsonObjectCollection();
            account.Name = "account";
            account.Add(bankCode);
            account.Add(accountNum);
            account.Add(accountPWD);
            account.Add(acctBalance);

            JsonNumericValue POSHWID = new JsonNumericValue("POSHWID", -1);
            JsonStringValue currentDK = new JsonStringValue("currentDK", "");
            JsonStringValue nextDK = new JsonStringValue("nextDK", "");
            JsonObjectCollection hardwareInfo = new JsonObjectCollection();
            hardwareInfo.Name = "hardwareInfo";
            hardwareInfo.Add(POSHWID);
            hardwareInfo.Add(currentDK);
            hardwareInfo.Add(nextDK);

            JsonStringValue username = new JsonStringValue("username", "");
            JsonStringValue password = new JsonStringValue("password", "");
            JsonObjectCollection userID = new JsonObjectCollection();
            userID.Name = "userID";
            userID.Add(username);
            userID.Add(password);

            JsonStringValue userType = new JsonStringValue("userType", "");
            JsonStringValue transactionHistory = new JsonStringValue("transactionHistory", "");
            user = new JsonObjectCollection();
            user.Name = "user";
            user.Add(userType);
            user.Add(transactionHistory);
            user.Add(account);
            user.Add(hardwareInfo);
            user.Add(userID);

            JsonNumericValue merchantID = new JsonNumericValue("merchantID", -1);
            JsonStringValue merchantName = new JsonStringValue("merchantName", "");
            merchant = new JsonObjectCollection();
            merchant.Name = "merchant";
            merchant.Add(merchantID);
            merchant.Add(merchantName);

            JsonStringValue custUsername = new JsonStringValue("custUsername", "");
            JsonStringValue custPWD = new JsonStringValue("custPWD", "");
            customer = new JsonObjectCollection();
            customer.Name = "customer";
            customer.Add(custUsername);
            customer.Add(custPWD);

            JsonNumericValue year = new JsonNumericValue("year", -1);
            JsonNumericValue month = new JsonNumericValue("month", -1);
            JsonNumericValue day = new JsonNumericValue("day", -1);
            JsonObjectCollection transactionDate = new JsonObjectCollection();
            transactionDate.Name = "transactionDate";
            transactionDate.Add(year);
            transactionDate.Add(month);
            transactionDate.Add(day);

            JsonNumericValue hour = new JsonNumericValue("hour", -1);
            JsonNumericValue minute = new JsonNumericValue("minute", -1);
            JsonNumericValue second = new JsonNumericValue("second", -1);
            JsonObjectCollection transactionTime = new JsonObjectCollection();
            transactionTime.Name = "transactionTime";
            transactionTime.Add(hour);
            transactionTime.Add(minute);
            transactionTime.Add(second);

            JsonStringValue merchantUsername = new JsonStringValue("merchantUsername", "");
            JsonStringValue merchantPWD = new JsonStringValue("merchantPWD", "");
            JsonObjectCollection merchantIdent = new JsonObjectCollection();
            merchantIdent.Name = "merchantIdent";
            merchantIdent.Add(merchantUsername);
            merchantIdent.Add(merchantPWD);

            JsonNumericValue transactionID = new JsonNumericValue("transactionID", -1);
            JsonNumericValue debitAmount = new JsonNumericValue("debitAmount", -1);
            JsonNumericValue creditAmount = new JsonNumericValue("creditAmount", -1);
            JsonNumericValue balance = new JsonNumericValue("balance", -1);
            JsonNumericValue receiptNo = new JsonNumericValue("receiptNo", -1);
            transactions = new JsonObjectCollection();
            transactions.Name = "transactions";
            transactions.Add(transactionID);
            transactions.Add(debitAmount);
            transactions.Add(creditAmount);
            transactions.Add(balance);
            transactions.Add(receiptNo);
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
                defineResponse.Add(merchantIdent);
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
                        string uName = (string)received.SelectToken("custUsername");
                        string PWD = (string)received.SelectToken("custPWD");
                        Console.WriteLine("custUsename: {0}", uName);
                        Console.WriteLine("custPWD: {0}", PWD);

                        //JT: The authentication logic shoud come here. For now we will authenticate all requests
                        if (true)  //JT:HACK - This should evaluate logic that checks if the user is authentiicated or not
                        {
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_SUCCESS));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Success"));
                        }
                        else{
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_FAILURE));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Invalid username and passowrd combination"));
                        }
                        //build response content from already defined JSON Objects               
                        defineResponse.Insert(0, headers);
                        defineResponse.Add(messageType);

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