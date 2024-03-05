using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using ENC.Params;

namespace AVCS.Kernel
{
    public class MediaInfo : HashSet<String>
    {
        public enum MType { Unknown = 0, DataSet = 1, Media = 2, AIODataSet = 3 };
        public enum CDType { Base = 0, Update = 1 };

        MType _MediaType;
        CDType _byDataSetType;
        Byte _bySetNum;
        Byte _byTotalSetNum;
        string _strDataServerID;
        int _iWeekNum;

        public MediaInfo()
        {
            _MediaType = MType.Unknown;
            _strDataServerID = "";
            _iWeekNum = 0;
            _byDataSetType = 0;
            _bySetNum = 0;
            _byTotalSetNum = 0;
        }
        public String DataServerID
        {
            get
            {
                return _strDataServerID;
            }
        }
        public MType MediaType
        {
            get
            {
                return _MediaType;
            }
        }
        public int WeekNum
        {
            get
            {
                return _iWeekNum;
            }
        }
        public CDType VolumeType
        {
            get
            {
                return _byDataSetType;
            }
        }
        public Byte CDNum
        {
            get
            {
                return _bySetNum;
            }
        }
        public Byte CDCount
        {
            get
            {
                return _byTotalSetNum;
            }
        }

        /// <summary>
        /// Read ENC media information from Media.txt or Serial.enc file if it is found under path folder
        /// </summary>
        /// <param name="path"> root path to search </param>
        /// <returns></returns>
        public Boolean Read(String path)
        {
            Clear();
            _MediaType = MType.Unknown;

            String fpath = path.ToUpper();
            if (!fpath.EndsWith("\\")) fpath += "\\";

            int idx = fpath.IndexOf("ENC_ROOT");
            String fname = (idx >= 0) ? fpath.Substring(0, idx) : fpath;

            try
            {
                if (!File.Exists(fname + "SERIAL.ENC"))
                {
                    if (!File.Exists(fname += "SERIAL.AIO"))
                    {
                        if (!File.Exists(fname = fpath + "MEDIA.TXT"))
                            return false;
                        _MediaType = MType.Media;
                    }
                    else
                        _MediaType = MType.AIODataSet;
                }
                else
                {
                    fname += "SERIAL.ENC";
                    _MediaType = MType.DataSet;
                }


                StreamReader sr = File.OpenText(fname);
                if ((idx = fname.LastIndexOf('\\')) > -1)
                    fname = fname.Substring(0, idx + 1);
                else fname = "";

                String str = sr.ReadLine();

                if (!String.IsNullOrEmpty(str) && str.Length > 30)
                {
                    _strDataServerID = str.Substring(0, 2);
                    _iWeekNum = (2000 + Int32.Parse(str.Substring(7, 2))) * 100 + Int32.Parse(str.Substring(4, 2));
                    _byDataSetType = (str.Substring(20, 4) == "BASE") ? CDType.Base : CDType.Update;
                    if (_MediaType == MType.Media)
                    {
                        _bySetNum = Byte.Parse(str.Substring(31, 2));
                        _byTotalSetNum = Byte.Parse(str.Substring(34, 2));
                        sr.ReadLine();
                        while (!String.IsNullOrEmpty(str = sr.ReadLine()))
                        {
                            str = str.Substring(0, str.IndexOf(','));
                            idx = str.IndexOf(';');
                            if (Byte.Parse(str.Substring(1, idx - 1)) == _bySetNum) //volumes at current media
                                Add(str.Substring(idx + 1));
                        }
                    }
                    else if (Int16.Parse(str.Substring(30, 2)) > 1)
                    {
                        _bySetNum = Byte.Parse(str.Substring(36, 2));
                        _byTotalSetNum = Byte.Parse(str.Substring(39, 2));
                    }
                    else if (fname != null && File.Exists(fname += "info\\CD_INFO.TXT"))
                    {
                        StreamReader cdr = File.OpenText(fname);
                        str = cdr.ReadLine();
                        String[] strar = str.Split('|');
                        _bySetNum = Byte.Parse(strar[0]);
                        _byTotalSetNum = Byte.Parse(strar[1]);
                    }
                    else
                        _bySetNum = _byTotalSetNum = 1;

                    sr.Close();
                    return true;
                }
                sr.Close();
            }
            catch (SystemException)
            {
                _MediaType = 0;
            }
            return false;
        }
    }

