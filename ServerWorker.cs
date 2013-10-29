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
        public static Boolean authenticateUser(string authenticationString)
        {
            return true;
        }

        public static ServerApp.createProfleResultType createNewProfile(ServerApp.UserProfile newProfile)
        {
            return ServerApp.createProfleResultType.RESULT_CREATE_PROFILE_SUCCESS;
        }

    }
}
