public class ConstantsResponse
{
    // responses
    public const int SRV_REJECT = 0x80;
    public const int SRV_INVALID_TOKEN = 0xEEFFAA;
    public const int SRV_INVALID_PASSWORD = 0xDDEECC;
    public const int SRV_NACCESS_MASTER = 0xFFCCEE;
    public const int SRV_LOGIN_COMPLETED = 0xFCAADD;
    public const int SRV_LOGOUT_COMPLETED = 0xFCAACC;
    public const int SRV_ORDER_LIST = 0xDDDEEE;
    public const int SRV_ACCOUNT_LIST = 0xDBCDEE;
    public const int SRV_UPDATE_ORDER = 0x872CC3;
    public const int SRV_UPDATE_USER = 0x872AA3;
    public const int SRV_SELECT_ORDER = 0x8734763;
    public const int SRV_SELECT_USER = 0xFF6658;
    public const int SRV_PRODUCT_LIST = 0xDDBBC86;
    public const int SRV_SELECT_PRODUCT = 0xD3530CC;
    public const int SRV_UPDATE_PRODUCT = 0xD2130CC;
    public const int SRV_ORDER_POINTS = 0xFFAB77;
    public const int SRV_TOP_PRODUCT = 0x89765F;
    public const int SRV_PERSON_ANALYTICS = 0x45FBCC;
    public const int SRV_N_PRODUCT_LIST = 0x78B244;
    public const int SRV_N_INVOICE_LIST = 0x762FF;
    public const int SRV_N_ORDER_LIST = 0x82354;
    public const int SRV_FRPW_RESULT = 0x98659;
    public const int SRV_N_USER_INFO = 0xCDE34;
    public const int SRV_N_FULL_INVOICE = 0xFF7C386;
    public const int SRV_N_UPDATE_PROPS = 0xFFBCC65;
    public const int SRV_ORDER_NOTIFY = 0x34525F;
    public const int SRV_N_ORDER_NOTIFY = 0x245F3;

    // request
    public const int CLT_LOGIN = 0x86FFCC;
    public const int CLT_LOGOUT = 0x86EECC;
    public const int CLT_ORDER_LIST = 0x874FFF;
    public const int CLT_USER_LIST = 0xDFFEEE;
    public const int CLT_PRODUCT_LIST = 0xFFBCB;
    public const int CLT_APPLY_MASTER = 0x876333;
    public const int CLT_SELECT_ORDER = 0x863572;
    public const int CLT_ORD_STATUS = 0x872583;
    public const int CLT_DELETE_ORDER = 0xFCBC87;
    public const int CLT_UPDATE_USER = 0xFBBCC87;
    public const int CLT_DELETE_USER = 0xFDBCC88;
    public const int CLT_SELECT_USER = 0xFD08C;
    public const int CLT_CREATE_USER = 0x9818C;
    public const int CLT_SELECT_PRODUCT = 0x77265D;
    public const int CLT_UPDATE_PRODUCT = 0xDD4587;
    public const int CLT_GET_ORDER_POINTS = 0xFFBB77;
    public const int CLT_TOP_PRODUCT = 0xFFCCBB;
    public const int CLT_PERSON_ANALYTICS = 0x45FFCC;
    public const int CLT_N_PRODUCT_LIST = 0xCCBB89;
    public const int CLT_N_INVOICE_LIST = 0xDDCC89;
    public const int CLT_N_ORDER_LIST = 0xDD2489;
    public const int CLT_N_CANCEL_ORDER = 0x34BB89;
    public const int CLT_N_NEW_INVOICE = 0xCCEE90;
    public const int CLT_N_NEW_ORDER = 0xCCEE80;
    public const int CLT_N_USER_INFO = 0xFF4B86;
    public const int CLT_FORGOT_TOKEN = 0xCCDDFE;
    public const int CLT_FORGOT_PASSWORD = 0xCC4DFF;
    public const int CLT_N_CHANGE_PASS = 0xCC23FF;
    public const int CLT_N_UPDATE_INVOICE = 0x77CC66;
    public const int CLT_N_FULL_INVOICE = 0xFF7F386;
}
