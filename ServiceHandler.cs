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

public enum ResultCodeType
{
    ERROR_UNKNOWN = -1,
    RESULT_CREATE_PROFILE_SUCCESS = 0,
    ERROR_CREATE_PROFILE_USERNAME_TAKEN = 1,
    ERROR_CREATE_PROFILE_UNSUPPORTED_INSTITUTION = 2,
    ERROR_CREATE_PROFILE_INVALID_BANK_ACCT_NUM = 3,
    ERROR_CREATE_PROFILE_INVALID_BANK_CODE = 4,
    ERROR_CREATE_PROFILE_ACCOUNT_EXISTS = 5,
    UPDATE_USER_PROFILE_SUCCESS = 6,
    ERROR_UPDATE_USER_PROFILE = 7,
    //all new codes should be placed above this line
    ERROR_CREATE_PROFILE_MAX
};

/* Outgoing transaction message codes sent to mobile devices and web clients */
public enum clientOutgoingCodeEnum
{
    OUT_CODE_INVALID = -1,
    OUT_CODE_LOGIN_SUCCESS = 0,
    OUT_CODE_LOGIN_FAILURE = 1,
    OUT_CODE_SIGN_UP_SUCCESS = 2,
    OUT_CODE_SIGN_UP_FAILURE = 3,
    OUT_CODE_SEND_USER_PROFILE_SUCCESS = 4,
    OUT_CODE_SEND_USER_PROFILE_FAILURE = 5,
    //all new codes should be placed above this line
    OUT_CODE_MAX
};

/* Incoming transaction message codes received from mobile devices and web clients */
public enum clientIncomingCodeEnum
{
    IN_CODE_INVALID = -1,
    IN_CODE_LOGIN_REQ = 0,
    IN_CODE_SIGN_UP_REQ = 1,
    IN_CODE_GET_USER_PROFILE = 2,
    //all new codes should be placed above this line
    IN_CODE_MAX
};


namespace PaymentServer
{

    public class ServiceHandler
    {
        

