using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Json;
using Newtonsoft.Json.Linq;

namespace PaymentServer
{
    /* paymentServer_connectBank
     * Contain a banch of functions that can make cnnection to bank server
     * Act like a HTTPS client
     * 
     * payment server <- mobile application
     * 
     * */
    class paymentServer_connectBank
    {
        public static void connectBankLogin()
        {
            
        }

        /// <summary>
        /// create JSON object from input field. Call sendStringToServer() method to send string
        /// pass back the result from bank
        /// </summary>
        /// <param name="senderAccNum"></param>
        /// <param name="senderAccPwd"></param>
        /// <param name="senderBankCode"></param>
        /// <param name="recverAccNum"></param>
        /// <param name="recverBankCode"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static TransactionResult sendBankTransaction(String senderAccNum, String senderAccPwd, String senderBankCode,
            String recverAccNum, String recverBankCode, string amount)
        {
            // form a transaction request message
            JsonObjectCollection message = new JsonObjectCollection();
            JsonNumericValue messageType = new JsonNumericValue("messageType", (int)ToBankServerMessageTypes.TO_BANK_SERVER_TRANSACTION);
            JsonStringValue payerAccountNumber = new JsonStringValue("payerAccountNumber", senderAccNum);
            JsonStringValue payerAccountPassword = new JsonStringValue("payerAccountPassword", senderAccPwd);
            JsonStringValue payerBankCode = new JsonStringValue("payerBankCode", senderBankCode);
            JsonStringValue payeeAccountNumber = new JsonStringValue("payeeAccountNumber", recverAccNum);
            JsonStringValue payeeBankCode = new JsonStringValue("payeeBankCode", recverBankCode);
            JsonStringValue paymentAmount = new JsonStringValue("amount", ""+amount);
            message.Add(messageType);
            message.Add(payerAccountNumber);
            message.Add(payerAccountPassword);
            message.Add(payerBankCode);
            message.Add(payeeAccountNumber);
            message.Add(payeeBankCode);
            message.Add(paymentAmount);

            TransactionResult result = new TransactionResult();
            String bankServerResult = sendStringToServer(message.ToString());

            // nothing returned fram bank sever, means there is a time out
            if (bankServerResult.Length == 0)
            {
                result.status = FromBankServerMessageTypes.FROM_BANK_CONNECTION_FAIL;
                result.transactionMessage = "not available";
                result.receiptNumber = "not available";
                result.payerBalance = "not available";
                result.payeeBalance = "not available";
                return result;
            }

            JObject received = JObject.Parse(bankServerResult);
            result.status = (FromBankServerMessageTypes)(int)received.SelectToken("messageType");
            result.transactionMessage = (string)received.SelectToken("transactionMessage");
            result.receiptNumber = (string)received.SelectToken("receiptNumber");
            result.payerBalance = (string)received.SelectToken("payerBalance");
            result.payeeBalance = (string)received.SelectToken("payeeBalance");

            return result;
        }

        /// <summary>
        /// create a https client thread, send the string, wait for response,
        /// if not receive anything or in error, the thread will be terminated.
        /// This thread is protected.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String sendStringToServer(String input)
        {
            HTTPCLIENT httpclient = new HTTPCLIENT(input);
            Thread thread = new Thread(new ThreadStart(httpclient.connect));
            thread.Start();
            int countDownTimer = 350;
            while (thread.IsAlive && countDownTimer > 0)
            {
                Thread.Sleep(10);
                countDownTimer--;
            }

            thread.Abort();

            String output = httpclient.getContentReceive();

            if(countDownTimer == 0)
                Console.WriteLine(countDownTimer + " Time out! Client thread Terminated!\n" + output);
            // Console.ReadLine();

            return output;
        }
    }

    

    /// <summary>
    /// an object that can connect to a server with specified url in secured level
    /// use example:
    /// HTTPCLIENT httpclient = new HTTPCLIENT(sendString);
    /// Thread thread = new Thread(new ThreadStart(httpclient.connect));
    /// thread.Start();
    /// after some time
    /// thread.Abort();
    /// </summary>
    class HTTPCLIENT
    {
        String contentSent = "";
        String contentReceive = "";
        public HTTPCLIENT(String c)
        {
            this.contentSent = c;
        }
        public void connect()
        {
            // Create a request using a URL that can receive a post. 
            //WebRequest request = WebRequest.Create("http://www.contoso.com/PostAccepter.aspx");
            WebRequest request = WebRequest.Create("https://bankserver.dynu.com:443/");

            // Set the Method property of the request to POST.
            request.Method = "POST";
            request.Credentials = CredentialCache.DefaultCredentials;
            // Create POST data and convert it to a byte array.
            string postData = contentSent;//  "This is a test that posts this string to a Web server.";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            try
            {
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                Console.WriteLine("OK: write");
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.

                // Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                // Console.WriteLine(responseFromServer);
                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();
                Console.WriteLine("OK: read");
                this.contentReceive = responseFromServer;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
            // Console.ReadLine();
        }

        public String getContentReceive()
        {
            return this.contentReceive;
        }
    }
}
