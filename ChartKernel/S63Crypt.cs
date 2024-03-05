using System;
using System.Globalization;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using ENC.Params;

namespace AVCS.Kernel
{
    public class CrtInfo
    {
        public DSACryptoServiceProvider _dsaPK = null;
        public bool _bIHO = false;
        public S63Error _ErrStatus = S63Error.X509CertNotAval;
        public string _Issuer = "";
        public string _Expire = "";
        public string _strPubKey = "";
    }

    public static class S63Crypt
    {
        public static int[] PolyCoefficients = new int[15]{0, 1, 2, 4, 5, 7, 8, 10, 11, 12, 16, 22, 23, 26, 32};
        public static UInt32 Polynomial = 0;
        public static string _MKey =""; 
        public static string _MId = ""; 
        public static byte [] _HWID;
        public static string _UP = ""; 

        public static void Dword2Byte(UInt32 dwval, Byte[] bytear, int StartIdx)
        {
            bytear[StartIdx + 3] = (Byte)(dwval & 0x000000ff);
            bytear[StartIdx + 2] = (Byte)((dwval & 0x0000ff00) >> 8);
            bytear[StartIdx + 1] = (Byte)((dwval & 0x00ff0000) >> 16);
            bytear[StartIdx] = (Byte)((dwval & 0xff000000) >> 24);
        }
        public static UInt32 Byte2Dword(Byte[] bytear, int startidx)
        {
            return (((UInt32)bytear[startidx] << 24) + ((UInt32)bytear[startidx + 1] << 16) + ((UInt32)bytear[startidx + 2] << 8) + (UInt32)bytear[startidx + 3]);
        }

        /// <summary>
        /// Get encryption keys from ENC permit string
        /// </summary>
        /// <param name="permit">permit string formated according S63 standard</param>
        /// <param name="byHWID">binary HW_ID of  5 bytes length minimum</param>
        /// <param name="CK1">first encryption key</param>
        /// <param name="CK2">second encryption key</param>
        /// <returns>
        /// CryptResult.HWIDFmtErr - if HW_ID length less than 5 bytes
        /// CryptResult.CRCErr - if permit CRC value is incorrect (possible permit is encrypted with other HW_ID)
        /// return CryptResult.OK - if CK1 and CK2 keys are successfully created and decrypted 
        /// </returns>
        public static S63Error GetEncKeysFromPermit(String permit, BFAlg bf, ref Byte[] CK1, ref Byte[] CK2)
        {
            //convert permit string to Byte array
            Byte[] pmtar = Encoding.UTF8.GetBytes(permit);

	        //--------- calculate CRC32 for left part of Cell permit(48 bytes)
            ICSharpCode.SharpZipLib.Checksums.Crc32 crcProc = new ICSharpCode.SharpZipLib.Checksums.Crc32();
	        crcProc.Update(pmtar, 0, 48);

            Byte[] bcrc32 = new Byte[8];
            Dword2Byte((UInt32)crcProc.Value, bcrc32, 0);//convert DWORD to BYTES
            bcrc32[4] = bcrc32[5] = bcrc32[6] = bcrc32[7] = 0x04;	//padding for Blowfish

	        //--- encrypt CRC32 by Blowfish alghorithm -------------------
            bf.Encrypt(bcrc32);
            
            //convert the result of crc encryption to hexadecimal string presentation
            String  hexcrc = "";
            int i, j, k;
            for(i = 0; i < 8; i++) hexcrc += bcrc32[i].ToString("X2");
        	
            //check permit validity
            if(permit.Length < (48 + 16) || hexcrc != permit.Substring(48, 16))
                return  S63Error.CellPmtCRCErr; // Cell Permit is invalid (checksum is incorrect)

            CK1 = new Byte[8];
            CK2 = new Byte[8];
	        
            for(i = 0, j = 16, k = 32; k < 48; j += 2, k += 2)
            {
                CK1[i] = Byte.Parse(permit.Substring( j, 2), NumberStyles.AllowHexSpecifier);
                CK2[i++] = Byte.Parse(permit.Substring(k, 2), NumberStyles.AllowHexSpecifier);
            }

            bf.Decrypt(CK1);
            bf.Decrypt(CK2);
            Array.Resize(ref CK1, 5);
            Array.Resize(ref CK2, 5);

            return S63Error.SUCCESS;
        }
        /// <summary>
        /// Convert hexadecimal string to bytes array of 'len' length
        /// blank, '-' or '_' symbols are allowed and will not be taken into consideration
        /// </summary>
        /// <param name="input_str">string has to be converted</param>
        /// <param name="len">necessary bytes array length</param>
        /// <returns>
        /// null - if string is too short or includes not hexadecimal symbols
        /// bytes array in case of successfully convertion
        /// </returns>
        public static Byte[] ConvertHex2Byte(String input_str, int len)
        {
            try
            {
                String str = input_str;
                Char[] ar = new Char[] { ' ', '-', '_', '.' };
                Int32 idx = 0;
                while ((idx = str.IndexOfAny(ar, idx)) > -1)
                    str = str.Remove(idx, 1);

                if (str.Length < (len * 2))
                    return null;

                Byte[] buf = new Byte[len];

                for (Int32 i = 0, j = 0; i < len; j += 2)
                {
                    buf[i++] = Byte.Parse(str.Substring(j, 2), NumberStyles.AllowHexSpecifier);
                }
                return buf;
            }
            catch (SystemException ex)
            {
                throw ex;
            }
        }