    public class S57DataInfo
    {
        public S57File.Status iStatus;
        public String strFname;
        public String strVolume;
        public String CRC;
        public String implem;
        public float nwLat;
        public float nwLon;
        public float seLat;
        public float seLon;
        public Int16 Edtn;
        public Int16 Updn;
        public String IssueDate;
        public String UpdDate;
        public S63Error Err;

        public S57DataInfo()
        {
            Clear();
        }

        public void Clear()
        {
            Edtn = -1;
            Updn = -1;
            iStatus = S57File.Status.IncorrectFormat;
            nwLat = 0;
            nwLon = 0;
            seLat = 0;
            seLon = 0;
            IssueDate = "";
            UpdDate = "";
            CRC = "";
            Err = S63Error.CellPmtNotFound;
        }

    };

    public class CellInstInfoList : LinkedList<S57DataInfo>
    {
        Permit _pmt;

        public Permit pmt { get { return _pmt; } }
        public CellInstInfoList(Permit pmt)
        {
            _pmt = pmt;
        }

    }
    /// <summary>
    /// provide access to S57 catalog.031 information
    /// </summary>
    public class S57File
    {
        BinaryReader _reader;
        String _Path;

        public enum Status : uint
        {   OK = 0,
            NotOpen = 0x01, 
            EndOfFile = 0x02,
            IncorrectFormat = 0x04,
            Encrypted = 0x08,
            FromRejected = 0x10 
        };

