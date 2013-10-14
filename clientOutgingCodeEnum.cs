namespace PaymentServer
{

    public enum clientOutgoingCodeEnum
    {
	    /* Outgoing transaction message codes sent to mobile devices and web clients */

	    OUT_CODE_INVALID = -1,
	    OUT_CODE_LOGIN_SUCCESS = 0,
        OUT_CODE_LOGIN_FAILURE = 1,

	    
	    //all new codes should be placed above this line
	    OUT_CODE_MAX,


	    /* Incoming transaction message codes received from mobile devices and web clients */
	
		IN_CODE_INVALID = -1,
		IN_CODE_LOGIN_REQ = 0,
		IN_CODE_LOGIN_FAILURE = 1,

		IN_CODE_TRANSACTION_ID_RECEIVED_CONF = 2,
		IN_CODE_TRY_AGAIN_REQ = 3,
		IN_CODE_READY_FOR_RECEIPT_RESP = 4,
		IN_CODE_RECEIPT_VALID_MESSAGE = 5,
		IN_CODE_RECEIPT_INVALID_MESSAGE = 6,
		IN_CODE_READY_FOR_BALANCE_RESP = 7,
		IN_CODE_BALANCE_RECEIVED_CONF = 8,
		IN_CODE_SENDING_PAYMENT_AMOUNT = 9,
		IN_CODE_SENDING_MERCHANT_ACCT_NUM = 10,
		IN_CODE_SERVER_CONNECTION_STATUS_REQ = 11,
		IN_CODE_AUTH_STATUS_REQ = 12,
		IN_CODE_VERIFICATION_STATUS_REQ = 13,
		//all new codes should be placed above this line
		IN_CODE_MAX
    }
}
