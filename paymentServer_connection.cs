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
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using PaymentServer;

namespace PaymentServer
{

    public class HttpProcessor {
        public TcpClient socket;        
        public HttpServer srv;
        public X509Certificate serverCert;

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
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;
                Console.WriteLine("Read timeout: " + sslStream.ReadTimeout);
                Console.WriteLine("Write timeout: " + sslStream.WriteTimeout);

                // Write status message to the client. 
                byte[] message = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n");
                Console.WriteLine("Sending status message.");
                sslStream.Write(message);

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

        public void writeFailure() {
            string text = "HTTP/1.1 404 File not found\n"+"Connection: close\n";
            byte[] message = Encoding.UTF8.GetBytes(text);
            sslStream.Write(message);
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


        public MyHttpServer(int port, string cert)
            : base(port, cert) {
        }
     

        public override void handleGETRequest (HttpProcessor p)
		{
            paymentServer_requestHandler.handleRequest(p, null, "GET");    
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData) {
            paymentServer_requestHandler.handleRequest(p, inputData, "POST");
        }
    }

    public class TestMain {
        public static int Main3(String[] args)
        {
            paymentServer_dataBase DBHandler = new paymentServer_dataBase();

            UserProfile p = new UserProfile();
            // p.userNo = 555;
            p.username = "user8";
            p.email = "a2@g.com";
            p.password = "passw3";
            p.userType = "cu";
            p.firstName = "chang2";
            p.middleName = "m2";
            p.lastName = "zhou2";
            p.DOBDay = 1999;
            p.DOBMonth = 12;
            p.DOBDay = 15;
            p.transactionHistory = "0";
            p.occupation = "1";
            p.address1 = "1";
            p.address2 = "2";
            p.city = "c";
            p.province = "p";
            p.country = "c";
            p.postalCode = "pc";
            p.bankCode = "0123";
            p.accountNum = "123456";
            p.accountPWD = "qwerty";
            p.currentDK = "cdk";
            p.nextDK = "ndk";
            p.authenticationString = "user6passw3";


            // paymentServer_requestWorker.createNewProfile(DBHandler, p);

            // paymentServer_requestWorker.createNewProfile(DBHandler, p);
            /*GetProfileResultType g = paymentServer_requestWorker.getUserProfile(DBHandler, 555);
            Console.WriteLine("1: "+g.profile.username);

            DBHandler.Update("userProfile", "transactionHistory = 'new Line'", "userNo = 555");

            GetProfileResultType g2 = paymentServer_requestWorker.getUserProfile(DBHandler, 555);
            Console.WriteLine("2: " + g2.profile.transactionHistory);

            // DBHandler.Insert("userProfile", "(userNo)", "('0')");
            // List<string>[] list = DBHandler.Select("userProfile", "userNo", "" + 1);

            // Console.WriteLine(list.Length);
            // Console.WriteLine(list[0]);*/

            int now = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

            /*GetProfileResultType result = paymentServer_requestWorker.MYgetUserProfileByUsername(DBHandler, "user8");
            Console.WriteLine("result.status: " + result.status);
            Console.WriteLine("result.accountNum: " + result.profile.accountNum);
            Console.WriteLine("result.accountPWD: " + result.profile.accountPWD);
            Console.WriteLine("result.authenticationString: " + result.profile.authenticationString);*/

            TransactionResult result = paymentServer_connectBank.sendBankTransaction("0102", "123", "010",
                "0106", "010", "2000000");
            Console.WriteLine("1: " + result.bankReplyMessage);
            Console.WriteLine("2: " + result.receiptNumber);
            Console.WriteLine("3: " + result.status);
            Console.WriteLine("4: " + result.payeeBalance);
            Console.WriteLine("5: " + result.payerBalance);

            int now2 = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - now;
            Console.WriteLine(now2);

            /*transactionRecord tr = new transactionRecord();
            paymentServer_requestWorker.addNewTransactionRecord(DBHandler, tr);
            return 0;*/

            /*TransactionResult tresult;
            tresult = paymentServer_connectBank.sendBankTransaction(
                "str1", "str2", "str3",
                "str4", "str5", "" + 12334);
            Console.WriteLine("status: " + tresult.status);
            Console.WriteLine("receiptNumber: " + tresult.receiptNumber);*/

            Console.ReadLine();
            return 0;
        }


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



