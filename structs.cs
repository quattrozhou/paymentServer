﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentServer
{
    public struct GetProfileResultType
    {
        public UserProfile profile;
        public ResultCodeType status;
    };

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
        ERROR_TRANSACTION_HISTORY_GET_PROFILE = 8,
        ERROR_TRANSACTION_HISTORY_UPDATE = 9,
        SUCC_TRANSACTION_HISTORY_UPDATE = 10,
        ERROR_CREATE_PROFILE_USERNAME_EXISTS = 11,
        ERROR_CREATE_PROFILE_EMAIL_EXISTS = 12,
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
        OUT_CODE_SIGN_UP_FAILURE= 3,
        OUT_CODE_SEND_USER_PROFILE_SUCCESS = 4,
        OUT_CODE_SEND_USER_PROFILE_FAILURE = 5,
        OUT_CODE_TRANSACTION_CUSTOMER_AUTH_SUCCESS = 6,
        OUT_CODE_TRANSACTION_CUSTOMER_AUTH_FAILURE = 7,
        OUT_CODE_TRANSACTION_MERCHANT_AUTH_SUCCESS = 8,
        OUT_CODE_TRANSACTION_MERCHANT_AUTH_FAILURE = 9,
        OUT_CODE_TRANSACTION_BANK_NOT_ACCESSABLE = 10,
        OUT_CODE_TRANSACTION_BANK_TRANSACTION_APPROVED = 11,
        OUT_CODE_TRANSACTION_BANK_TRANSACTION_FAILED = 12,
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
        IN_CODE_PROCESS_PAYMENT_REQ = 3,
        //all new codes should be placed above this line
        IN_CODE_MAX
    };

    public class UserProfile
    {
        // DO NOT CHANGE ORDER, if need add any thing, please add to the end of each field
        public int userNo;
        public string email;
        public string username;
        public string password;             //base64-encoded
        public string userType;
        public string firstName;
        public string middleName;
        public string lastName;
        public int DOBDay;
        public int DOBMonth;
        public int DOBYear;
        public string occupation;
        public int SIN;
        public string address1;
        public string address2;
        public string city;
        public string province;
        public string country;
        public string postalCode;
        public int phoneNumber;
        public int receiveCommunication;
        public string bankCode;             //base64-encoded
        public string accountNum;           //base64-encoded
        public string accountPWD;      //base64-encoded
        public double acctBalance;          //base64-encoded
        public string transactionHistory;
        public int POSHWID;
        public string currentDK;            //base64-encoded
        public string nextDK;               //base64-encoded
        public string authenticationString;  //base64-encoded
        // public string createTime;

        public UserProfile()
        { }

        public UserProfile(List <string> input)
        {
            if (input.Count < 30)
                return;

            this.userNo = Convert.ToInt32(input[0]);
            this.email = input[1];
            this.username = input[2];
            this.password = input[3];        
            this.userType = input[4];
            this.firstName = input[5];
            this.middleName = input[6];
            this.lastName = input[7];
            this.DOBDay = Convert.ToInt32(input[8]);
            this.DOBMonth = Convert.ToInt32(input[9]);
            this.DOBYear = Convert.ToInt32(input[10]);
            this.occupation = input[11];
            this.SIN = Convert.ToInt32(input[12]);
            this.address1 = input[13];
            this.address2 = input[14];
            this.city = input[15];
            this.province = input[16];
            this.country = input[17];
            this.postalCode = input[18];
            this.phoneNumber = Convert.ToInt32(input[19]);

            if(input[20].Equals("True")) this.receiveCommunication = 1;
            else this.receiveCommunication = 0;

            this.bankCode = input[21];        
            this.accountNum = input[22];      
            this.accountPWD = input[23]; 
            this.acctBalance = Convert.ToDouble(input[24]);
            this.transactionHistory = input[25];
            this.POSHWID = Convert.ToInt32(input[26]);
            this.currentDK = input[27];       
            this.nextDK = input[28];          
            this.authenticationString = input[29];
            // this.createTime = input[30];

        }
    };

    public struct TransactionResult
    {
        public FromBankServerMessageTypes status;
        public string transactionMessage;
        public string receiptNumber;
    }

    public enum ToBankServerMessageTypes
    {
        TO_BANK_SERVER_LOGIN = 1,
        TO_BANK_SERVER_TRANSACTION = 2,
        // all new codes should be placed above this line
        TO_BANK_CODE_MAX
    };

    public enum FromBankServerMessageTypes
    {
        FROM_BANK_CONNECTION_FAIL = 0,
        FROM_BANK_LOGIN_ACK = 1,
        FROM_BANK_LOGIN_NACK = 2,
        FROM_BANK_TRANSACTION_ACK = 3,
        FROM_BANK_TRANSACTION_NACK = 4,
        ERROR_BEFORE_CONTACT_BANK = 90,
        ERROR_AUTHENDICATION_CUSTOMER = 91,
        ERROR_AUTHENDICATION_MERCHANT = 92,
        // all new codes should be placed above this line
        FROM_BANK_CODE_MAX
    };

    class transactionRecord
    {
        public DateTime time;
        public string customerName;
        public string merchantName;
        public string amount;
        public Boolean isRefund;
        public FromBankServerMessageTypes status;
        public string transactionMessage;
        public string receiptNumber;
        public int userNo;

        public transactionRecord(int y, int m, int d, int h, int min, int s, string cn, string mn, string a)
        {
            this.time = new DateTime(y, m, d, h, min, s);
            this.customerName = cn;
            this.merchantName = mn;
            this.amount = a;
            isRefund = false;
            status = FromBankServerMessageTypes.ERROR_BEFORE_CONTACT_BANK;
            transactionMessage = "";
            receiptNumber = "";
            // userNo = 0;
        }

        public transactionRecord(DateTime d, string cn, string mn, string a)
        {
            this.time = d;
            this.customerName = cn;
            this.merchantName = mn;
            this.amount = a;
            isRefund = false;
            status = FromBankServerMessageTypes.ERROR_BEFORE_CONTACT_BANK;
            transactionMessage = "";
            receiptNumber = "";
            // userNo = 0;
        }

        public transactionRecord()
        {
            time = new DateTime();
            customerName = "";
            merchantName = "";
            amount = "0";
            isRefund = false;
            status = FromBankServerMessageTypes.ERROR_BEFORE_CONTACT_BANK;
            transactionMessage = "";
            receiptNumber = "";
            // userNo = 0;
        }

        public string MyToString()
        {
            return "" + time.ToString() +
                " payer: " + customerName +
                " payee: " + merchantName +
                " amount: " + amount +
                " isRefund: " + isRefund +
                " status: " + (int)status +
                " message: " + transactionMessage +
                " receiptNumber: " + receiptNumber;
                // " userNo: " + userNo;
        }

        public string getDatabaseColumnList()
        {
            return "(time_year, time_month, time_day, time_hour, time_minute, time_second, " +
                "customerName, merchantName, amount, isRefund, status, transactionMessage, receiptNumber)";
        }

        public string getDatabaseValueList()
        {
            return "('" + this.time.Year + "','" + this.time.Month + "','" + this.time.Day + "','" +
                this.time.Hour + "','" + this.time.Minute + "','" + this.time.Second + "','" +
                this.customerName + "','" + this.merchantName +
                "','" + this.amount + "','" + booleanToInt(this.isRefund) + 
                "','" + (int)(this.status) + "','" +
                this.transactionMessage + "','" + this.receiptNumber + "')";
        }

        public static int booleanToInt(Boolean input)
        {
            if (input)
                return 1;
            else
                return 0;
        }
    }

    class structs
    {
    }
}
