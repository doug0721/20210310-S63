using System.ComponentModel;

namespace ENC.Params
{
    public enum S63Error : int
    {
        [Description("Self Signed Key is invalid")]
        SSKeyWrong = -1,
        [Description("Format of Self Signed Key file is incorrect")]
        SSKeyFmtErr = -2,
        [Description("SA Signed Data Server Certificate is invalid")]
        SADSCertWrong = -3,
        [Description("Format of SA Signed DS Certificate is incorrect")]
        SADSCertFmtErr = -4,
        [Description("SA Digital Certificate (X509)  file is not available")]
        X509CertNotAval = -5,
        [Description("Format of private key is invalid")]
        PrivKeyFmtErr = -6,
        [Description("SA signed DS Certificate file is not available")]
        SADSCertNotAval = -7,
        [Description("The format of the SA Digital Certificate (X509) file or public key is incorrect")]
        PubKeyFmtErr = -8,
        [Description("ENC signature is invalid")]
        ENCSignWrong = -9,
        [Description("Permits not available for this data provider. Contact your data supplier to obtain the correct permits")]
        PmtsNotAval = -10,
        [Description("Cell permit not found. Contact your data supplier")]
        CellPmtNotFound = -11,
        [Description("Cell Permit format is incorrect. Obtain a new permit file from your data supplier")]
        CellPmtFmtErr = -12,
        [Description("Cell Permit is invalid (checksum is incorrect). Obtain a new permit file from your data supplier")]
        CellPmtCRCErr = -13,
        [Description("Incorrect system date, check that the computer clock is set correctly or contact your data supplier")]
        SysDateWrong = -14,
        [Description("Cell Permit has expired. Please contact your data supplier to renew the subscription licence")]
        CellPmtExpired = -15,
        [Description("ENC CRC value is incorrect.Contact your data supplier as ENC(s) may be corrupted or missing data")]
        CellCRCErr = -16,
        [Description("Wrong Data Server name")]
        CellPmtWrongDS = -18,
        [Description("SA Digital Certificate (X509) has expired")]
        X509CrtExpired = -19,
        [Description("Decryption failed, no valid cell permit found. Permits may be for another system or new permits maybe required, please contact your supplier to obtain a new licence")]//MK_ERR_CANNOT_UNZIP_CELL
        UnzipCellErr = -20,
        [Description("Incorrect hardware ID")]
        HWIDFmtErr = -22,
        [Description("File is not available")]
        FileNotAval = -24,
        [Description("Incorrect cell name or cell folder name")]
        CellPathErr = -25,
        [Description("Exception.")]
        Exception = -27,
        [Description("Incorrect cell file format.")]
        S57FmtErr = -28,
        [Description("Can not open S57 cataloque file.")]
        S57CatOpenErr = -29,
        [Description("Incorrect collection folders structure. Run setup program.")]
        FolderStructErr = -30,
        [Description("Can not load cell file")]
        CellLoadErr = -34,
        [Description("Can not create binary catalogue")]
        BinCatCreateErr = -42,
        [Description("Already applied")]
        AlreadyAppliedErr = -46,
        [Description("Wrong edition number - file rejected")]
        WrongEdtnErr = -47,
        [Description("Non sequential update, previous update(s) missing try reloading from the base media")]
        WrongUpdnErr = -48,
        [Description("The same or newer edition already installed - file skipped")]
        CellExistErr = -49,
        [Description("Cell is encrypted and cannot be decrypted without S57 catalog file.")]
        CellDecryptErr = -55,
        [Description("Cannot find one of presentation library files in specified directory")]
        PresLibReadErr = -57,
        [Description("Impossible to apply update. Base cell is not installed")]
        BaseCellAbsentErr = -60,
        [Description("The cell is already cancelled")]
        AlreadyCanceledErr = -62,
        [Description("SENC protection error.Chart(s) can not be loaded: Incorrect HW_ID")]
        CellProtectErr = -65,
        [Description("The same or newer permit is already installed")]
        ValidPmtExistErr = -67,
        [Description("SENC format is incorrect")]
        SencFmtErr = -68,
        [Description("Cell is canceled")]
        CellCancelledErr = -71,
        [Description("User is not registered")]
        UserNotRegistered = -72,
        [Description("OK")]
        SUCCESS = 0,
        [Description("Warning! Cell Permit will soon expire.Please contact your data supplier to renew the subscription licence")]
        PmtWillExpireWarn = 1,
        [Description("Warning! Cell is deleted because of cancelation update")]
        CancUpdateWarn = 7,
        [Description("Warning! This ENC is not authenticated by the IHO acting as the Scheme Administrator")]
        NonSAAuthentWarn = 13,
        [Description("Warning! Permit has expired. Contact your distributor for a new permit.")]
        ParmitExpiredWarn = 14,
    };
}
