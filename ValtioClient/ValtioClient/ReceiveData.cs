using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace ValtioClient
{
    struct BlkIOTrace
    {
        public UInt32 magic; //4+4+8+8+4+4+4+4+4+2+2 = 48bytes
        public UInt32 sequence;
        public UInt64 time;
        public UInt64 sector;
        public UInt32 bytes;
        public UInt32 action;
        public UInt32 pid;
        public UInt32 device;
        public UInt32 cpu;
        public UInt16 error;
        public UInt16 pdu_len;
    };

    enum blkTA
    {
        __BLK_TA_QUEUE = 1,		/* queued */
        __BLK_TA_BACKMERGE,		/* back merged to existing rq */
        __BLK_TA_FRONTMERGE,		/* front merge to existing rq */
        __BLK_TA_GETRQ,			/* allocated new request */
        __BLK_TA_SLEEPRQ,		/* sleeping on rq allocation */
        __BLK_TA_REQUEUE,		/* request requeued */
        __BLK_TA_ISSUE,			/* sent to driver */
        __BLK_TA_COMPLETE,		/* completed by driver */
        __BLK_TA_PLUG,			/* queue was plugged */
        __BLK_TA_UNPLUG_IO,		/* queue was unplugged by io */
        __BLK_TA_UNPLUG_TIMER,		/* queue was unplugged by timer */
        __BLK_TA_INSERT,		/* insert request */
        __BLK_TA_SPLIT,			/* bio was split */
        __BLK_TA_BOUNCE,		/* bio was bounced */
        __BLK_TA_REMAP,			/* bio was remapped */
        __BLK_TA_ABORT,			/* request aborted */
        __BLK_TA_DRV_DATA,		/* binary driver data */
    };

    enum blkTC
    {
        BLK_TC_READ = 1 << 0,	/* reads */
        BLK_TC_WRITE = 1 << 1,	/* writes */
        BLK_TC_FLUSH = 1 << 2,	/* flush */
        BLK_TC_SYNC = 1 << 3,	/* sync */
        BLK_TC_QUEUE = 1 << 4,	/* queueing/merging */
        BLK_TC_REQUEUE = 1 << 5,	/* requeueing */
        BLK_TC_ISSUE = 1 << 6,	/* issue */
        BLK_TC_COMPLETE = 1 << 7,	/* completions */
        BLK_TC_FS = 1 << 8,	/* fs requests */
        BLK_TC_PC = 1 << 9,	/* pc requests */
        BLK_TC_NOTIFY = 1 << 10,	/* special message */
        BLK_TC_AHEAD = 1 << 11,	/* readahead */
        BLK_TC_META = 1 << 12,	/* metadata */
        BLK_TC_DISCARD = 1 << 13,	/* discard requests */
        BLK_TC_DRV_DATA = 1 << 14,	/* binary driver data */
        BLK_TC_FUA = 1 << 15,	/* fua requests */

        BLK_TC_END = 1 << 15,	/* we've run out of bits! */
    };

    public class ReceiveData
    {

        public int BLK_STRUCT_SIZE = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BlkIOTrace));
        public Dictionary<UInt64, UInt64> readQueue = new Dictionary<UInt64, UInt64>();
        public Dictionary<UInt64, UInt64> writeQueue = new Dictionary<UInt64, UInt64>();
        public Dictionary<UInt64, UInt32> checkQueue = new Dictionary<UInt64, UInt32>();
        public int endIndex = 0;
        public int test = 0;
        public UInt64 startTime = 0;
        public int startTimeInt = 0;
        public TracingIcon tracingIcon;

        public void getData(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = (Socket)sender;
            GlobalPref.szData = e.Buffer;
            byte[] temp = new byte[48];

            Buffer.BlockCopy(GlobalPref.szData, 0, GlobalPref.szDataSum, endIndex, e.BytesTransferred);

            if (ClientSocket.Connected && e.BytesTransferred > 0)
            {

                int buflength = e.BytesTransferred + endIndex;
                int offset = 0;
                int i = 0;


                UInt32 converted_action;
                UInt64 elapsed = 0;

                while (buflength - offset >= BLK_STRUCT_SIZE)
                {
                    BlkIOTrace t = new BlkIOTrace();
                    t.magic = BitConverter.ToUInt32(GlobalPref.szData, i * 48);
                    t.sequence = BitConverter.ToUInt32(GlobalPref.szData, 4 + i * 48);
                    t.time = BitConverter.ToUInt64(GlobalPref.szData, 8 + i * 48);
                    t.sector = BitConverter.ToUInt64(GlobalPref.szData, 16 + i * 48);
                    t.bytes = BitConverter.ToUInt32(GlobalPref.szData, 24 + i * 48);
                    t.action = BitConverter.ToUInt32(GlobalPref.szData, 28 + i * 48);
                    t.pid = BitConverter.ToUInt32(GlobalPref.szData, 32 + i * 48);
                    t.device = BitConverter.ToUInt32(GlobalPref.szData, 36 + i * 48);
                    t.cpu = BitConverter.ToUInt32(GlobalPref.szData, 40 + i * 48);
                    t.error = BitConverter.ToUInt16(GlobalPref.szData, 44 + i * 48);
                    t.pdu_len = BitConverter.ToUInt16(GlobalPref.szData, 46 + i * 48);

                    i++;

                    if (startTime == 0)
                    {
                        startTimeInt = GlobalPref.timeElapsedInt;
                        startTime = t.time;
                    }

                    UInt32 w = t.action & (((UInt32)blkTC.BLK_TC_WRITE) << 16);
                    UInt32 a = t.action & (((UInt32)blkTC.BLK_TC_AHEAD) << 16);
                    UInt32 s = t.action & (((UInt32)blkTC.BLK_TC_SYNC) << 16);
                    UInt32 m = t.action & (((UInt32)blkTC.BLK_TC_META) << 16);
                    UInt32 d = t.action & (((UInt32)blkTC.BLK_TC_DISCARD) << 16);
                    UInt32 f = t.action & (((UInt32)blkTC.BLK_TC_FLUSH) << 16);
                    UInt32 u = t.action & (((UInt32)blkTC.BLK_TC_FUA) << 16);

                    char[] rwbs = new char[7];
                    int k = 0;
                    if (f > 0)
                        rwbs[k++] = 'F'; // flush 

                    if (d > 0)
                        rwbs[k++] = 'D';
                    else if (w > 0)
                        rwbs[k++] = 'W';
                    else if (t.bytes > 0)
                        rwbs[k++] = 'R';
                    else
                        rwbs[k++] = 'N';

                    if (u > 0)
                        rwbs[k++] = 'F'; // fua
                    if (a > 0)
                        rwbs[k++] = 'A';
                    if (s > 0)
                        rwbs[k++] = 'S';
                    if (m > 0)
                        rwbs[k++] = 'M';

                    rwbs[k] = '\0';

                    string rwbsString = new String(rwbs);

                    converted_action = t.action & 0xFFFF;

                    //문제점. queue 가 꽤 많이 남는다. linux 테스트 시 time 은 문제 없음. centos time 은 valtio에서 찍히는 것 부터 좀 이상. queue 남는건 issue 에서만 하면 백몇개 남고, queue 로해서 제대로 하면 천단위로 남음.
                    string savePath = @"c:\test.txt";
                    string textValue = "";
                    switch (converted_action)
                    {
                        case (UInt32)blkTA.__BLK_TA_QUEUE:
                            if (readQueue.ContainsKey(t.sector)) //find_track()
                            {
                                readQueue[t.sector] = t.time;
                            }
                            else
                            {
                                readQueue.Add(t.sector, t.time);
                            }
                            break;
                        case (UInt32)blkTA.__BLK_TA_FRONTMERGE:
                            UInt64 target;
                            target = t.sector + (t.bytes >> 9);
                            if (readQueue.ContainsKey(target))
                            {
                                UInt64 temptime = readQueue[target];
                                readQueue.Remove(target);
                                target -= (t.bytes >> 9);
                                try
                                {
                                    readQueue.Add(target, temptime);
                                }
                                catch (Exception)
                                {

                                }
                            }
                            break;
                        case (UInt32)blkTA.__BLK_TA_ISSUE:
                            if ((t.action & ((UInt32)blkTC.BLK_TC_FS << 16)) == 0)
                            {
                                break;
                            }
                            if (readQueue.ContainsKey(t.sector))
                            {
                                readQueue[t.sector] = t.time;
                            }
                            break;
                        case (UInt32)blkTA.__BLK_TA_COMPLETE:
                            if (readQueue.ContainsKey(t.sector))
                            {
                                elapsed = t.time - readQueue[t.sector];

                                Console.WriteLine("time is : " + elapsed);

                                bool rwTemp;
                                if (rwbsString.Contains('R'))
                                    rwTemp = false;
                                else
                                    rwTemp = true;
                                
                                /* Find out min, max block & max latency */
                                if (GlobalPref.minBlock == 0 && GlobalPref.maxBlock == 0)
                                {
                                    GlobalPref.minBlock = t.sector;
                                    GlobalPref.maxBlock = t.sector + t.bytes;
                                }
                                else
                                {
                                    if (t.sector < GlobalPref.minBlock)
                                    {
                                        GlobalPref.minBlock = t.sector;
                                    }
                                    if (t.sector + t.bytes > GlobalPref.maxBlock)
                                    {
                                        GlobalPref.maxBlock = t.sector + t.bytes;
                                    }
                                }

                                if (elapsed > GlobalPref.maxLat)
                                {
                                    GlobalPref.maxLat = elapsed;
                                }

                                /*****************ADD REQUEST***************/
                                Request tempRequest = new Request(t.sector, t.sector + t.bytes, rwTemp, elapsed);
                                int time_unit = (int)((t.time - startTime) / 1000000000) + startTimeInt;

                                GlobalPref.totalInfo.addRequest(tempRequest, time_unit);

                                ProcessInfo curInfo;
                                if (GlobalPref.processInfos.TryGetValue(t.pid, out curInfo))
                                {
                                    curInfo.addRequest(tempRequest, time_unit);
                                }
                                else
                                {
                                    ProcessInfo tempInfo = new ProcessInfo(t.pid);
                                    tempInfo.addRequest(tempRequest, time_unit);
                                    GlobalPref.processInfos.Add(t.pid, tempInfo);
                                    GlobalPref.pids.Add(t.pid);
                                }
                                /*****************ADD REQUEST***************/

                                Console.WriteLine(time_unit);

                                if (elapsed > 1000000000)
                                {
                                    textValue = "time is : " + elapsed + " t.time is : " + t.time + " readQueue[t.sector] is : " + readQueue[t.sector];
                                    System.IO.File.AppendAllText(savePath, textValue + Environment.NewLine, Encoding.Default);
                                }
                                readQueue.Remove(t.sector);
                            }
                            break;
                        default:
                            break;
                    }

                    if (test > 1000)
                    {
                        readQueue.ToList().Where(pair => pair.Value < (t.time - 10000000000)).ToList().ForEach(pair => readQueue.Remove(pair.Key));
                        test = 0;
                    }

                    test++;
                    offset = offset + BLK_STRUCT_SIZE;

                }

                endIndex = buflength % BLK_STRUCT_SIZE;

                Buffer.BlockCopy(GlobalPref.szDataSum, offset, temp, 0, endIndex); //총 array의 offset 만큼 뒷부분을 temp 에다가 복사해둔다
                GlobalPref.szDataSum = new byte[GlobalPref.szDataSum.Length]; //초기화
                Buffer.BlockCopy(temp, 0, GlobalPref.szDataSum, 0, endIndex); //다시 넣는다. (szDataSum 에서 endIndex 이후의 뒷부분을 깨끗하게 하기 위함)

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                GlobalPref.szData = new byte[10000];
                args.SetBuffer(GlobalPref.szData, 0, 9600);
                args.UserToken = GlobalPref.m_ClientSocket;
                args.Completed
                    += new EventHandler<SocketAsyncEventArgs>(getData);
                GlobalPref.m_ClientSocket.ReceiveAsync(args);

            }
            else
            {
                Application.Current.Dispatcher.Invoke((Action)delegate() { tracingIcon.StopTrace(); }, null);

                Console.WriteLine("Connection ended!");
                Console.WriteLine(readQueue.Count);
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
            }
        }
    }
}