        public static String CreateUserPermit(Byte[] HWID, string magic, string manufacturer)
	    {
            if (HWID.Length < 5)
                return null;

		    Byte[] hw_id = new Byte[8];
            Array.Copy(HWID, hw_id, 5);
		    hw_id[5] = hw_id[6] = hw_id[7] = 0x03;

		    ICSharpCode.SharpZipLib.Checksums.Crc32  crc = new ICSharpCode.SharpZipLib.Checksums.Crc32();

		    BFAlg bfenc = new BFAlg(Encoding.UTF8.GetBytes(magic));
		    if(!bfenc.Encrypt(hw_id))
			    return null;

		    String hexHW_ID = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", hw_id[0].ToString("X2"), hw_id[1].ToString("X2"),
			    hw_id[2].ToString("X2"), hw_id[3].ToString("X2"),hw_id[4].ToString("X2"), hw_id[5].ToString("X2"),
			    hw_id[6].ToString("X2"), hw_id[7].ToString("X2"));
		    Byte[] hexar = Encoding.UTF8.GetBytes(hexHW_ID);
		    crc.Update (hexar);
		    hexHW_ID += String.Format("{0}{1}", ((Int32)(crc.Value)).ToString("X8"), manufacturer);

		    return hexHW_ID;
	    }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public static S63Error CheckX509CrtGetPublicKey(string fname, ref DSACryptoServiceProvider pk)
        {
            try
            {
                if (!File.Exists(fname))
                    return S63Error.X509CertNotAval;
                X509Certificate2 cert = new X509Certificate2(fname);
                if (DateTime.Now > cert.NotAfter)
                    return S63Error.X509CrtExpired;

                if (cert.PublicKey == null || cert.PublicKey.Oid == null || 
                    cert.PublicKey.Oid.FriendlyName.ToLower() != "dsa" || cert.SignatureAlgorithm.FriendlyName.ToLower() != "sha1dsa")
                    return S63Error.PubKeyFmtErr;

                pk = (DSACryptoServiceProvider)cert.PublicKey.Key;

                return S63Error.SUCCESS;

            }
            catch (Exception ex)
            {
                return S63Error.PubKeyFmtErr;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="issuer"></param>
        /// <param name="PubKey"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public static S63Error CheckX509CrtGetInfo(string fname, ref string issuer, ref string PubKey, ref string expire)
        {
            try
            {
                if( !File.Exists(fname))
                    return S63Error.X509CertNotAval;
                
                X509Certificate2 cert = new X509Certificate2(fname);
                expire = cert.NotAfter.ToString();
                issuer = cert.Issuer;
                PubKey = "N/A";

                if (cert.PublicKey == null || cert.PublicKey.Oid == null ||
                    cert.PublicKey.Oid.FriendlyName.ToLower() != "dsa" || cert.SignatureAlgorithm.FriendlyName.ToLower() != "sha1dsa")
                    return S63Error.PubKeyFmtErr;

                PubKey = cert.GetPublicKeyString();

                return  (DateTime.Now > cert.NotAfter)? S63Error.X509CrtExpired : S63Error.SUCCESS;

            }
            catch(Exception )
            {
                return S63Error.PubKeyFmtErr;
            }
        }

        public static byte[] DecryptUnzipCell(byte[] Data, BFAlg bf)
        {
            try
            {
                byte[] zipped = bf.GetDecrypted(Data);
                ZipInputStream ZipIn = new ZipInputStream(new MemoryStream(zipped));
                ZipEntry entry = ZipIn.GetNextEntry();
                int pos = 0, len = 0;
                byte[] data = new byte[2048];
                while ((len = ZipIn.Read(data, pos, 2048)) == 2048)
                {
                    Array.Resize(ref data, data.Length + 2048);
                    pos += len;
                }
                if ((pos += len) < data.Length) Array.Resize(ref data, pos);
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get HW_ID from ENC UserPermit
        /// </summary>
        /// <param name="inUserPmt"> User permit string in S63 format</param>
        /// <returns>
        /// HW_ID in binary representation (6 bytes), first and last bytes are equal
        /// </returns>
        public static Byte[] GetHWIDFromUserPermit(String M_KEY, String inUserPmt)
	    {
		    Byte[] hw_id = null;
            try
            {
                if (inUserPmt.Length != 28)
                    throw new ApplicationException(String.Format("Incorrect format of UserPermit '{0}'", inUserPmt));

                Byte[] id = new Byte[2];
                id[0] = Byte.Parse(inUserPmt.Substring(24, 2), NumberStyles.AllowHexSpecifier);
                id[1] = Byte.Parse(inUserPmt.Substring(26, 2), NumberStyles.AllowHexSpecifier);
                String mid = Encoding.UTF8.GetString(id);
                String inHexHWID = inUserPmt.Substring(0, 16);

                ICSharpCode.SharpZipLib.Checksums.Crc32 crc = new ICSharpCode.SharpZipLib.Checksums.Crc32();
                crc.Update(Encoding.UTF8.GetBytes(inHexHWID));

                UInt32 upmtcrc = UInt32.Parse(inUserPmt.Substring(16, 8), NumberStyles.AllowHexSpecifier);
                if (upmtcrc != (UInt32)(crc.Value))
                    throw new ApplicationException("UserPermit CRC ERROR");

                hw_id = new Byte[8];
                for (int i = 0, j = 0; i < 8; j += 2)
                    hw_id[i++] = Byte.Parse(inHexHWID.Substring(j, 2), NumberStyles.AllowHexSpecifier);

                BFAlg bfenc = new BFAlg(Encoding.UTF8.GetBytes(M_KEY));
                if (!bfenc.Decrypt(hw_id))
                    throw new ApplicationException("UserPermit decryption FAILED");
                
                hw_id[5] = hw_id[0];
                Array.Resize(ref hw_id, 6);
            }
            catch (Exception ex)
            {
                hw_id = null;
                throw ex;
            }
            return hw_id;
	    }
    }

}
