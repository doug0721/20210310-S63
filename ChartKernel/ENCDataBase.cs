using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Security.Cryptography;
using System.Xml.Linq;
using ENC.Params;
using System.Security.Principal;
using System.Security.AccessControl;
using System.ComponentModel;
using System.Reflection;

namespace AVCS.Kernel
{
    public class Permit
    {
        public S63Error Status = S63Error.CellPmtNotFound;	//if < 0 - error code, if 1 or 2 - pmt file format versions
        public int Edtn = 0;	//installed permit edition, = 0 if permit is not installed
        public DateTime Expire;
        public byte[] CK1 = null;
        public byte[] CK2 = null;
        public string pmtstr = null;

        public Permit(string pmtstr, string sedt, BFAlg bf)
        {
            DateTime tm = DateTime.Now;
            DateTime pmttm = new DateTime(int.Parse(pmtstr.Substring(8, 4)), int.Parse(pmtstr.Substring(12, 2)), int.Parse(pmtstr.Substring(14, 2)), 0, 0, 0);
            Status = S63Crypt.GetEncKeysFromPermit(pmtstr, bf, ref CK1, ref CK2);
            if (Status == (int)S63Error.SUCCESS)
            {
                if (pmttm < tm)
                    Status = S63Error.ParmitExpiredWarn;
                else if ((pmttm - tm).Days < 30)
                    Status = S63Error.PmtWillExpireWarn;
            }
            Expire = pmttm;
            if (!int.TryParse(sedt, out Edtn)) Edtn = 0;
        }
        public string GetStatusString()
        {
            if (Status == S63Error.SUCCESS)
                return ("Valid till " + string.Format("{0:D2}/{1:D2}/{2}",Expire.Day, Expire.Month, Expire.Year));
            if (Status == S63Error.CellPmtCRCErr)
                return "Invalide for HWID";
            if (Status == S63Error.CellPmtExpired || Status == S63Error.ParmitExpiredWarn)
                return "Expired";
            if (Status == S63Error.PmtWillExpireWarn)
                return "Will expire in one month";
            return "Invalid";

        }

    };

    public class ENCCollection 
    {
        public static S63Error AddPermit(string nm, string pmtstr, Dictionary<string, Permit> pmts, BFAlg bf)
        {
            string[] ss = pmtstr.Split(',');
            if (ss.Length == 0 || ss[0].Length < 64) return S63Error.CellPmtFmtErr;
            if (ss.Length > 3 && !String.IsNullOrEmpty(ss[3]))
            {
                if (ss[3].Length != 2) return S63Error.CellPmtFmtErr;
            }
            Permit pmt = new Permit(ss[0], ss.Length > 2 ? ss[2] : "", bf);
            if (pmt.Status < 0) return pmt.Status;
            pmts[nm] = pmt;

            return pmt.Status;
        }

        public static S63Error CheckPermit(string permit, Dictionary<string, Permit> pmts)
        {
            BFAlg bf = new BFAlg(S63Crypt._HWID);
            S63Error res = S63Error.SUCCESS;  

            string name = permit.Substring(0, 8);
            res = AddPermit(name, permit, pmts, bf);
            return res;
        }

