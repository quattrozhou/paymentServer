using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static int contactBankServer()
        {
            return 0;
        }

        public static int sendTransaction()
        {
            return 0;
        }
    }

    class transactionRecord
    {
        private DateTime transactionTime;
        private string customerName;
        private string merchantName;

        // amount > 0: customer is sending money to merchant
        // amount < 0: merchant is refunding money back to customer
        private int amount;
        
        public transactionRecord (int y, int m, int d, int h, int min, int s, string cn, string mn, int a)
        {
            DateTime transactionTime = new DateTime(y,m,d,h,min,s);
            this.customerName = cn;
            this.merchantName = mn;
            this.amount = a;
        }

        public transactionRecord(DateTime d, string cn, string mn, int a)
        {
            this.transactionTime = d;
            this.customerName = cn;
            this.merchantName = mn;
            this.amount = a;
        }

        public DateTime getTime()
        {
            return transactionTime;
        }

        public string toString()
        {
            return "" + transactionTime.ToString() + " A: " + customerName + " B: " + merchantName + " amount: " + amount;
        }
    }
}
