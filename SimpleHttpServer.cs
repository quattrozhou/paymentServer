using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Json;



// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace Bend.Util {

    public class HttpProcessor {
        public TcpClient socket;        
        public HttpServer srv;

        private Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();


        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv) {
            this.socket = s;
            this.srv = srv;                   
        }
        

        private string streamReadLine(Stream inputStream) {
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
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
            try {
                parseRequest();
                readHeaders();
                
                if (http_method.Equals("GET")) {
                    Console.WriteLine("http_method: " + http_method);
                    handleGETRequest();
                } else if (http_method.Equals("POST")) {
                    Console.WriteLine("http_method: " + http_method);
                    handlePOSTRequest();
                }
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.ToString());
                writeFailure();
            }
            outputStream.Flush();
            // bs.Flush(); // flush any remaining output
            inputStream = null; outputStream = null; // bs = null;            
            socket.Close();             
        }

        public void parseRequest() {
            String request = streamReadLine(inputStream);
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
            while ((line = streamReadLine(inputStream)) != null) {
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

                     int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
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

        public void writeSuccess(string content_type="text/html") {
            outputStream.WriteLine("HTTP/1.1 200 OK");            
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeFailure() {
            outputStream.WriteLine("HTTP/1.1 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }

    public abstract class HttpServer {

        protected int port;
        TcpListener listener;
        bool is_active = true;
        public Hashtable respStatus;
       
        public HttpServer(int port) {
            this.port = port;
            respStatusInit();
        }

        private void respStatusInit()
        {
            respStatus = new Hashtable();

            respStatus.Add(200, "200 Ok");
            respStatus.Add(201, "201 Created");
            respStatus.Add(202, "202 Accepted");
            respStatus.Add(204, "204 No Content");

            respStatus.Add(301, "301 Moved Permanently");
            respStatus.Add(302, "302 Redirection");
            respStatus.Add(304, "304 Not Modified");

            respStatus.Add(400, "400 Bad Request");
            respStatus.Add(401, "401 Unauthorized");
            respStatus.Add(403, "403 Forbidden");
            respStatus.Add(404, "404 Not Found");

            respStatus.Add(500, "500 Internal Server Error");
            respStatus.Add(501, "501 Not Implemented");
            respStatus.Add(502, "502 Bad Gateway");
            respStatus.Add(503, "503 Service Unavailable");
        }
        //IPAddress IP = new IPAddress(0x0100007F);

        public void listen() {
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostByName(host);
            IPAddress MyAddress = new IPAddress(ip.AddressList[2].Address);
            Console.WriteLine(MyAddress.ToString());

            listener = new TcpListener(MyAddress, port);
            listener.Start();
            Console.WriteLine("Listening On: " + port.ToString());
            while (is_active) {
                Console.WriteLine("Waiting for connection...\n");
                TcpClient s = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }

    public class MyHttpServer : HttpServer {
        public MyHttpServer(int port)
            : base(port) {
        }
        public override void handleGETRequest (HttpProcessor p)
		{

			if (p.http_url.Equals ("/Test.png")) {
				Stream fs = File.Open("../../Test.png",FileMode.Open);

				p.writeSuccess("image/png");
				fs.CopyTo (p.outputStream.BaseStream);
				p.outputStream.BaseStream.Flush ();
			}

            Console.WriteLine("request: {0}", p.http_url);
            p.writeSuccess();
            JsonTextParser parser = new JsonTextParser();
            string jsonText = 
                "{"+
                " \"headers\": {" +
                " \"Accept-Encoding\": \"gzip,deflate,sdch\"," +
                " \"Cookie\": \"_gauges_unique_month=1; _gauges_unique_year=1; _gauges_unique=1\"," +
                " \"Accept-Language\": \"en-GB,en-US;q=0.8,en;q=0.6\"," +
                " \"Accept\": \"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8\"," +
                " \"Host\": \"paymentserver.dynu.com\"," +
                " \"Connection\": \"close\"," +
                " \"User-Agent\": \"Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36\"" +
                " }," +
                " \"body\": {" +
                " \"messageType\": {" +
                " \"request\": \" \"," +
                " \"response\": \" \"," +
                " \"details\": \" \"" +
                " }," +
                " \"user\": {" +
                " \"userType\": \" \"," +
                " \"userID\": {" +
                " \"username\": \" \"," +
                " \"password\": \" \"" +
                " },"  +
                " \"account\": {" +
                " \"bankCode\": \" \"," +
                " \"accountNum\": \" \"," +
                " \"accountPWD\": \" \"," +
                " \"acctBalance\": \" \"" +
                " }," +
                " \"hardwareInfo\": {" +
                " \"POSHWID\": \" \"," +
                " \"currentDK\": \" \"," +
                " \"nextDK\": \" \"" +
                " }," +
                " \"transactionHistory\": \" \"" +
                " },"+
                " \"merchant\": {" +
                " \"merchantID\": \" \"," +
                " \"merchantName\": \" \"" +
                " }," +
                " \"customer\": {" +
                " \"custUsername\": \" \"," +
                " \"custPWD\": \" \"" +
                " }," +
                " \"transactions\": {" +
                " \"transactionID\": \" \"," +
                " \"transactionDate\": {" +
                " \"year\": \" \"," +
                " \"month\": \" \","  +
                " \"day\": \" \"" +
                " }," +
                " \"transactionTime\": {" +
                " \"hour\": \" \"," +
                " \"minute\": \" \"," +
                " \"second\": \" \"" +
                " }," +
                " \"merchantID\": { \"username\": \" \" }," +
                " \"debitAmount\": \" \"," +
                " \"creditAmount\": \" \"," +
                " \"balance\": \" \"," +
                " \"receiptNo\": \" \"" +
                " }" +
                " }" +
                "}";

            p.outputStream.WriteLine(parser.Parse(jsonText));
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData) {
            Console.WriteLine("POST request: {0}", p.http_url);
            string data = inputData.ReadToEnd();

            p.writeSuccess();
            JsonTextParser parser = new JsonTextParser();
            string jsonText =
                "{" +
                " \"headers\": {" +
                " \"Accept-Encoding\": \"gzip,deflate,sdch\"," +
                " \"Cookie\": \"_gauges_unique_month=1; _gauges_unique_year=1; _gauges_unique=1\"," +
                " \"Accept-Language\": \"en-GB,en-US;q=0.8,en;q=0.6\"," +
                " \"Accept\": \"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8\"," +
                " \"Host\": \"paymentserver.dynu.com\"," +
                " \"Connection\": \"close\"," +
                " \"User-Agent\": \"Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36\"" +
                " }," +
                " \"body\": {" +
                " \"messageType\": {" +
                " \"request\": \" \"," +
                " \"response\": \" \"," +
                " \"details\": \" \"" +
                " }," +
                " \"user\": {" +
                " \"userType\": \" \"," +
                " \"userID\": {" +
                " \"username\": \" \"," +
                " \"password\": \" \"" +
                " }," +
                " \"account\": {" +
                " \"bankCode\": \" \"," +
                " \"accountNum\": \" \"," +
                " \"accountPWD\": \" \"," +
                " \"acctBalance\": \" \"" +
                " }," +
                " \"hardwareInfo\": {" +
                " \"POSHWID\": \" \"," +
                " \"currentDK\": \" \"," +
                " \"nextDK\": \" \"" +
                " }," +
                " \"transactionHistory\": \" \"" +
                " }," +
                " \"merchant\": {" +
                " \"merchantID\": \" \"," +
                " \"merchantName\": \" \"" +
                " }," +
                " \"customer\": {" +
                " \"custUsername\": \" \"," +
                " \"custPWD\": \" \"" +
                " }," +
                " \"transactions\": {" +
                " \"transactionID\": \" \"," +
                " \"transactionDate\": {" +
                " \"year\": \" \"," +
                " \"month\": \" \"," +
                " \"day\": \" \"" +
                " }," +
                " \"transactionTime\": {" +
                " \"hour\": \" \"," +
                " \"minute\": \" \"," +
                " \"second\": \" \"" +
                " }," +
                " \"merchantID\": { \"username\": \" \" }," +
                " \"debitAmount\": \" \"," +
                " \"creditAmount\": \" \"," +
                " \"balance\": \" \"," +
                " \"receiptNo\": \" \"" +
                " }" +
                " }" +
                "}";

            p.outputStream.WriteLine(parser.Parse(jsonText));      

        }
    }

    public class TestMain {
        public static int Main(String[] args) {
            HttpServer httpServer;
            if (args.GetLength(0) > 0) {
                httpServer = new MyHttpServer(Convert.ToInt16(args[0]));
            } else {
                httpServer = new MyHttpServer(80);
            }
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            return 0;
        }
    }
}



