using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Json;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;



// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace Bend.Util {

    public class HttpProcessor {
        public TcpClient socket;        
        public HttpServer srv;
        public X509Certificate serverCert;

        private Stream inputStream;
        public StreamWriter outputStream;
        public SslStream sslStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();


        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv, X509Certificate serverCertificate) {
            this.socket = s;
            this.srv = srv;
            this.serverCert = serverCertificate;       
        }
        

        private string streamReadLine(SslStream inputStream) {
            int next_char;
            string data = "";
            while (true) {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }            
            return data;
        }
        public void process() {

            // A client has connected. Create the  
            // sslStream using the client's network stream.
            sslStream = new SslStream(socket.GetStream(), false);
            // Authenticate the server but don't require the client to authenticate. 
            try
            {
                sslStream.AuthenticateAsServer(serverCert,
                    false, SslProtocols.Tls, true);
                // Display the properties and settings for the authenticated stream.
                DisplaySecurityLevel(sslStream);
                DisplaySecurityServices(sslStream);
                DisplayCertificateInformation(sslStream);
                DisplayStreamProperties(sslStream);

                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 500000;
                sslStream.WriteTimeout = 500000;
                Console.WriteLine("Read timeout: " + sslStream.ReadTimeout);
                Console.WriteLine("Write timeout: " + sslStream.WriteTimeout);

                // Write a message to the client. 
                byte[] message = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n");
                Console.WriteLine("Sending status message.");
                sslStream.Write(message);

                // Read a message from the client.   
                //Console.WriteLine("Waiting for client message...");
                //string messageData = ReadMessage(sslStream);
                //Console.WriteLine("Received: {0}", messageData);
                  
                /*// we can't use a StreamReader for input, because it buffers up extra data on us inside it's
                // "processed" view of the world, and we want the data raw after the headers
                inputStream = new BufferedStream(socket.GetStream());

                // we probably shouldn't be using a streamwriter for all output from handlers either
                outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));*/
                try
                {

                    parseRequest();
                    readHeaders();

                    if (http_method.Equals("GET"))
                    {
                        Console.WriteLine("http_method: " + http_method);
                        handleGETRequest();
                    }
                    else if (http_method.Equals("POST"))
                    {
                        Console.WriteLine("http_method: " + http_method);
                        handlePOSTRequest();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.ToString());
                    writeFailure();
                    return;
                }
                //sslStream.Flush();
                // bs.Flush(); // flush any remaining output
                //inputStream = null; outputStream = null; // bs = null;      
               // sslStream = null;
              //  socket.Close();
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                sslStream.Close();
                socket.Close();
                return;
            }
            finally
            {
                // The client stream will be closed with the sslStream 
                // because we specified this behavior when creating 
                // the sslStream.
                sslStream.Close();
                socket.Close();
            }  
        }

        public void parseRequest() {
            String request = streamReadLine(sslStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3) {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];

            Console.WriteLine("starting: " + request);
        }

        public void readHeaders() {
            Console.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(sslStream)) != null) {
                if (line.Equals("")) {
                    Console.WriteLine("got headers");
                    return;
                }
                
                int separator = line.IndexOf(':');
                if (separator == -1) {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' ')) {
                    pos++; // strip any spaces
                }
                    
                string value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}",name,value);
                httpHeaders[name] = value;
            }
        }

        public void handleGETRequest() {
            srv.handleGETRequest(this);
        }

        private const int BUF_SIZE = 4096;
        public void handlePOSTRequest() {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length")) {
                 content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                 if (content_len > MAX_POST_SIZE) {
                     throw new Exception(
                         String.Format("POST Content-Length({0}) too big for this simple server",
                           content_len));
                 }
                 byte[] buf = new byte[BUF_SIZE];              
                 int to_read = content_len;
                 while (to_read > 0) {  
                     Console.WriteLine("starting Read, to_read={0}",to_read);

                     int numread = this.sslStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                     Console.WriteLine("read finished, numread={0}", numread);
                     if (numread == 0) {
                         if (to_read == 0) {
                             break;
                         } else {
                             throw new Exception("client disconnected during post");
                         }
                     }
                     to_read -= numread;
                     ms.Write(buf, 0, numread);
                 }
                 ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }

        public void writeSuccess(string content_type="application/json") {
            string text = "HTTP/1.1 200 OK\n"+"Content-Type: " + content_type+"\n"+" ";
            byte[] message = Encoding.UTF8.GetBytes(text);
            sslStream.Write(message);
        }

        public void writeFailure() {
            string text = "HTTP/1.1 404 File not found\n"+"Connection: close\n";
            byte[] message = Encoding.UTF8.GetBytes(text);
            sslStream.Write(message);
        }

        public string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the client. 
            // The client signals the end of the message using the 
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                // Read the client's test message.
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8 
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF or an empty message. 
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }

    }

    public abstract class HttpServer {

        protected int port;
        TcpListener listener;
        bool is_active = true;
        public Hashtable respStatus;
        string certificate = null;
        static X509Certificate serverCertificate = null;
       
        public HttpServer(int port, string cert) {
            this.port = port;
            this.certificate = cert;
        }

        // The certificate parameter specifies the name of the file  
        // containing the machine certificate. 
        public void listen() {
            serverCertificate = X509Certificate.CreateFromCertFile(certificate);

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Listening On: " + port.ToString());
            while (is_active) {
                Console.WriteLine("Waiting for connection...\n");
                TcpClient s = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this, serverCertificate);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }

    public class MyHttpServer : HttpServer {
        JsonObjectCollection headers;
        JsonObjectCollection messageType;
        JsonObjectCollection user;
        JsonObjectCollection merchant;
        JsonObjectCollection customer;
        JsonObjectCollection transactions;

        public MyHttpServer(int port, string cert)
            : base(port, cert) {

                //Define outgoing JSON message structures 
                headers = new JsonObjectCollection();
                headers.Name = "headers";
                headers.Add(new JsonStringValue("Accept-Encoding", "gzip,deflate,sdch"));
                headers.Add(new JsonStringValue("Cookie", "_gauges_unique_month=1; _gauges_unique_year=1; _gauges_unique=1"));
                headers.Add(new JsonStringValue("Accept-Language", "en-CA,en-GB,en-US;q=0.8,en;q=0.6"));
                headers.Add(new JsonStringValue("Accept", "application/json, text/json"));
                headers.Add(new JsonStringValue("Host", "paymentserver.dynu.com"));
                headers.Add(new JsonStringValue("Connection", "close"));
                headers.Add(new JsonStringValue("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"));

                messageType = new JsonObjectCollection();
                messageType.Name = "messageType";
                messageType.Add(new JsonBooleanValue("request", false));
                messageType.Add(new JsonBooleanValue("response", false));
                messageType.Add(new JsonStringValue("details", ""));

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
        }
     

        public override void handleGETRequest (HttpProcessor p)
		{
            Console.WriteLine("request: {0}", p.http_url);
            
            //build response content from already defined JSON Objects
            JsonObjectCollection defineResponse = new JsonObjectCollection();
            defineResponse.Insert(0, headers);
            defineResponse.Add(messageType);
            defineResponse.Add(user);
            defineResponse.Add(merchant);
            defineResponse.Add(customer);
            defineResponse.Add(transactions);

            //finalize ougoing JSON message
            JsonObjectCollection completeResponse = new JsonObjectCollection();
            completeResponse.Add(defineResponse);

            //Write message to client
            byte[] message = JsonStringToByteArray(completeResponse.ToString()); 
            p.sslStream.Write(message);
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData) {
            Console.WriteLine("POST request: {0}", p.http_url);
            string data = inputData.ReadToEnd();

            //build response content from already defined JSON Objects
            JsonObjectCollection defineResponse = new JsonObjectCollection();
            defineResponse.Insert(0, headers);
            defineResponse.Add(messageType);
            defineResponse.Add(user);
            defineResponse.Add(merchant);
            defineResponse.Add(customer);
            defineResponse.Add(transactions);

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
    }

    public class TestMain {
        public static int Main(String[] args) {
            HttpServer httpServer;
            string certificate = null;
            if (args.GetLength(0) > 0) {
                certificate = args[1];
                httpServer = new MyHttpServer(Convert.ToInt16(args[0]), certificate);        
            } else {
                certificate = "TempCert.cer";
                httpServer = new MyHttpServer(443, certificate);           
            }
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            return 0;
        }
    }
}