        public static void handleRequest(HttpProcessor p, StreamReader inputData, string method){
            MySQLDataHandler DBHandler = new MySQLDataHandler();

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

            JsonNumericValue DOBDay = new JsonNumericValue("DOBDay", -1);
            JsonNumericValue DOBMonth = new JsonNumericValue("DOBMonth", -1);
            JsonNumericValue DOBYear = new JsonNumericValue("DOBYear", -1);
            JsonObjectCollection dateOfBirth = new JsonObjectCollection();
            dateOfBirth.Name = "dateOfBirth";
            dateOfBirth.Add(DOBDay);
            dateOfBirth.Add(DOBMonth);
            dateOfBirth.Add(DOBYear);
           
            JsonStringValue firstName = new JsonStringValue("firstName", "");
            JsonStringValue middleName = new JsonStringValue("middleName", "");
            JsonStringValue lastName = new JsonStringValue("lastName", "");
            JsonStringValue occupation = new JsonStringValue("occupation", "");
            JsonNumericValue SIN = new JsonNumericValue("SIN", -1);
            JsonStringValue address1 = new JsonStringValue("address1", "");
            JsonStringValue address2 = new JsonStringValue("address2", "");
            JsonStringValue city = new JsonStringValue("city", "");
            JsonStringValue province = new JsonStringValue("province", "");
            JsonStringValue country = new JsonStringValue("country", "");
            JsonStringValue postalCode = new JsonStringValue("postalCode", "");
            JsonStringValue email = new JsonStringValue("email", "");
            JsonNumericValue phoneNumber = new JsonNumericValue("phoneNumber", -1);
            JsonObjectCollection personalInfo = new JsonObjectCollection();
            personalInfo.Name = "personalInfo";
            personalInfo.Add(firstName);
            personalInfo.Add(lastName);
            personalInfo.Add(email);
            personalInfo.Add(dateOfBirth);
            personalInfo.Add(occupation);
            personalInfo.Add(SIN);
            personalInfo.Add(address1);
            personalInfo.Add(address2);
            personalInfo.Add(city);
            personalInfo.Add(province);
            personalInfo.Add(country);
            personalInfo.Add(postalCode);
            personalInfo.Add(phoneNumber);
            

            JsonStringValue username = new JsonStringValue("username", "");
            JsonStringValue password = new JsonStringValue("password", "");
            JsonObjectCollection userID = new JsonObjectCollection();
            userID.Name = "userID";
            userID.Add(username);
            userID.Add(password);

            JsonBooleanValue receiveCommunication = new JsonBooleanValue("receiveCommunication", false);
            JsonStringValue userType = new JsonStringValue("userType", "");
            JsonNumericValue userNo = new JsonNumericValue("userNo", -1);
            JsonStringValue transactionHistory = new JsonStringValue("transactionHistory", "");
            user = new JsonObjectCollection();
            user.Name = "user";
            user.Add(userNo);
            user.Add(userType);
            user.Add(transactionHistory);
            user.Add(receiveCommunication);
            user.Add(account);
            user.Add(hardwareInfo);
            user.Add(userID);
            user.Add(personalInfo);

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
                JObject msgType = (JObject)received.SelectToken("messageType");
                int transactionCode = (int)msgType.SelectToken("code");
                Console.WriteLine("Transaction code: {0}", transactionCode);

                //Determine anad handle the received transaction code
                switch (transactionCode)
                {
                    /*
                     * handle user authentication request
                     */
                    case ((int)clientIncomingCodeEnum.IN_CODE_LOGIN_REQ):
                        JObject cust = (JObject)received.SelectToken("customer");
                        string authString = "";
                        string uName = (string)cust.SelectToken("custUsername");
                        string PWD = (string)cust.SelectToken("custPWD");
                        authString += uName;
                        authString += PWD;
                        Console.WriteLine("custUsename: {0}", uName);
                        Console.WriteLine("custPWD: {0}", PWD);

                        //Call the ServerWorker function
                        if (ServerWorker.authenticateUser(DBHandler, authString))  
                        {
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_SUCCESS));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Authentication Successful"));
                        }
                        else{
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_FAILURE));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Invalid username and passowrd combination"));
                        }
                        //build response message content from already defined JSON Objects               
                        defineResponse.Insert(0, headers);
                        defineResponse.Add(messageType);          
                        break;
                     
                    /*
                     * handle new user sign-up request
                     */
                    case ((int)clientIncomingCodeEnum.IN_CODE_SIGN_UP_REQ):
                        UserProfile newProfile = new UserProfile();

                        //Retrieve encapsulated JSON objects from message
                        JObject newUser = (JObject)received.SelectToken("user");
                        JObject acct = (JObject)newUser.SelectToken("account");
                        JObject UID = (JObject)newUser.SelectToken("userID");
                        JObject DOB = (JObject)newUser.SelectToken("dateOfBirth");

                        //Populate the newProfile object with the information received from the client
                        newProfile.userType = (string)newUser.SelectToken("userType");
                        newProfile.receiveCommunication = (bool)newUser.SelectToken("receiveCommunication");
                        newProfile.bankCode = (string)acct.SelectToken("bankCode");
                        newProfile.accountNum = (string)acct.SelectToken("accountNum");
                        newProfile.accountPWD = (string)acct.SelectToken("accountPWD");
                        newProfile.acctBalance = (double)acct.SelectToken("acctBalance");
                        newProfile.username = (string)UID.SelectToken("username");
                        newProfile.password = (string)UID.SelectToken("password");
                        newProfile.firstName = (string)UID.SelectToken("firstName");
                        newProfile.lastName = (string)UID.SelectToken("lastName");
                        newProfile.DOBDay = (int)DOB.SelectToken("DOBDay");
                        newProfile.DOBMonth = (int)DOB.SelectToken("DOBMonth");
                        newProfile.DOBYear = (int)DOB.SelectToken("DOBYear");
                        newProfile.occupation = (string)UID.SelectToken("occupation");
                        newProfile.SIN = (int)UID.SelectToken("SIN");
                        newProfile.address1 = (string)UID.SelectToken("address1");
                        newProfile.address2 = (string)UID.SelectToken("address2");
                        newProfile.city = (string)UID.SelectToken("city");
                        newProfile.province = (string)UID.SelectToken("province");
                        newProfile.country = (string)UID.SelectToken("country");
                        newProfile.postalCode = (string)UID.SelectToken("postalCode");
                        newProfile.email = (string)UID.SelectToken("email");
                        newProfile.phoneNumber = (int)UID.SelectToken("phoneNumber");  
                        newProfile.authenticationString = "";
                        newProfile.authenticationString += newProfile.username;
                        newProfile.authenticationString += newProfile.password; 
                       
                        //pass the populated newProfile information to ServerWorker to try and create a new profile
                        //and build response message to client based on the return code receiveed from ServerWorker
                        if (ServerWorker.createNewProfile(DBHandler, newProfile) == ResultCodeType.RESULT_CREATE_PROFILE_SUCCESS)
                        {
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SIGN_UP_SUCCESS));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "User account created"));
                        }
                        else
                        {
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SIGN_UP_FAILURE));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Could not create profile. The email provided is already registered"));
                        }

                        //build response message content from already defined JSON Objects               
                        defineResponse.Insert(0, headers);
                        defineResponse.Add(messageType); 
                        break;

                    /*
                    * handle get user profile request
                    */
                    case ((int)clientIncomingCodeEnum.IN_CODE_GET_USER_PROFILE):



                        break;
                }
            }   

            //finalize outgoing JSON message
            JsonObjectCollection completeResponse = new JsonObjectCollection();
            JsonObjectCollection packagedResponse = new JsonObjectCollection();
            completeResponse.Add(defineResponse);
           // completeResponse = (JsonObjectCollection)defineResponse;
            //Write message to client
            Console.WriteLine("Response to Client: \n{0}", completeResponse.ToString());  
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

