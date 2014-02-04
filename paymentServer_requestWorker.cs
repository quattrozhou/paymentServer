﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using PaymentServer;

//  this is test_chang, welcome
namespace PaymentServer
{
    class paymentServer_requestWorker
    {
         /*
         * Authenticate user
         */
        public static Boolean authenticateUser(paymentServer_dataBase DBHandler, string authenticationString)
        {
            
            int count = DBHandler.Count("*", "authenticationList WHERE authenticationString='"+authenticationString+"'");

            if (count >= 1)
            {
                return true;
            }
            return false;
        }

        /*
         * create new user profile
         */
        public static ResultCodeType createNewProfile(paymentServer_dataBase DBHandler, UserProfile P)
        {
            /// check if this username is taken
            int count = DBHandler.Count("*", "userProfile WHERE username='" + P.username + "'");
            if (count != 0)
            {
                Console.WriteLine("ResultCodeType.ERROR_CREATE_PROFILE_USERNAME_EXISTS");
                return ResultCodeType.ERROR_CREATE_PROFILE_USERNAME_EXISTS;
            }
                
            /// check if this email is taken
            count = DBHandler.Count("*", "userProfile WHERE email='" + P.email + "'");
            if (count != 0)
            {
                Console.WriteLine("ResultCodeType.ERROR_CREATE_PROFILE_EMAIL_EXISTS");
                return ResultCodeType.ERROR_CREATE_PROFILE_EMAIL_EXISTS;
            }
                
            /// create a new profile in the database
            string profile = "userProfile";
            string items = P.getDatabaseColumnList();
            string values = P.getDatabaseValueList();

            DBHandler.Insert(profile, items, values);
            DBHandler.Insert("authenticationList", "(authenticationString)", "('" + P.authenticationString + "')");

            return ResultCodeType.SUCC_CREATE_PROFILE;
        }

        /// <summary>
        /// get user profile from database, search by username
        /// </summary>
        /// <param name="DBHandler"></param>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public static GetProfileResultType MYgetUserProfileByUsername
            (paymentServer_dataBase DBHandler, string username)
        {
            GetProfileResultType reply = new GetProfileResultType();
            reply.status = ResultCodeType.ERROR_GET_USER_PROFILE;

            int count = DBHandler.Count("*", "userProfile WHERE username='" + username + "'");

            if (count != 1)
            {
                Console.WriteLine("ERROR - MYgetUserProfileByUsername!"+
                    "Trying to get user profile, found " + count + " profiles match username");
                return reply;
            }

            List<string> list = DBHandler.selectWholeRow("" + username);

            UserProfile Profile = new UserProfile(list);

            reply.status = ResultCodeType.SUCC_GET_USER_PROFILE;
            reply.profile = (UserProfile)Profile;

            return reply;
        }

        /// <summary>
        /// put transaction record into database
        /// </summary>
        /// <param name="DBHandler"></param>
        /// <param name="tr"></param>
        /// <returns>
        /// Transaction ID (assigned by database)
        /// </returns>
        /*public static int addNewTransactionRecord
            (paymentServer_dataBase DBHandler, transactionRecord tr)
        {
            string profile = "transactionhistory";
            string items = tr.getDatabaseColumnList();
            string values = tr.getDatabaseValueList();
            DBHandler.Insert(profile, items, values);
            string result = DBHandler.selectColumn(profile, "receiptNumber", tr.receiptNumber, "transactionNo");
            try
            {
                int tid = Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return 0;
        }*/

        public static ResultCodeType updateBalance(paymentServer_dataBase DBHandler, string username, long balance)
        {
            ResultCodeType res = new ResultCodeType();
            res = ResultCodeType.ERROR_UPDATE_USER_PROFILE;

            DBHandler.Update("userProfile", "acctBalance = " + balance + "", "username = '" + username+"'");

            res = ResultCodeType.SUCC_UPDATE_USER_PROFILE;
            return res;
        }

