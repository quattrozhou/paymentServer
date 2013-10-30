using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaymentServer;

namespace PaymentServer
{
    class ServerWorker
    {
        public static Boolean authenticateUser(MySQLDataHandler DBHandler, string authenticationString)
        {
            return true;
        }

        public static ServiceHandler.createProfleResultType createNewProfile(MySQLDataHandler DBHandler, ServiceHandler.UserProfile P)
        {
            string obj = "userProfile";

            string items = "(username, email, password, userType, firstName, lastName, middleName, DOBDay, DOBMonth, DOBYear, "+
                "occupation, SIN, address1, address2, city, province, country, postalCode, phoneNumber, receiveCommunication, "+
                "bankCode, accountNum, accountPWD, acctBalance, transactionHistory, POSHWID, currentDK, nextDK, authenticationString)";

            string values = "('"+P.username+"', '"+P.email+"', '"+P.password+"', '"+P.userType+"', '"+P.firstName+"', '"+P.lastName+"', '"+
                P.middleName+"', '"+P.DOBDay+"', '"+P.DOBMonth+"', '"+P.DOBYear+"', '"+P.occupation+"', '"+P.SIN+"', '"+P.address1+"', '"+
                P.address2+"', '"+P.city+"', '"+P.province+"', '"+P.country+"', '"+P.postalCode+"', '"+P.phoneNumber+"', '"+P.receiveCommunication+"', '"+
                P.bankCode+"', '"+P.accountNum+"', '"+P.accountPWD+"', '"+P.acctBalance+"', '"+P.transactionHistory+"', '"+P.POSHWID+"', '"+
                P.currentDK+"', '"+P.nextDK+"', '"+P.authenticationString+"')";
           
            DBHandler.Insert(obj, items, values);
            return ServiceHandler.createProfleResultType.RESULT_CREATE_PROFILE_SUCCESS;
        }

    }
}