        public S57File(String fname)
        {
            try
            {
                _reader = new BinaryReader(File.OpenRead(fname));
                _Path = fname.Substring(0, fname.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            }
            catch (SystemException)
            {
                Close();
            }
        }
        public Boolean IsOpen() { return (_reader != null); }
        public void Close()
        {
            if (IsOpen())
            {
                _reader.Close();
                _reader = null;
            }
        }

        /// <summary>
        /// read one record from S57 catalog (catalog.031 file) 
        /// </summary>
        /// <param name="rec">S57CatalogRecord variable where information from record is saved </param>
        /// <returns> false if catalog file not opened or no more records to read</returns>
        public S57File.Status ReadRecord(String tagnm, ref S57DataInfo rec)
        {
            try
            {
                rec.Clear();

                if (_reader == null)
                    return Status.NotOpen;
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                nfi.NumberDecimalSeparator = ".";
                Encoding encoding = Encoding.ASCII;
                Byte[] dr = null;
                String str;
                bool start = tagnm == "DSID";
                S57Tag tagDSID = null;

                rec.iStatus = Status.IncorrectFormat;

                while ((dr = _reader.ReadBytes(24)) != null && dr.Length == 24)
                {
                    if (start && encoding.GetString(dr, 5, 7) != "3LE1 09")
                        return Status.Encrypted;
                    Int32 rec_len = Int32.Parse(encoding.GetString(dr, 0, 5)) - 24;
                    if (!start && dr[6] != 'D')
                    {
                        _reader.BaseStream.Seek(rec_len, SeekOrigin.Current);
                        continue;
                    }
                    Byte[] data = _reader.ReadBytes(rec_len);

                    Int32 fld_len_sz = dr[20] - 48, fld_pos_sz = dr[21] - (Byte)48, fld_tag_sz = Byte.Parse(encoding.GetString(dr, 22, 2));
                    Int32 fld_area_addr = Int32.Parse(encoding.GetString(dr, 12, 5)) - 24; //'24' is a size of DDR_LEADER - it has to be substracted from full record length

                    for (int i = 0; data[i] != 30; ) //'30' is Field terminator
                    {
                        String tag = encoding.GetString(data, i, fld_tag_sz); i += fld_tag_sz;
                        if (tag != tagnm)
                        {
                            i += (fld_len_sz + fld_pos_sz);
                            continue; //// unpack only defined records
                        }

                        Int32 fld_len = Int32.Parse(encoding.GetString(data, i, fld_len_sz)); i += fld_len_sz;
                        Int32 fld_pos = Int32.Parse(encoding.GetString(data, i, fld_pos_sz)); i += fld_pos_sz;

                        int k, shift = fld_area_addr + fld_pos;
                        if (tagnm == "CATD") //read records from catalog
                        {
                            shift += 12;
                            rec.iStatus = Status.OK;
                            for (k = 0; k < 9; k++)
                            {
                                int len = 0;
                                while (data[shift + len] != 31) len++;

                                switch (k)
                                {
                                    case 0: rec.strFname = _Path + encoding.GetString(data, shift, len).ToUpper(); rec.strFname = rec.strFname.Replace('\\', Path.DirectorySeparatorChar); break;
                                    case 3: rec.implem = encoding.GetString(data, shift, 3).ToUpper();
                                        rec.seLat = len > 3 ? float.Parse(encoding.GetString(data, shift + 3, len - 3), nfi) : 0f;
                                        break;
                                    case 4: rec.nwLon = len > 0 ? float.Parse(encoding.GetString(data, shift, len), nfi) : 0f; break;
                                    case 5: rec.nwLat = len > 0 ? float.Parse(encoding.GetString(data, shift, len), nfi) : 0f; break;
                                    case 6: rec.seLon = len > 0 ? float.Parse(encoding.GetString(data, shift, len), nfi) : 0f; break;
                                    case 7: rec.CRC = encoding.GetString(data, shift, len); break;
                                    case 8: str = encoding.GetString(data, shift, len).ToUpper();
                                        if (rec.implem == "BIN")
                                        {
                                            if (String.IsNullOrEmpty(str))
                                            {
                                                S57File cellfile = new S57File(_Path + rec.strFname);
                                                rec.iStatus = cellfile.ReadRecord("DSID", ref rec);
                                                cellfile.Close();
                                            }
                                            else
                                            {
                                                String[] infar = str.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                for (int n = 0; n < infar.Length; n++)
                                                {
                                                    str = infar[n].Substring(0, 4);
                                                    if (str == "EDTN") rec.Edtn = Int16.Parse(infar[n].Substring(5));
                                                    else if (str == "UPDN") rec.Updn = Int16.Parse(infar[n].Substring(5));
                                                    else if (str == "UADT") rec.UpdDate = infar[n].Substring(5, 8);
                                                    else if (str == "ISDT") rec.IssueDate = infar[n].Substring(5, 8);
                                                }
                                                rec.iStatus = (rec.Edtn != -1 && rec.Updn != -1) ? Status.Encrypted : Status.IncorrectFormat;
                                            }
                                        }
                                        break;
                                }
                                shift += ++len;
                            }
                            if (!rec.strFname.ToLower().Contains("catalog"))
                                return rec.iStatus == Status.IncorrectFormat ? Status.IncorrectFormat : Status.OK;
                        }
                        else if (tagnm == "DSID")
                        {
                            if (start)
                            {
                                tagDSID = new S57Tag(tagnm, data, shift, fld_len);
                                break;
                            }
                            else
                            {
                                int endpos = shift + fld_len;
                                foreach (var p in tagDSID)
                                {
                                    switch (p.nm)
                                    {
                                        case FldName.EDTN: rec.Edtn = Int16.Parse(p.GetString(data, shift, endpos, false)); break;  // edition number
                                        case FldName.UPDN: rec.Updn = Int16.Parse(p.GetString(data, shift, endpos, false)); break;  // last update number
                                        case FldName.UADT: rec.UpdDate = ENCChart.GetDate(p.GetString(data, shift, endpos, false));
                                            break;
                                        case FldName.ISDT: rec.IssueDate = ENCChart.GetDate(p.GetString(data, shift, endpos, false));
                                            break;
                                        case FldName.AGEN:     
                                            break;
                                    }
                                    if (p.bytesnum == 0) shift = Array.FindIndex(data, shift, d => d == 31) + 1;
                                    else shift += p.bytesnum;
                                    if (shift > endpos)
                                        break;
                                }
                                return (rec.Edtn != -1 && rec.Updn != -1) ? Status.OK : Status.IncorrectFormat;
                            }
                        }
                    }
                    start = false;
                }
                return Status.EndOfFile;
            }
            catch (Exception)
            {
                return Status.IncorrectFormat;
            }
        }
        /// <summary>
        /// reads S57 catalog (catalog.031) file and prepares sorted information about cells have to be installed
        /// </summary>
        /// <param name="filter">defines which cells have to be installed</param>
        /// <returns></returns>
        public bool FillInstallInfoMap(String filter, Dictionary<String, CellInstInfoList> infomap, Dictionary<string, Permit> pmts, ref int iNumToInstall)
        {
            if (_reader == null)
                return false;

            bool bEncFound = false;
            String filt;
            Boolean bAll = String.IsNullOrEmpty(filter);
            if (!bAll) filt = filter.ToUpper();

            int extind;
            S57DataInfo rec = new S57DataInfo();
            Status ires = 0;
            CellInstInfoList cellinflist = null;
            while ((ires = ReadRecord("CATD", ref rec)) != Status.EndOfFile)
            {
                if (!bAll && !rec.strFname.StartsWith(filter)) continue;
                if(!File.Exists(rec.strFname)) continue;
                    
                if (ires != Status.OK && (rec.Edtn == -1 || rec.Updn == -1))
                        continue;

                if (rec.implem == "ASC")  continue;

                String extent = rec.strFname.Substring((extind = rec.strFname.LastIndexOf('.')) + 1);
                S57DataInfo first = null;
                int indx = rec.strFname.LastIndexOf(Path.DirectorySeparatorChar);
                String chKey;
                if (indx++ >= 0)
                    chKey = (rec.implem == "BIN") ? rec.strFname.Substring(indx, 8) : rec.strFname.Substring(0, indx);
                else
                    chKey = (rec.implem == "BIN") ? rec.strFname.Substring(0, 8) : "";


                Permit pmt = null;
                if (rec.implem == "BIN")
                {
                    if (chKey.Length != 8 || !Char.IsDigit(chKey[2]))
                        continue;
                    if (infomap.TryGetValue(chKey, out cellinflist) && (first = cellinflist.First()) != null && first.Edtn == 0) // Cancelation Update
                        continue;

                    if (cellinflist == null)
                        pmts.TryGetValue(chKey, out pmt);
                    else pmt = cellinflist.pmt;

                    if (rec.iStatus == Status.Encrypted)
                    {
                        bEncFound = true;
                        if (pmt == null) continue;
                        rec.Err = pmt.Status;
                    }
                    else rec.Err = S63Error.SUCCESS;

                    if (rec.Edtn == 0 && cellinflist != null) // cancelation update or previous edition
                    {
                        iNumToInstall -= cellinflist.Count;
                        cellinflist.Clear();
                    }
                }
                else if (!infomap.TryGetValue(chKey, out cellinflist))
                    cellinflist = null;

                if(cellinflist == null)
    			    infomap.Add(chKey, (cellinflist = new CellInstInfoList(pmt)));

                if (rec.implem == "BIN")
                {
                    S57DataInfo fndinf = cellinflist.FirstOrDefault(val => (val.Edtn == rec.Edtn && val.Updn >= rec.Updn) || val.Edtn < rec.Edtn);
                    if (fndinf != null) cellinflist.AddBefore(cellinflist.Find(fndinf), rec);
                    else cellinflist.AddLast(rec);
                    iNumToInstall++;

                }
                else// all others files will be included in NOTES list
                    cellinflist.AddLast(rec);

                //only if everything was OK we create new record
                rec = new S57DataInfo();
            }
            return bEncFound;
        }
    }
}