        /// <summary>
        /// get user profile from database, search by username
        /// </summary>
        /// <param name="DBHandler"></param>
        /// <param name="userNo"></param>
        /// <returns></returns>
        /*public static GetProfileResultType getUserProfileByUsername(paymentServer_dataBase DBHandler, string username)
        {
            GetProfileResultType reply = new GetProfileResultType();
            reply.status = ResultCodeType.ERROR_UNKNOWN;

            List<string>[] list = DBHandler.Select("userProfile", "username", "" + username);
            if (list.Length != 1)
            {
                Console.WriteLine("ServerWorker::getUserProfile - Error: Database query returned not one record. Number Received: " + list.Length);
                return reply;
            }

            if (list[0].Count() != (int)UserProfileEnum.NUM_PROFILE_DATA_ITEMS)
            {
                Console.WriteLine("ServerWorker::getUserProfile - Error: Did not receive extpected number of data items from server. Received: {0}, Expected: {1}", list[0].Count(), (int)UserProfileEnum.NUM_PROFILE_DATA_ITEMS);
                // Console.WriteLine("list[0]: "+list[0][0]+" list[0]size: "+list[0].Count);
                return reply;
            }

            Console.WriteLine("start get!");

            string String = "";
            int Int = 1;
            bool Bool = false;
            double Double = 0.1;

            object Profile = new UserProfile();
            PropertyInfo[] properties = Profile.GetType().GetProperties();
            int i;
            for (i = 0; i < properties.Length; i++)
            {
                Console.WriteLine("gup: " + properties[i].GetValue(Profile, null).ToString());
                if (properties[i].GetType() == String.GetType())
                {
                    properties[i].SetValue(Profile, (string)list[0][i], null);

                }
                else if (properties[i].GetType() == Double.GetType())
                {
                    properties[i].SetValue(Profile, Convert.ToDouble(list[0][i]), null);
                }
                else if (properties[i].GetType() == Int.GetType())
                {
                    properties[i].SetValue(Profile, Convert.ToInt32(list[0][i]), null);

                }
                else if (properties[i].GetType() == Bool.GetType())
                {
                    properties[i].SetValue(Profile, Convert.ToBoolean(list[0][i]), null);
                }
                reply.status = ResultCodeType.UPDATE_USER_PROFILE_SUCCESS;
                reply.profile = (UserProfile)Profile;
            }
            
            return reply;
        }*/

         /*
         * Get user profile
         */
        /*public static GetProfileResultType getUserProfile(paymentServer_dataBase DBHandler, int userNo)
        {         
            GetProfileResultType reply = new GetProfileResultType();
            reply.status = ResultCodeType.ERROR_UNKNOWN;

            List<string>[] list = DBHandler.Select("userProfile", "userNo", ""+userNo);
            if (list.Length == 1)
            {
                if (list[0].Count() == (int)UserProfileEnum.NUM_PROFILE_DATA_ITEMS)
                {
                    string String = "";
                    int Int = 1;
                    bool Bool = false;
                    double Double = 0.1;

                    object Profile = new UserProfile();
                    PropertyInfo[] properties = Profile.GetType().GetProperties();
                    int i;
                    for (i = 0; i < properties.Length; i++)
                    {
                        Console.WriteLine(properties[i].GetValue(Profile, null).ToString());
                        if (properties[i].GetType() == String.GetType())
                        {
                            properties[i].SetValue(Profile, (string)list[0][i], null);

                        }
                        else if (properties[i].GetType() == Double.GetType())
                        {
                            properties[i].SetValue(Profile, Convert.ToDouble(list[0][i]), null);
                        }
                        else if (properties[i].GetType() == Int.GetType())
                        {
                            properties[i].SetValue(Profile, Convert.ToInt32(list[0][i]), null);

                        }
                        else if (properties[i].GetType() == Bool.GetType())
                        {
                            properties[i].SetValue(Profile, Convert.ToBoolean(list[0][i]), null);
                        }
                        reply.status = ResultCodeType.UPDATE_USER_PROFILE_SUCCESS;
                        reply.profile = (UserProfile)Profile;
                    }
                }
                else
                {
                    // Console.WriteLine("ServerWorker::getUserProfile - Error: Did not receive extpected number of data items from server. Received: {}, Expected: {}", list[0].Count(), (int)UserProfileEnum.NUM_PROFILE_DATA_ITEMS);
                }

            }

            else
            {
                Console.WriteLine("ServerWorker::getUserProfile - Error: Database query returned more than one record. Number Received: {0}", list.Length);
            }
            return reply;
        
        }*/

        public static ResultCodeType addNewTransactionRecord_toCustomerField(
            paymentServer_dataBase DBHandler, transactionRecord tr)
        {
            ResultCodeType res = new ResultCodeType();
            res = ResultCodeType.ERROR_UPDATE_USER_PROFILE;

            string oldTH = DBHandler.selectColumn("userProfile", "username", "" + tr.customerUsername, "transactionHistory");
            // if (list.Length != 1) return res; // exit if there is an error in database

            // if (list[0].Count() != (int)UserProfileEnum.NUM_PROFILE_DATA_ITEMS) return res; // exit if there is an error in one entry

            res = ResultCodeType.ERROR_UPDATE_USER_PROFILE;

            // UserProfile p = new UserProfile();
            // PropertyInfo[] properties = p.GetType().GetProperties();

            // string transactionHistory = list[0][25];

            oldTH = tr.MyToString() + "\n\n" + oldTH;

            DBHandler.Update("userProfile", "transactionHistory = '" + oldTH + "'", "username = '" + tr.customerUsername+"'");

            res = ResultCodeType.SUCC_UPDATE_USER_PROFILE;
            return res;
        }
    }
}
