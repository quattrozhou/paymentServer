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

    public class paymentServer_requestHandler
    {
        public static void handleRequest(HttpProcessor p, StreamReader inputData, string method){
            paymentServer_dataBase DBHandler = new paymentServer_dataBase();

            JsonObjectCollection headers = new JsonObjectCollection();
            JsonObjectCollection messageType = new JsonObjectCollection("messageType");
            JsonObjectCollection user = new JsonObjectCollection("user");
            JsonObjectCollection merchant = new JsonObjectCollection("merchant");
            JsonObjectCollection customer = new JsonObjectCollection("customer");
            JsonObjectCollection transactions = new JsonObjectCollection("transactions");

            //Define outgoing JSON message structures 
            JsonStringValue Accept_Encoding = new JsonStringValue("Accept-Encoding", "gzip,deflate,sdch");
            JsonStringValue Cookie = new JsonStringValue("Cookie", "_gauges_unique_month=1; _gauges_unique_year=1; _gauges_unique=1");
            JsonStringValue Accept_Language = new JsonStringValue("Accept-Language", "en-CA,en-GB,en-US;q=0.8,en;q=0.6");
            JsonStringValue Accept = new JsonStringValue("Accept", "application/json, text/json");
            JsonStringValue Host = new JsonStringValue("Host", "paymentserver.dynu.com");
            JsonStringValue Referer = new JsonStringValue("Referer", "https://paymentserver.dynu.com");
            JsonStringValue Connection = new JsonStringValue("Connection", "close");
            JsonStringValue User_Agent = new JsonStringValue("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36");
            headers.Name = "headers";
            headers.Add(Accept_Encoding);
            headers.Add(Cookie);
            headers.Add(Accept_Language);
            headers.Add(Accept);
            headers.Add(Host);
            headers.Add(Referer);
            headers.Add(Connection);
            headers.Add(User_Agent);

            //create JSON object
            JsonObjectCollection defineResponse = new JsonObjectCollection();     

            if (method == "GET")
            {
                Console.WriteLine("GET request: {0}", p.http_url);

                JsonNumericValue code = new JsonNumericValue("code", -1);
                JsonBooleanValue request = new JsonBooleanValue("request", false);
                JsonBooleanValue response = new JsonBooleanValue("response", false);
                JsonStringValue details = new JsonStringValue("details", "");
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
                personalInfo.Add(middleName);
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
                merchant.Name = "merchant";
                merchant.Add(merchantID);
                merchant.Add(merchantName);

                JsonStringValue custUsername = new JsonStringValue("custUsername", "");
                JsonStringValue custPWD = new JsonStringValue("custPWD", "");
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

                // JsonNumericValue transactionID = new JsonNumericValue("transactionID", -1);
                JsonNumericValue amount = new JsonNumericValue("amount", -1);
                JsonBooleanValue isRefund = new JsonBooleanValue("isRefund", false);
                JsonNumericValue balance = new JsonNumericValue("balance", -1);
                JsonStringValue receiptNo = new JsonStringValue("receiptNo", "");
                JsonStringValue bankReplyMessage = new JsonStringValue("bankReplyMessage", "");
                transactions.Name = "transactions";
                // transactions.Add(transactionID);
                transactions.Add(amount);
                transactions.Add(isRefund);
                transactions.Add(balance);
                transactions.Add(receiptNo);
                transactions.Add(transactionDate);
                transactions.Add(transactionTime);
                transactions.Add(merchantID);
                transactions.Add(bankReplyMessage);

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

                if (transactionCode == (int)clientIncomingCodeEnum.IN_CODE_GET_USER_PROFILE)
                    transactionCode = (int)clientIncomingCodeEnum.IN_CODE_LOGIN_REQ;

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
                        if (! paymentServer_requestWorker.authenticateUser(DBHandler, authString))
                        {
                            messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_FAILURE));
                            messageType.Add(new JsonBooleanValue("response", true));
                            messageType.Add(new JsonBooleanValue("request", false));
                            messageType.Add(new JsonStringValue("details", "Invalid username and passowrd combination"));

                            //build response message content from already defined JSON Objects               
                            defineResponse.Insert(0, headers);
                            defineResponse.Add(messageType);
                            break;
                        }

                        GetProfileResultType UserProf = paymentServer_requestWorker.MYgetUserProfileByUsername(DBHandler, uName);
                        if (UserProf.status != ResultCodeType.SUCC_GET_USER_PROFILE)
                        {
                            messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SEND_USER_PROFILE_FAILURE));
                            messageType.Add(new JsonBooleanValue("response", true));
                            messageType.Add(new JsonBooleanValue("request", false));
                            messageType.Add(new JsonStringValue("details", "Server error - Could not get profile data"));
                            
                            defineResponse.Insert(0, headers);
                            defineResponse.Add(messageType);
                            break;
                        }

                        messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_LOGIN_SUCCESS));
                        messageType.Add(new JsonBooleanValue("response", true));
                        messageType.Add(new JsonBooleanValue("request", false));
                        messageType.Add(new JsonStringValue("details", "Authentication Successful"));

                        //populate User fields
                        user.Add(new JsonNumericValue("userNo", (int)UserProf.profile.userNo));
                        user.Add(new JsonStringValue("userType", (string)UserProf.profile.userType));
                        user.Add(new JsonStringValue("transactionHistory", (string)UserProf.profile.transactionHistory));
                        user.Add(new JsonBooleanValue("receiveCommunication", Convert.ToBoolean(UserProf.profile.receiveCommunication)));

                        JsonObjectCollection account = new JsonObjectCollection("account");
                        account.Add(new JsonStringValue("bankCode", (string)UserProf.profile.bankCode));
                        account.Add(new JsonStringValue("accountNum", (string)UserProf.profile.accountNum));
                        account.Add(new JsonStringValue("accountPWD", (string)UserProf.profile.accountPWD));
                        account.Add(new JsonNumericValue("acctBalance", (int)UserProf.profile.acctBalance));
                        user.Add(account);

                        JsonObjectCollection hardwareInfo = new JsonObjectCollection("hardwareInfo");
                        hardwareInfo.Add(new JsonNumericValue("POSHWID", (int)UserProf.profile.POSHWID));
                        hardwareInfo.Add(new JsonStringValue("currentDK", (string)UserProf.profile.currentDK));
                        hardwareInfo.Add(new JsonStringValue("nextDK", (string)UserProf.profile.nextDK));
                        user.Add(hardwareInfo);

                        JsonObjectCollection userID = new JsonObjectCollection("userID");
                        userID.Add(new JsonStringValue("username", (string)UserProf.profile.username));
                        userID.Add(new JsonStringValue("password", (string)UserProf.profile.password));
                        user.Add(userID);

                        JsonObjectCollection personalInfo = new JsonObjectCollection("personalInfo");
                        personalInfo.Add(new JsonStringValue("firstName", (string)UserProf.profile.firstName));
                        personalInfo.Add(new JsonStringValue("lastName", (string)UserProf.profile.lastName));
                        personalInfo.Add(new JsonStringValue("middleName", (string)UserProf.profile.middleName));
                        personalInfo.Add(new JsonStringValue("email", (string)UserProf.profile.email));
                        personalInfo.Add(new JsonStringValue("occupation", (string)UserProf.profile.occupation));
                        personalInfo.Add(new JsonNumericValue("SIN", (int)UserProf.profile.SIN));
                        personalInfo.Add(new JsonStringValue("address1", (string)UserProf.profile.address1));
                        personalInfo.Add(new JsonStringValue("address2", (string)UserProf.profile.address2));
                        personalInfo.Add(new JsonStringValue("email", (string)UserProf.profile.city));
                        personalInfo.Add(new JsonStringValue("province", (string)UserProf.profile.province));
                        personalInfo.Add(new JsonStringValue("country", (string)UserProf.profile.country));
                        personalInfo.Add(new JsonStringValue("postalCode", (string)UserProf.profile.postalCode));
                        personalInfo.Add(new JsonNumericValue("phoneNumber", (int)UserProf.profile.phoneNumber));

                        JsonObjectCollection dateOfBirth = new JsonObjectCollection("dateOfBirth");
                        dateOfBirth.Add(new JsonNumericValue("DOBDay", (int)UserProf.profile.DOBDay));
                        dateOfBirth.Add(new JsonNumericValue("DOBMonthr", (int)UserProf.profile.DOBMonth));
                        dateOfBirth.Add(new JsonNumericValue("DOBYear", (int)UserProf.profile.DOBYear));
                        personalInfo.Add(dateOfBirth);

                        user.Add(personalInfo);

                        //build response message content from already defined JSON Objects               
                        defineResponse.Insert(0, headers);
                        defineResponse.Add(messageType);
                        defineResponse.Add(user);
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
                        JObject persInfo = (JObject)newUser.SelectToken("personalInfo");
                        JObject DOB = (JObject)persInfo.SelectToken("dateOfBirth");
                        JObject HWInfo = (JObject)newUser.SelectToken("hardwareInfo");

                        //Populate the newProfile object with the information received from the client
                        newProfile.userType = (string)newUser.SelectToken("userType");
                        newProfile.receiveCommunication = Convert.ToInt16((bool)newUser.SelectToken("receiveCommunication"));                     
                        newProfile.bankCode = (string)acct.SelectToken("bankCode");
                        newProfile.accountNum = (string)acct.SelectToken("accountNum");
                        newProfile.accountPWD = (string)acct.SelectToken("accountPWD");
                        newProfile.username = (string)UID.SelectToken("username");
                        newProfile.password = (string)UID.SelectToken("password");
                        newProfile.firstName = (string)persInfo.SelectToken("firstName");
                        newProfile.middleName = (string)persInfo.SelectToken("middleName");
                        newProfile.lastName = (string)persInfo.SelectToken("lastName");
                        newProfile.DOBDay = (int)DOB.SelectToken("DOBDay");
                        newProfile.DOBMonth = (int)DOB.SelectToken("DOBMonth");
                        newProfile.DOBYear = (int)DOB.SelectToken("DOBYear");
                        newProfile.occupation = (string)persInfo.SelectToken("occupation");
                        newProfile.SIN = (int)persInfo.SelectToken("SIN");
                        newProfile.address1 = (string)persInfo.SelectToken("address1");
                        newProfile.address2 = (string)persInfo.SelectToken("address2");
                        newProfile.city = (string)persInfo.SelectToken("city");
                        newProfile.province = (string)persInfo.SelectToken("province");
                        newProfile.country = (string)persInfo.SelectToken("country");
                        newProfile.postalCode = (string)persInfo.SelectToken("postalCode");
                        newProfile.email = (string)persInfo.SelectToken("email");
                        newProfile.phoneNumber = (int)persInfo.SelectToken("phoneNumber");
                        newProfile.POSHWID = (int)HWInfo.SelectToken("POSHWID");
                        newProfile.authenticationString = "";
                        newProfile.authenticationString += newProfile.username;
                        newProfile.authenticationString += newProfile.password;
                       
                        //pass the populated newProfile information to ServerWorker to try and create a new profile
                        //and build response message to client based on the return code receiveed from ServerWorker
                        ResultCodeType rtype = paymentServer_requestWorker.createNewProfile(DBHandler, newProfile);

                        if (rtype == ResultCodeType.SUCC_CREATE_PROFILE)
                        {
                            messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SIGN_UP_SUCCESS));
                            messageType.Add(new JsonBooleanValue("response", true));
                            messageType.Add(new JsonBooleanValue("request", false));
                            messageType.Add(new JsonStringValue("details", "User account created"));
                            paymentServer_email.emailSignupHandler(newProfile, "AccountCreated");
                        }
                        else if(rtype == ResultCodeType.ERROR_CREATE_PROFILE_EMAIL_EXISTS)
                        {
                            messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SIGN_UP_FAILURE));
                            messageType.Add(new JsonBooleanValue("response", true));
                            messageType.Add(new JsonBooleanValue("request", false));
                            messageType.Add(new JsonStringValue("details", "Could not create profile. The email provided is already registered"));
                        }
                        else if (rtype == ResultCodeType.ERROR_CREATE_PROFILE_USERNAME_EXISTS)
                        {
                            messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SIGN_UP_FAILURE));
                            messageType.Add(new JsonBooleanValue("response", true));
                            messageType.Add(new JsonBooleanValue("request", false));
                            messageType.Add(new JsonStringValue("details", "Could not create profile. The username provided is already registered"));
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

                    case ((int)clientIncomingCodeEnum.IN_CODE_PROCESS_PAYMENT_REQ):
                        // --------------------Message comming in-------------------------------------
                        // --------------------Extract information-------------------------------
                        // customer ID and password, prepare for curstomer authentication
                        /*JObject customerJsonObj = (JObject)received.SelectToken("customer");
                        string tcustUsername = (string)customerJsonObj.SelectToken("custUsername");
                        string tcustPWD = (string)customerJsonObj.SelectToken("custPWD");
                        string tcustAuthString = "" + tcustUsername + tcustPWD;

                        // merchant ID and password, prepare for merchant authentication
                        JObject merchantJsonObj = (JObject)received.SelectToken("merchantIdent");
                        string tmerchantUsername = (string)merchantJsonObj.SelectToken("merchantUsername");
                        string tmerchantPWD = (string)customerJsonObj.SelectToken("merchantPWD");
                        string tmerchantAuthString = "" + tmerchantUsername + tmerchantPWD;

                        // obtain transaction object and extract information
                        JObject transactionJsonObj = (JObject)received.SelectToken("transactions");
                        int ttransactionID = (int)transactionJsonObj.SelectToken("transactionID");
                        // double tdebitAmount = (double)transactionJsonObj.SelectToken("debitAmount");
                        // double tcreditAmount = (double)transactionJsonObj.SelectToken("creditAmount");
                        string tamount = (string)transactionJsonObj.SelectToken("amount");
                        // int tbalance = (int)transactionJsonObj.SelectToken("balance");
                        // int treceiptNo = (int)transactionJsonObj.SelectToken("receiptNo");
                        Boolean isRefundT = (Boolean)transactionJsonObj.SelectToken("isRefund");

                        Console.WriteLine("------------------Received message: ---------------------\n" +
                            received.ToString() + "------------------Received message: ---------------------\n");

                        // JObject transactionRequester = (JObject)received.SelectToken("user");
                        // int transactionUserNo = (int)transactionRequester.SelectToken("userNo");
                        
                        // obtain transaction date and time

                        // DateTime transactionTimeOnMobile = new DateTime(tyear, tmonth, tday, thour, tminute, tsecond);

                        DateTime currentTime = DateTime.Now;

                        // -----------prepare transaction record object-----------------------------
                        // add this transactio recoed into database
                        transactionRecord trecode = new transactionRecord();
                        // trecode.userNo = transactionUserNo;
                        trecode.time = currentTime;
                        trecode.customerUsername = tcustUsername;
                        trecode.merchantUsername = tmerchantUsername;
                        trecode.amount = tamount;
                        trecode.isRefund = isRefundT;
                        trecode.status = FromBankServerMessageTypes.ERROR_AUTHENDICATION_CUSTOMER;
                        trecode.transactionMessage = "";
                        trecode.receiptNumber = "";

                        transactions.Clear();

                        // store time into the transaction JSON object
                        transactionTime = insert(transactionTime, hour, new JsonNumericValue("hour", currentTime.Hour));
                        transactionTime = insert(transactionTime, minute, new JsonNumericValue("minute", currentTime.Minute));
                        transactionTime = insert(transactionTime, second, new JsonNumericValue("second", currentTime.Second));
                        transactions.Add(transactionTime);

                        transactionDate = insert(transactionDate, year, new JsonNumericValue("year", currentTime.Year));
                        transactionDate = insert(transactionDate, month, new JsonNumericValue("month", currentTime.Month));
                        transactionDate = insert(transactionDate, day, new JsonNumericValue("day", currentTime.Day));
                        transactions.Add(transactionTime);

                        messageType.Clear();

                        

                        // ---------get user profile----------------------------------
                        // pull user's account details
                        GetProfileResultType UserProf_transaction = paymentServer_requestWorker.MYgetUserProfileByUsername(DBHandler, tcustUsername);

                        if (UserProf_transaction.status != ResultCodeType.UPDATE_USER_PROFILE_SUCCESS)
                        {
                            // cannot get customer's profile from database
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SEND_USER_PROFILE_FAILURE));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Server error - Could not get profile data"));

                            trecode.transactionMessage = "error when look up customer info from database";
                            trecode.status = FromBankServerMessageTypes.ERROR_BEFORE_CONTACT_BANK;
                            // int dbr = paymentServer_requestWorker.addNewTransactionRecord(DBHandler, trecode);

                            defineResponse.Insert(0, headers);
                            defineResponse.Add(messageType);
                            Console.WriteLine("pull user's account details f");
                            break;
                        }

                        Console.WriteLine("pull user's account details s");

                        // prepare for the bank info for bank server
                        string accountNum_transaction = UserProf_transaction.profile.accountNum;
                        string accountPWD_transaction = UserProf_transaction.profile.accountPWD;
                        string bankCode_transaction = UserProf_transaction.profile.bankCode;

                        // ------------get merchant profile---------------------------------------
                        // pull merchant's account details
                        GetProfileResultType merchantProf_transaction = paymentServer_requestWorker.MYgetUserProfileByUsername(DBHandler, tmerchantUsername);

                        if (merchantProf_transaction.status != ResultCodeType.UPDATE_USER_PROFILE_SUCCESS)
                        {
                            // cannot get merchant's profile from database
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_SEND_USER_PROFILE_FAILURE));
                            messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                            messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                            messageType = insert(messageType, details, new JsonStringValue("details", "Server error - Could not get profile data"));

                            trecode.transactionMessage = "error when look up merchant info from database";
                            trecode.status = FromBankServerMessageTypes.ERROR_BEFORE_CONTACT_BANK;
                            // int dbr = paymentServer_requestWorker.addNewTransactionRecord(DBHandler, trecode);

                            defineResponse.Insert(0, headers);
                            defineResponse.Add(messageType);

                            Console.WriteLine("pull merchant's account details f");
                            break;
                        }

                        Console.WriteLine("pull merchant's account details s");

                        // prepare for the bank info for bank server
                        string accountNumMerchant_transaction = merchantProf_transaction.profile.accountNum;
                        string accountPWDMerchant_transaction = merchantProf_transaction.profile.accountPWD;
                        string bankCodeMerchant_transaction = merchantProf_transaction.profile.bankCode;

                        // ------------connect to bank server---------------------------------
                        // contact bank
                        TransactionResult tresult;
                        if(! isRefundT)
                            tresult = paymentServer_connectBank.sendBankTransaction(
                                accountNum_transaction, accountPWD_transaction, bankCode_transaction,
                                accountNumMerchant_transaction, bankCodeMerchant_transaction, tamount);
                        else
                            tresult = paymentServer_connectBank.sendBankTransaction(
                                accountNumMerchant_transaction, accountPWDMerchant_transaction, bankCodeMerchant_transaction,
                                accountNum_transaction, bankCode_transaction, tamount);

                        Console.WriteLine("------------connect to bank ----------");
                        Console.WriteLine(tresult.MyToString());

                        // -----------add this transaction record to database-------------------------
                        trecode.status = tresult.status;
                        trecode.transactionMessage = tresult.bankReplyMessage;
                        trecode.receiptNumber = tresult.receiptNumber;
                        // int transactionIDT = paymentServer_requestWorker.addNewTransactionRecord(DBHandler, trecode);
                        paymentServer_requestWorker.addNewTransactionRecord_toCustomerField(DBHandler, trecode);

                        Console.WriteLine("addNewTransactionRecord_toCustomerField");

                        // ---------- update the remaining balance in the user profile database
                        if (trecode.isRefund) // is refund, the customer is the payee
                        {
                            long thisbalance = tryToConvertStringToLong(tresult.payeeBalance);
                            paymentServer_requestWorker.updateBalance(DBHandler, tcustUsername, thisbalance);

                            thisbalance = tryToConvertStringToLong(tresult.payerBalance);
                            paymentServer_requestWorker.updateBalance(DBHandler, tmerchantUsername, thisbalance);
                        }
                        else // is refund, the customer is the payer
                        {
                            long thisbalance = tryToConvertStringToLong(tresult.payerBalance);
                            paymentServer_requestWorker.updateBalance(DBHandler, tcustUsername, thisbalance);

                            thisbalance = tryToConvertStringToLong(tresult.payeeBalance);
                            paymentServer_requestWorker.updateBalance(DBHandler, tmerchantUsername, thisbalance);
                        }
                    
                        // -------------analyse bank server response------------------------------
                        if(tresult.status == FromBankServerMessageTypes.FROM_BANK_TRANSACTION_ACK)
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_PAYMENT_SUCCESSFUL));
                        else
                            messageType = insert(messageType, code, new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_PAYMENT_FAILURE));

                        string remainBalance = "";
                        if (trecode.isRefund) // is refund, the customer is the payee
                        {
                            remainBalance = tresult.payeeBalance;
                        }
                        else // is refund, the customer is the payer
                        {
                            remainBalance = tresult.payerBalance;
                        }

                        messageType = insert(messageType, response, new JsonBooleanValue("response", true));
                        messageType = insert(messageType, request, new JsonBooleanValue("request", false));
                        messageType = insert(messageType, details, new JsonStringValue("details", tresult.bankReplyMessage));

                        // transactions = insert(transactions, transactionID, new JsonNumericValue("transactionID", transactionIDT));
                        transactions = insert(transactions, amount, new JsonNumericValue("amount", Convert.ToInt64(tamount)));
                        transactions = insert(transactions, isRefund, new JsonBooleanValue("isRefund", isRefundT));
                        transactions = insert(transactions, balance, new JsonNumericValue("balance", Convert.ToInt64(remainBalance)));
                        transactions = insert(transactions, receiptNo, new JsonStringValue("receiptNo", trecode.receiptNumber));
                        transactions = insert(transactions, merchantID, new JsonNumericValue("merchantID", (int)1));
                        transactions = insert(transactions, bankReplyMessage, new JsonStringValue("bankReplyMessage", trecode.transactionMessage));
                        */
                        //build response message content from already defined JSON Objects                           

                        /// provide message type and transactions
                        defineResponse = paymentServer_paymentRequestHandler.processPayment(DBHandler, received);

                        defineResponse.Insert(0, headers);

                        break;
                }
            }   

            //finalize outgoing JSON message
            JsonObjectCollection completeResponse = new JsonObjectCollection();
            JsonObjectCollection packagedResponse = new JsonObjectCollection();
            completeResponse.Add(defineResponse);

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

        /*public static JsonObjectCollection insert(JsonObjectCollection obj, JsonObject item, JsonObject newItem){
            // obj.Remove(item);
            obj.Add( newItem);
            return obj;
        }*/
    }
}

