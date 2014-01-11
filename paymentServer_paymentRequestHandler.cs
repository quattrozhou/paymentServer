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
    class paymentServer_paymentRequestHandler
    {
        public static void println(string input)
        {
            Console.WriteLine(input);
        }

        public static JsonObjectCollection processPayment(paymentServer_dataBase DBHandler, JObject received)
        {
            println("------------------Received message start: ---------------------\n");
            println(received.ToString());
            println("------------------Received message end: ---------------------\n");

            transactionRecord tr = getReceivedData(received);

            tr.status = authentication(DBHandler, received);

            // possible problem: authentication fail
            if ((int)tr.status > 50) return prepareReturnMessage(tr);

            tr = connectToBank(DBHandler, tr);

            // possible problem: get user profile fail
            if ((int)tr.status > 50) return prepareReturnMessage(tr);

            paymentServer_requestWorker.addNewTransactionRecord_toCustomerField(DBHandler, tr);
            updateBalance(DBHandler, tr);

            return prepareReturnMessage(tr);
        }

        public static JsonObjectCollection prepareReturnMessage(transactionRecord tr)
        {
            JsonObjectCollection defineResponse = new JsonObjectCollection();

            if ((int)tr.status > 50)
            {
                /// possible error type: authentication, get user profile
                JsonObjectCollection messageType = new JsonObjectCollection("messageType");
                messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_PAYMENT_FAILURE));
                messageType.Add(new JsonBooleanValue("request", false));
                messageType.Add(new JsonBooleanValue("response", false));
                messageType.Add(new JsonStringValue("details", "" + tr.status.ToString()));
                defineResponse.Add(messageType);
            }
            else 
            {
                JsonObjectCollection messageType = new JsonObjectCollection("messageType");
                if (tr.status == FromBankServerMessageTypes.FROM_BANK_TRANSACTION_ACK)
                {
                    messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_PAYMENT_SUCCESSFUL));
                    messageType.Add(new JsonBooleanValue("request", false));
                    messageType.Add(new JsonBooleanValue("response", false));
                    messageType.Add(new JsonStringValue("details", "" + tr.status.ToString() + "message: " + tr.transactionMessage));
                    defineResponse.Add(messageType);
                }
                else
                {
                    messageType.Add(new JsonNumericValue("code", (int)clientOutgoingCodeEnum.OUT_CODE_PAYMENT_FAILURE));
                    messageType.Add(new JsonBooleanValue("request", false));
                    messageType.Add(new JsonBooleanValue("response", false));
                    messageType.Add(new JsonStringValue("details", "" + tr.status.ToString() + "message: " + tr.transactionMessage));
                    defineResponse.Add(messageType);
                }
                
                JsonObjectCollection transaction = new JsonObjectCollection("transactions");

                JsonObjectCollection transactionTime = new JsonObjectCollection("transactionTime");
                transactionTime.Add(new JsonNumericValue("hour", tr.time.Hour));
                transactionTime.Add(new JsonNumericValue("minute", tr.time.Minute));
                transactionTime.Add(new JsonNumericValue("second", tr.time.Second));
                transaction.Add(transactionTime);

                JsonObjectCollection transactionDate = new JsonObjectCollection("transactionDate");
                transactionTime.Add(new JsonNumericValue("year", tr.time.Year));
                transactionTime.Add(new JsonNumericValue("month", tr.time.Month));
                transactionTime.Add(new JsonNumericValue("day", tr.time.Day));
                transaction.Add(transactionDate);

                transaction.Add(new JsonNumericValue("amount", tryToConvertStringToLong(tr.amount)));
                transaction.Add(new JsonBooleanValue("isRefund", tr.isRefund));
                transaction.Add(new JsonNumericValue("balance", tryToConvertStringToLong(tr.customerBalance)));
                transaction.Add(new JsonStringValue("receiptNo", tr.receiptNumber));
                transaction.Add(new JsonStringValue("bankReplyMessage", tr.transactionMessage));
                defineResponse.Add(transaction);
            }

            println("++++++++++++++++++Return message start: +++++++++++++++++++\n");
            println(defineResponse.ToString());
            println("++++++++++++++++++Return message end: +++++++++++++++++++++\n");

            return defineResponse;
        }

        public static void updateBalance(paymentServer_dataBase DBHandler, transactionRecord tr)
        {
            // ---------- update the remaining balance in the user profile database

            long thisbalance = tryToConvertStringToLong(tr.customerBalance);
            paymentServer_requestWorker.updateBalance(DBHandler, tr.customerUsername, thisbalance);

            thisbalance = tryToConvertStringToLong(tr.merchantBalance);
            paymentServer_requestWorker.updateBalance(DBHandler, tr.merchantUsername, thisbalance);
        }

        public static transactionRecord connectToBank(paymentServer_dataBase DBHandler, transactionRecord tr)
        {
            GetProfileResultType customerProfile = paymentServer_requestWorker.MYgetUserProfileByUsername(DBHandler, tr.customerUsername);
            GetProfileResultType merchantProfile = paymentServer_requestWorker.MYgetUserProfileByUsername(DBHandler, tr.merchantUsername);

            if ((int)customerProfile.status > 50)
            {
                tr.status = FromBankServerMessageTypes.ERROR_GET_CUSTOMER_PROFILE;
                return tr;
            }
            if ((int)merchantProfile.status > 50)
            {
                tr.status = FromBankServerMessageTypes.ERROR_GET_MERCHANT_PROFILE;
                return tr;
            }

            string cusAccNum = customerProfile.profile.accountNum;
            string cusAccPwd = customerProfile.profile.accountPWD;
            string cusBakCod = customerProfile.profile.bankCode;

            string mctAccNum = merchantProfile.profile.accountNum;
            string mctAccPwd = merchantProfile.profile.accountPWD;
            string mctBakCod = merchantProfile.profile.bankCode;

            TransactionResult tresult;

            if (!tr.isRefund)
            {
                tresult = paymentServer_connectBank.sendBankTransaction(
                    cusAccNum, cusAccPwd, cusBakCod, mctAccNum, mctBakCod, tr.amount);

                tr.customerBalance = tresult.payerBalance;
                tr.merchantBalance = tresult.payeeBalance;
            }
            else
            {
                tresult = paymentServer_connectBank.sendBankTransaction(
                    mctAccNum, mctAccPwd, mctBakCod, cusAccNum, cusBakCod, tr.amount);

                tr.customerBalance = tresult.payeeBalance;
                tr.merchantBalance = tresult.payerBalance;
            }

            tr.status = tresult.status;
            tr.transactionMessage = tresult.bankReplyMessage;
            tr.receiptNumber = tresult.receiptNumber;

            return tr;
        }

        public static transactionRecord getReceivedData(JObject received)
        {
            transactionRecord tr = new transactionRecord();

            JObject customerJObject = (JObject)received.SelectToken("customer");
            string custUsername = (string)customerJObject.SelectToken("custUsername");
            JObject merchantJObject = (JObject)received.SelectToken("merchantIdent");
            string merchantUsername = (string)merchantJObject.SelectToken("merchantUsername");
            JObject transactionJsonObj = (JObject)received.SelectToken("transactions");

            tr.customerUsername = custUsername;
            tr.merchantUsername = merchantUsername;
            tr.time = DateTime.Now;
            tr.amount = (string)transactionJsonObj.SelectToken("amount");
            tr.isRefund = (Boolean)transactionJsonObj.SelectToken("isRefund");

            return tr;
        }

        public static FromBankServerMessageTypes authentication(paymentServer_dataBase DBHandler, JObject received)
        {
            JObject transactionJsonObj = (JObject)received.SelectToken("transactions");
            Boolean isRefundT = (Boolean)transactionJsonObj.SelectToken("isRefund");

            // -------------authentication costomer-----------------------------
            // authentication costomer
            // customer ID and password, prepare for curstomer authentication
            JObject customerJObject = (JObject)received.SelectToken("customer");
            string custUsername = (string)customerJObject.SelectToken("custUsername");
            string custPWD = (string)customerJObject.SelectToken("custPWD");
            string custAuthString = "" + custUsername + custPWD;

            if ((!paymentServer_requestWorker.authenticateUser(DBHandler, custAuthString)) && !isRefundT)
            {
                return FromBankServerMessageTypes.ERROR_AUTHENDICATION_CUSTOMER;
            }

            // merchant ID and password, prepare for merchant authentication
            JObject merchantJObject = (JObject)received.SelectToken("merchantIdent");
            string merchantUsername = (string)merchantJObject.SelectToken("merchantUsername");
            string merchantPWD = (string)merchantJObject.SelectToken("merchantPWD");
            string merchantAuthString = "" + merchantUsername + merchantPWD;

            //// ------------authentication merchant-------------------------------------
            // authentication merchant
            if ((!paymentServer_requestWorker.authenticateUser(DBHandler, merchantAuthString)) && isRefundT)
            {
                return FromBankServerMessageTypes.ERROR_AUTHENDICATION_MERCHANT;
            }
            return FromBankServerMessageTypes.AUTHEDICATION_SUCCESS;
        }

        public static long tryToConvertStringToLong(string input)
        {
            long result = 0;
            try
            {
                result = Convert.ToInt64(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }
    }
}
