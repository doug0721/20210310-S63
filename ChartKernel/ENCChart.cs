using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace AVCS.Kernel
{
    public enum TagName
    {
        CATD	= 0,
        DSID	= 1,
        DSSI	= 2,
        DSPM	= 3,
        VRID	= 4,
        ATTV	= 5,
        VRPC	= 6,
        VRPT	= 7,
        SGCC	= 8,
        SG2D	= 9,
        SG3D	= 10,
        FRID	= 11,
        FOID	= 12,
        ATTF	= 13,
        NATF	= 14,
        FFPC	= 15,
        FFPT	= 16,
        FSPC	= 17,
        FSPT	= 18,
    };
    public enum FldName
    {
       AALL	= 1,
       AGEN	= 2,
       ATTL	= 3,
       ATVL	= 4,
       CCUI	= 5,
       CCIX	= 6,
       CCNC	= 7,
       COUN	= 8,
       COMF	= 9,
       COMT	= 10,
       CSCL	= 11,
       DSNM	= 12,
       DSTR	= 13,
       DUNI	= 14,
       EDTN	= 15,
       EXPP	= 16,
       FIDS	= 17,
       FIDN	= 18,
       FFUI	= 19,
       FFIX	= 20,
       FSUI	= 21,
       FSIX	= 22,
       GRUP	= 23,
       HDAT	= 24,
       HUNI	= 25,
       INTU	= 26,
       ISDT	= 27,
       LNAM	= 28,
       MASK	= 29,
       NAME	= 30,
       NALL	= 31,
       NFPT	= 32,
       NOMR	= 33,
       NOCR	= 34,
       NOGR	= 35,
       NOLR	= 36,
       NOIN	= 37,
       NOCN	= 38,
       NOED	= 39,
       NOFA	= 40,
       NSPT	= 41,
       NVPT	= 42,
       OBJL	= 43,
       ORNT	= 44,
       PRSP	= 45,
       PSDN	= 46,
       PRED	= 47,
       PRIM	= 48,
       PROF	= 49,
       PUNI	= 50,
       RCNM	= 51,
       RCID	= 52,
       RIND	= 53,
       RVER	= 54,
       RUIN	= 55,
       SDAT	= 56,
       SOMF	= 57,
       STED	= 58,
       TOPI	= 59,
       UPDN	= 60,
       UADT	= 61,
       USAG	= 62,
       VE3D	= 63,
       VDAT	= 64,
       VPUI	= 65,
       VPIX	= 66,
       XCOO	= 67,
       YCOO	= 68,
    };

    public class S57TagParam
    {
        public FldName nm;
        public UInt16 bytesnum;
        public char format;

        public S57TagParam(string tag, UInt16 n, char f)
        {
            if (tag[0] == 'A')
            {
                if (tag.EndsWith("ALL")) nm = FldName.AALL;
                else if (tag.EndsWith("GEN")) nm = FldName.AGEN;
                else if (tag.EndsWith("TTL")) nm = FldName.ATTL;
                else if (tag.EndsWith("TVL")) nm = FldName.ATVL;
            }
            else if (tag[0] == 'C')
            {
                if (tag[1] == 'C')
                {
                    if (tag.EndsWith("UI")) nm = FldName.CCUI;
                    else if (tag.EndsWith("IX")) nm = FldName.CCIX;
                    else if (tag.EndsWith("NC")) nm = FldName.CCNC;
                }
                else if (tag[1] == 'O')
                {
                    if (tag.EndsWith("UN")) nm = FldName.COUN;
                    else if (tag.EndsWith("MF")) nm = FldName.COMF;
                    else if (tag.EndsWith("MT")) nm = FldName.COMT;
                }
                else if (tag.EndsWith("SCL")) nm = FldName.CSCL;
            }
            else if (tag[0] == 'D')
            {
                if (tag.EndsWith("SNM")) nm = FldName.DSNM;
                else if (tag.EndsWith("STR")) nm = FldName.DSTR;
                else if (tag.EndsWith("UNI")) nm = FldName.DUNI;
            }
            else if (tag[0] == 'E')
            {
                if (tag.EndsWith("DTN")) nm = FldName.EDTN;
                else if (tag.EndsWith("XPP")) nm = FldName.EXPP;
            }
            else if (tag[0] == 'F')
            {
                if (tag.EndsWith("IDS")) nm = FldName.FIDS;
                else if (tag.EndsWith("IDN")) nm = FldName.FIDN;
                else if (tag.EndsWith("FUI")) nm = FldName.FFUI;
                else if (tag.EndsWith("FIX")) nm = FldName.FFIX;
                else if (tag.EndsWith("SUI")) nm = FldName.FSUI;
                else if (tag.EndsWith("SIX")) nm = FldName.FSIX;
            }
            else if (tag == "GRUP") nm = FldName.GRUP;
            else if (tag[0] == 'H')
            {
                if (tag.EndsWith("DAT")) nm = FldName.HDAT;
                else if (tag.EndsWith("UNI")) nm = FldName.HUNI;
            }
            else if (tag[0] == 'I')
            {
                if (tag.EndsWith("NTU")) nm = FldName.INTU;
                else if (tag.EndsWith("SDT")) nm = FldName.ISDT;
            }
            else if (tag == "LNAM") nm = FldName.LNAM;
            else if (tag == "MASK") nm = FldName.MASK;
            else if (tag[0] == 'N')
            {
                if (tag[1] == 'O')
                {
                    if (tag.EndsWith("MR")) nm = FldName.NOMR;
                    else if (tag.EndsWith("CR")) nm = FldName.NOCR;
                    else if (tag.EndsWith("GR")) nm = FldName.NOGR;
                    else if (tag.EndsWith("LR")) nm = FldName.NOLR;
                    else if (tag.EndsWith("IN")) nm = FldName.NOIN;
                    else if (tag.EndsWith("CN")) nm = FldName.NOCN;
                    else if (tag.EndsWith("ED")) nm = FldName.NOED;
                    else if (tag.EndsWith("FA")) nm = FldName.NOFA;
                }
                else if (tag.EndsWith("AME")) nm = FldName.NAME;
                else if (tag.EndsWith("ALL")) nm = FldName.NALL;
                else if (tag.EndsWith("FPT")) nm = FldName.NFPT;
                else if (tag.EndsWith("SPT")) nm = FldName.NSPT;
                else if (tag.EndsWith("VPT")) nm = FldName.NVPT;
            }
            else if (tag[0] == 'O')
            {
                if (tag.EndsWith("BJL")) nm = FldName.OBJL;
                else if (tag.EndsWith("RNT")) nm = FldName.ORNT;
            }
            else if (tag[0] == 'P')
            {
                if (tag[1] == 'R')
                {
                    if (tag.EndsWith("SP")) nm = FldName.PRSP;
                    else if (tag.EndsWith("ED")) nm = FldName.PRED;
                    else if (tag.EndsWith("IM")) nm = FldName.PRIM;
                    else if (tag.EndsWith("OF")) nm = FldName.PROF;
                }
                else if (tag.EndsWith("SDN")) nm = FldName.PSDN;
                else if (tag.EndsWith("UNI")) nm = FldName.PUNI;
            }
            else if (tag[0] == 'R')
            {
                if (tag.EndsWith("CNM")) nm = FldName.RCNM;
                else if (tag.EndsWith("CID")) nm = FldName.RCID;
                else if (tag.EndsWith("IND")) nm = FldName.RIND;
                else if (tag.EndsWith("VER")) nm = FldName.RVER;
                else if (tag.EndsWith("UIN")) nm = FldName.RUIN;
            }
            else if (tag[0] == 'S')
            {
                if (tag.EndsWith("DAT")) nm = FldName.SDAT;
                else if (tag.EndsWith("OMF")) nm = FldName.SOMF;
                else if (tag.EndsWith("TED")) nm = FldName.STED;
            }
            else if (tag == "TOPI") nm = FldName.TOPI;
            else if (tag[0] == 'U')
            {
                if (tag.EndsWith("PDN")) nm = FldName.UPDN;
                else if (tag.EndsWith("ADT")) nm = FldName.UADT;
                else if (tag.EndsWith("SAG")) nm = FldName.USAG;
            }
            else if (tag[0] == 'V')
            {
                if (tag.EndsWith("E3D")) nm = FldName.VE3D;
                else if (tag.EndsWith("DAT")) nm = FldName.VDAT;
                else if (tag.EndsWith("PUI")) nm = FldName.VPUI;
                else if (tag.EndsWith("PIX")) nm = FldName.VPIX;
            }
            else if (tag == "XCOO") nm = FldName.XCOO;
            else if (tag == "YCOO") nm = FldName.YCOO;

            bytesnum = n;
            format = f;

        }
        public string GetString(byte[] data, int pos, int endpos, bool bUnicode)
        {
            Encoding encod = bUnicode? Encoding.Unicode : Encoding.GetEncoding("ISO-8859-1"); //Encoding.UTF7;

            UInt16 num = bytesnum;
            if(num == 0)
            {
                int idx = pos;
                if (bUnicode)
                {
                    for (; idx < endpos; idx += 2)
                    {
                        if (BitConverter.ToChar(data, idx) == 0x1f)
                            break;
                    }
                }
                else idx = Array.FindIndex(data, pos, endpos - pos, d=>d == 0x1f);
                if (idx != -1)
                {
                    num = (UInt16)(idx - pos);
                }
                else num = 0;
            }
            return encod.GetString(data, pos, num);
        }
    };
    public class S57Tag : List<S57TagParam>
    {
        public TagName nm;
        public byte lexLevel;
        public S57Tag(string tag, byte[] data, int pos, int len)
        {
            if (tag == "CATD") nm = TagName.CATD;
            else if (tag == "DSID") nm = TagName.DSID;
            else if (tag == "DSSI") nm = TagName.DSSI;
            else if (tag == "DSPM") nm = TagName.DSPM;
            else if (tag == "VRID") nm = TagName.VRID;
            else if (tag == "ATTV") nm = TagName.ATTV;
            else if (tag == "VRPC") nm = TagName.VRPC;
            else if (tag == "VRPT") nm = TagName.VRPT;
            else if (tag == "SGCC") nm = TagName.SGCC;
            else if (tag == "SG2D") nm = TagName.SG2D;
            else if (tag == "SG3D") nm = TagName.SG3D;
            else if (tag == "FRID") nm = TagName.FRID;
            else if (tag == "FOID") nm = TagName.FOID;
            else if (tag == "ATTF") nm = TagName.ATTF;
            else if (tag == "NATF") nm = TagName.NATF;
            else if (tag == "FFPC") nm = TagName.FFPC;
            else if (tag == "FFPT") nm = TagName.FFPT;
            else if (tag == "FSPC") nm = TagName.FSPC;
            else if (tag == "FSPT") nm = TagName.FSPT;

            String lexlev = Encoding.UTF8.GetString(data, pos + 6, 3);
            lexLevel = (byte)(lexlev == "   " ? 0 : (lexlev == "-A " ? 1 : (lexlev == "%/A"? 2 : 255)));
            if(lexLevel == 255) throw new SystemException("Incorrect lexical level defined in data descriptive field (DDR) for tag " + tag);

            byte[] UT = new byte[2] { 31, 0 };//0x1f
            byte[] FT = new byte[2]{30,0};//0x1e
            string descr = Encoding.UTF8.GetString(data, pos + 9, len - 9);
            String[] strar = descr.Split(Encoding.UTF8.GetChars(UT));
            if (strar.Length != 3) throw new SystemException("Incorrect format of data descriptive field (DDR) for tag " + tag);
            strar[1] = strar[1].TrimStart('*');
            string[] spars = strar[1].Split('!');
            descr = strar[2].Trim(new char[]{'(', ')', (char)(30)});
            string[] fmts = descr.Split(',');
            int nn = 0;
            foreach (String fmt in fmts)
            {
                Regex r = new Regex(@"(?<cnt>\d{0,2})(?<format>[AIRB@b])(?<btype>\d{0,2})(\((?<num>\d{0,})\)){0,1}", RegexOptions.Compiled);
                Match res = r.Match(fmt);
                if (!res.Success)
                    throw new SystemException("Incorrect format of data descriptive field (DDR) for tag " + tag + "field format: " + fmt);

                char format = res.Groups["format"].Value[0];
                String str = res.Groups["cnt"].Value;
                int num = 0, cnt = (str.Length > 0) ? int.Parse(str) : 1;
                if (format == 'b')
                {
                    str = res.Groups["btype"].Value;
                    if (str.Length < 2)
                        throw new SystemException("Incorrect format of data descriptive field (DDR) for tag " + tag + "field format: " + fmt);
                    if (str[1] == '2') format = 'c';
                    num = int.Parse(str.Substring(1));
                }
                else
                {
                    str = res.Groups["num"].Value;
                    num = str.Length > 0 ? int.Parse(str) : 0;
                    if (format == 'B') num /= 8;
                }
                for (int i = 0; i < cnt; i++)
                {
                    if (nn > spars.Length)
                        throw new SystemException("Incorrect format of data descriptive field (DDR) for tag " + tag + ". Parameters number doesn't match formats number");
                    Add(new S57TagParam(spars[nn++], (UInt16)num, format));
                }
            }
        }
    };
    public class BaseCellInfo
    {
        internal UInt16 _wEdtNum;
        internal UInt16 _wUpdNum;

        public UInt16 Edtn { get { return _wEdtNum; } }
        public UInt16 Updt { get { return _wUpdNum; } }
        public BaseCellInfo()
        {
	        _wEdtNum = 0;
	        _wUpdNum = 0;
        }
    }

    public class ENCChart:BaseCellInfo
    {
	    string		_strFileName;

        public string FileName { get { return _strFileName; } }
        public ENCChart()
        {
	        _strFileName = "";
        }
        static public string GetDate(string str)
        {
           String outstr = str.Insert(6, "/");
           return outstr.Insert(4, "/");
        }
    }
}
