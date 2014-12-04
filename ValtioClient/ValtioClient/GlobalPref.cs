using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValtioClient
{
    /* Stores global preference set by the user */
    public static class GlobalPref
    {
        private static bool debug = false; // Flag used for debugging
        private static String serverIP = null;
        private static String serverPort = null;
        private static String deviceID = null;
        private static int traceLength = 0; // Length of trace in seconds
        private static int timeWindow = 0;
        private static int blockUnit = 0;
        public static String timeElapsed = null;
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
