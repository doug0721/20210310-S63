using System;
using System.Drawing;


namespace GeoFunctions
{
    public struct eGeoPoint
    {
        public double lat;
        public double lon;

        public static bool operator ==(eGeoPoint a, eGeoPoint b)
        {
            return (a.lat == b.lat && a.lon == b.lon);
        }
        public static bool operator !=(eGeoPoint a, eGeoPoint b)
        {
            return (a.lat != b.lat || a.lon != b.lon);
        }
    }
    public struct eGeoRect
    {
        public double nw_lat;
        public double nw_lon;
        public double se_lat;
        public double se_lon;

        public eGeoPoint nw
        {
            get
            {
                eGeoPoint ret = new eGeoPoint();
                ret.lat = nw_lat;
                ret.lon = nw_lon;
                return ret;
            }
            set
            {
                nw_lat = value.lat;
                nw_lon = value.lon;
            }
        }
        public eGeoPoint se
        {
            get
            {
                eGeoPoint ret = new eGeoPoint();
                ret.lat = se_lat;
                ret.lon = se_lon;
                return ret;
            }
            set
            {
                se_lat = value.lat;
                se_lon = value.lon;
            }
        }
    }
    public struct eWayPoint
    {
        public DateTime date;
        public double lat;
        public double lon;

        public static bool operator ==(eWayPoint a, eWayPoint b)
        {
            return (a.lat == b.lat && a.lon == b.lon && a.date.ToBinary() == b.date.ToBinary());
        }
        public static bool operator !=(eWayPoint a, eWayPoint b)
        {
            return (a.lat != b.lat || a.lon != b.lon || a.date.ToBinary() != b.date.ToBinary());
        }
    }
}