        public static bool InstallCharts(StringWriter sw, string SrcDir, Dictionary<string, Permit> pmts)
        {
            sw.WriteLine(string.Format("Process start. Cell {0} is selected.", SrcDir));
            S63Error result = S63Error.SUCCESS;
            int iNumToInstall = 0;

            try
            {
                DateTime tm = DateTime.Now;
                Dictionary<String, CellInstInfoList> InstInfoMap = new Dictionary<string, CellInstInfoList>();
                string chKey;
                string chSourcePath = "";
                string chFilter = ""; 
                string str = "";
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

                HashSet<string> MissingMediaList = new HashSet<string>();
                Dictionary<string, string> MediaList = new Dictionary<string, string>();
                bool bEncFound = false;

                chSourcePath = Path.GetDirectoryName(Path.GetDirectoryName(SrcDir));
                if (!Directory.Exists(chSourcePath))
                {
                    sw.WriteLine(string.Format("Catalog.031 is not found in folder " + SrcDir));
                    return false;
                }

                S57File cat = null;
                if (Directory.Exists(chSourcePath))
                {
                    cat = new S57File(Path.Combine(chSourcePath,  "catalog.031"));
                }

                if (cat != null && cat.IsOpen())
                {
                    if (cat.FillInstallInfoMap(chFilter, InstInfoMap, pmts, ref iNumToInstall))
                        bEncFound = true;
                    cat.Close();
                }

                if (iNumToInstall == 0 && bEncFound && pmts.Count == 0)
                {
                    result = S63Error.PmtsNotAval;
                    sw.WriteLine("Error. Permits are not available.");
                    return false;
                }

                DateTime sys_time = DateTime.Now;
                try
                {
                    foreach (var val in InstInfoMap)
                    {
                        if (val.Key.Length != 8) continue;//other lists including NOTES and will be used during cells installation

                        chKey = val.Key;
                        foreach (var inf in val.Value)
                        {
                            String fname = inf.strFname.Substring(inf.strFname.LastIndexOf('\\') + 1);
                            String ext = Path.GetExtension(fname);
                            Boolean bUpdate = (ext != ".000");
                            try
                            {
                                if (inf.Err != S63Error.CellPmtExpired && (int)inf.Err < 0)
                                    throw new ApplicationException();

                                if (inf.iStatus == S57File.Status.Encrypted && inf.Edtn > 0)
                                {
                                    DateTime cellissue = new DateTime(int.Parse(inf.IssueDate.Substring(0, 4)), int.Parse(inf.IssueDate.Substring(4, 2)), int.Parse(inf.IssueDate.Substring(6, 2)));
                                    if (cellissue > val.Value.pmt.Expire)
                                    {
                                        inf.Err = S63Error.CellPmtExpired;
                                        sw.WriteLine("Error. Permit has expired.");
                                        throw new ApplicationException();
                                    }
                                }
                                sw.WriteLine(String.Format("{0} (Ed. {1}, Upd {2}) processing...", fname, inf.Edtn, inf.Updn ));

                                //read data from file and later work with data in memory
                                byte[] data = File.ReadAllBytes(inf.strFname);

                                if (inf.Err < 0)
                                    throw new ApplicationException();

                                byte[] buf = null;
                                BFAlg bf = null;
                                if ((inf.iStatus & S57File.Status.Encrypted) > 0 && (buf = S63Crypt.DecryptUnzipCell(data, (bf = new BFAlg(val.Value.pmt.CK1)))) == null &&
                                        (buf = S63Crypt.DecryptUnzipCell(data, (bf = new BFAlg(val.Value.pmt.CK2)))) == null)
                                    inf.Err = S63Error.UnzipCellErr;
                                else
                                {
                                    string Filename = inf.strFname.Replace(".", "__");
                                    if (buf != null && !string.IsNullOrEmpty(Filename))
                                    {
                                        string dir = Path.GetDirectoryName(Filename);
                                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                        File.WriteAllBytes(Filename, buf);//here the decrypted cell is saved in file. This is not allowed!
                                        sw.WriteLine(String.Format("Decrypted cell {0} is saved in {1}.", fname, Filename));
                                    }
                                }

                                if (inf.Err < 0)
                                    throw new ApplicationException();

                                if (val.Value.pmt != null && (val.Value.pmt.Expire - sys_time).Days < 30)
                                    inf.Err = S63Error.PmtWillExpireWarn;
                            }
                            catch (ApplicationException)
                            {
                                sw.WriteLine("InstallCharts: ApplicationException.");
                            }
                            catch (Exception ex)
                            {
                                inf.Err = S63Error.Exception;
                                sw.WriteLine(String.Format("{0} installation exception: {1}.", inf.strFname, ex.Message));
                            }
                            finally
                            {
                                str = inf.Err.DisplayString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sw.WriteLine(String.Format("Exception: {0}.", ex.Message));
                }
                finally
                {
                    if (result >= 0 && MissingMediaList.Count > 0)
                    {
                        string s = "", ss = "";
                        foreach (string m in MissingMediaList)
                        {
                            if (MediaList.TryGetValue(m, out s))
                            {
                                if (!String.IsNullOrEmpty(ss)) ss += ",";
                                ss += s;
                            }
                        }
                        if (!String.IsNullOrEmpty(ss))
                            sw.WriteLine(string.Format("The Update Media is not compatible with currently installed ENCs. Please install {0} and then continue with the update process", ss));
                    }
                }
            }
            catch (Exception ex)
            {
                result = S63Error.Exception;
                sw.WriteLine(ex.Message);
            }
            return true;
        }

    }

    public static class Format
    {
        public static string DisplayString(this Enum value)
        {
            //Using reflection to get the field info
            FieldInfo info = value.GetType().GetField(value.ToString());
            if (info == null) return "";

            //Get the Description Attributes
            DescriptionAttribute[] attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //Only capture the description attribute if it is a concrete result (i.e. 1 entry)
            if (attributes.Length == 1)
            {
                return attributes[0].Description;
            }
            else //Use the value for display if not concrete result
            {
                return value.ToString();
            }
        }
    }
}
