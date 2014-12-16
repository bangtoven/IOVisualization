using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValtioClient
{
    public class Request
    {
        public UInt64 st_addr; // Starting address
        public UInt64 ed_addr; // Ending address (Starting address + data length - 1)
        public bool rw; // false: read, true: write
        public UInt64 lat; // Latency

        public Request(UInt64 st_addr, UInt64 ed_addr, bool rw, UInt64 lat)
        {
            this.st_addr = st_addr;
            this.ed_addr = ed_addr;
            this.rw = rw;
            this.lat = lat;
        }
    }

    public class TimeUnit
    {
        public int tu;
        public List<Request> time_unit;

        public TimeUnit()
        {
            this.tu = -1;
            this.time_unit = new List<Request>();
        }

        public TimeUnit(int tu)
        {
            this.tu = tu;
            this.time_unit = new List<Request>();
        }
    }

    public class ProcessInfo
    {
        public UInt32 pid;
        public List<TimeUnit> time_units;
        public int cur_index; // Used to keep track of the list index
        public int prev_unit; // Used to store previous time unit

        public ProcessInfo()
        {
            this.pid = 0;
            this.time_units = new List<TimeUnit>();
            this.cur_index = -1;
            this.prev_unit = -1;
        }

        public ProcessInfo(UInt32 pid)
        {
            this.pid = pid;
            this.time_units = new List<TimeUnit>();
            this.cur_index = -1;
            this.prev_unit = -1;
        }

        public void addRequest(Request req, int time_unit)
        {
            // Convert from seconds into time unit
            time_unit -= time_unit % GlobalPref.getTimeWindow();

            if (time_unit == this.prev_unit)
            {
                // Add request to the end of the current time unit
                this.time_units[cur_index].time_unit.Add(req);
            }
            else
            {
                // Make new TimeUnit with the given request and add it to the time unit list
                TimeUnit temp = new TimeUnit(time_unit);
                temp.time_unit.Add(req);
                this.time_units.Add(temp);
                this.prev_unit = time_unit; // Store current time unit
                this.cur_index++;
            }
        }
    }

    /* Stores global preference set by the user */
    public static class GlobalPref
    {
        // Debug
        public static bool debug = false;

        // Socket
        public static Socket m_ClientSocket;
        public static byte[] szData;
        public static byte[] szDataSum = new byte[9648]; //4096+48=4144
        public static int test = 0;
        public static int count = 0;

        // ProcessInfo
        public static ProcessInfo totalInfo = new ProcessInfo();
        public static Dictionary<UInt32, ProcessInfo> processInfos = new Dictionary<UInt32, ProcessInfo>();
        public static List<UInt32> pids = new List<UInt32>();
        public static Dictionary<UInt32, int> requestCount = new Dictionary<UInt32, int>();
        public static List<KeyValuePair<uint, int>> requestCountList;
        public static UInt64 minBlock = 0, maxBlock = 0;
        public static UInt64 maxLat = 0;

        private static String serverIP = null;
        private static String serverPort = null;
        private static String deviceID = null;
        private static int traceLength = 0; // Length of trace in seconds
        private static int timeWindow = 0;
        private static int blockUnit = 0;
        public static String timeElapsed = null;
        public static int timeElapsedInt = 0;
        public static int donePercent = 0;

        // Get & set serverIP
        public static void setServerIP(String t)
        {
            serverIP = t;
        }
        public static String getServerIP()
        {
            return serverIP;
        }
        
        // Get & set serverPort
        public static void setServerPort(String t)
        {
            serverPort = t;
        }
        public static String getServerPort()
        {
            return serverPort;
        }

        // Get & set deviceID
        public static void setDeviceID(String t)
        {
            deviceID = t;
        }
        public static String getDeviceID()
        {
            return deviceID;
        }

        // Get & set traceLength
        public static void setTraceLength(int t)
        {
            traceLength = t;
        }
        public static int getTraceLength()
        {
            return traceLength;
        }

        // Get & set timeWindow
        public static void setTimeWindow(int t)
        {
            timeWindow = t;
        }
        public static int getTimeWindow()
        {
            return timeWindow;
        }

        // Get & set blockUnit
        public static void setBlockUnit(int t)
        {
            blockUnit = t;
        }
        public static int getBlockUnit()
        {
            return blockUnit;
        }
    }
}
