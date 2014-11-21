using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using valtio_client_getData;

namespace valtio_client_getData
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

    public static class receiveData
    {
        
        public static int BLK_STRUCT_SIZE = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BlkIOTrace));
        public static Dictionary<UInt64, UInt64> readQueue = new Dictionary<UInt64, UInt64>();
        public static Dictionary<UInt64, UInt64> writeQueue = new Dictionary<UInt64, UInt64>();
        public static int endIndex = 0;

        public static void getData(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = (Socket)sender;
            connectServer.szData = e.Buffer;
            byte[] temp = new byte[48];

            Buffer.BlockCopy(connectServer.szData, 0, connectServer.szDataSum, endIndex, e.BytesTransferred);

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
                    t.magic = BitConverter.ToUInt32(connectServer.szData, i * 48);
                    t.sequence = BitConverter.ToUInt32(connectServer.szData, 4 + i * 48);
                    t.time = BitConverter.ToUInt64(connectServer.szData, 8 + i * 48);
                    t.sector = BitConverter.ToUInt64(connectServer.szData, 16 + i * 48);
                    t.bytes = BitConverter.ToUInt32(connectServer.szData, 24 + i * 48);
                    t.action = BitConverter.ToUInt32(connectServer.szData, 28 + i * 48);
                    t.pid = BitConverter.ToUInt32(connectServer.szData, 32 + i * 48);
                    t.device = BitConverter.ToUInt32(connectServer.szData, 36 + i * 48);
                    t.cpu = BitConverter.ToUInt32(connectServer.szData, 40 + i * 48);
                    t.error = BitConverter.ToUInt16(connectServer.szData, 44 + i * 48);
                    t.pdu_len = BitConverter.ToUInt16(connectServer.szData, 46 + i * 48);

                    i++;
                    
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


                    //Console.WriteLine(rwbs);

                    if (t.action == 0) //action 0 들어온 경우
                    {
                        offset = offset + BLK_STRUCT_SIZE;
                        continue;
                    }

                    string rwbsString = new String(rwbs);

                    converted_action = t.action & 0xFFFF;


                    /*
                    if (rwbsString.Contains('R')) //read
                    {

                        if (converted_action == (UInt32)blkTA.__BLK_TA_ISSUE) // issue-read
                        {
                            if (readQueue.ContainsKey(t.sector))
                            {
                                Console.WriteLine("같은 sector 에 대한 READ");
                                readQueue.Remove(t.sector);
                            }

                            readQueue.Add(t.sector, t.time);
                            //Console.WriteLine(t.sector + " " + rwbsString + " READ ISSUE");
                        }

                        if (converted_action == (UInt32)blkTA.__BLK_TA_COMPLETE) // read-complete
                        {
                            if (readQueue.ContainsKey(t.sector)) //issue 된 section 에 대해 complete 됨.
                            {
                                elapsed = t.time - readQueue[t.sector];
                                readQueue.Remove(t.sector);
                                Console.WriteLine("Time : " + elapsed);
                            }
                            else
                            {
                                Console.WriteLine("Issue 안됐는데 READ - Complete 함.");
                            }

                            //Console.WriteLine(t.sector + " " + rwbsString + " WRITE ISSUE");
                        }


                    }

                    else if (rwbsString.Contains('W')) //write
                    {
                        if (converted_action == (UInt32)blkTA.__BLK_TA_ISSUE) // issue write
                        {
                            if (writeQueue.ContainsKey(t.sector))
                            {
                                Console.WriteLine("같은 sector 에 대한 WRITE");
                                writeQueue.Remove(t.sector);
                            }

                            writeQueue.Add(t.sector, t.time);

                            //Console.WriteLine(t.sector + " " + rwbsString + " READ COMPLETE");
                        }

                        if (converted_action == (UInt32)blkTA.__BLK_TA_COMPLETE) // complete-write
                        {
                            if (writeQueue.ContainsKey(t.sector))
                            {
                                elapsed = t.time - writeQueue[t.sector];
                                writeQueue.Remove(t.sector);
                                Console.WriteLine("Time : " + elapsed);
                            }
                            else
                            {
                                Console.WriteLine("Issue 안됐는데 WRITE - Complete 함.");
                            }

                            //Console.WriteLine(t.sector + " " + rwbsString + " WRITE COMPLETE");
                        }

                    }

                    else
                    {
                        Console.WriteLine("STRANGE@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ " + rwbsString + " " + converted_action);
                        return;
                    }

                    */


                    /*
                    if (converted_action == (UInt32)blkTA.__BLK_TA_ISSUE)
                        Console.WriteLine(rwbsString + " issue");
                    else if (converted_action == (UInt32)blkTA.__BLK_TA_COMPLETE)
                        Console.WriteLine(rwbsString + " complete");
                    else
                        Console.WriteLine("Strange!!!" + rwbsString);
                    */



                    
                    if (converted_action == (UInt32)blkTA.__BLK_TA_ISSUE) //issue
                    {
                        //Console.WriteLine(t.sector + " " + rwbsString + " READ ISSUE");
                        if (rwbsString.Contains('R')) // issue-read
                        {
                            if (readQueue.ContainsKey(t.sector))
                            {
                                Console.WriteLine("@@@@@@@@@@@@같은 sector 에 대한 READ");
                                //readQueue.Remove(t.sector);
                            }

                            readQueue.Add(t.sector, t.time);
                            
                        }

                        if (rwbsString.Contains('W')) // issue-write
                        {
                            //Console.WriteLine(t.sector + " " + rwbsString + " WRITE ISSUE");
                            if (writeQueue.ContainsKey(t.sector))
                            {
                                Console.WriteLine("@@@@@@@@@@@@@같은 sector 에 대한 WRITE");
                                writeQueue.Remove(t.sector);
                            }

                            writeQueue.Add(t.sector, t.time);

                            
                        }


                    }

                    else if (converted_action == (UInt32)blkTA.__BLK_TA_COMPLETE) //complete
                    {
                        if (rwbsString.Contains('R')) // complete-read
                        {
                            //Console.WriteLine(t.sector + " " + rwbsString + " READ COMPLETE");
                            if (readQueue.ContainsKey(t.sector)) //issue 된 section 에 대해 complete 됨.
                            {
                                elapsed = t.time - readQueue[t.sector];
                                readQueue.Remove(t.sector);
                                Console.WriteLine("Time : " + elapsed);
                            }
                            else
                            {
                                Console.WriteLine("@@@@@@@@@@@@@@@Issue 안됐는데 READ - Complete 함.");
                            }

                  
                        }

                        if (rwbsString.Contains('W')) // complete-write
                        {
                            //Console.WriteLine(t.sector + " " + rwbsString + " WRITE COMPLETE");

                            if (writeQueue.ContainsKey(t.sector))
                            {
                                elapsed = t.time - writeQueue[t.sector];
                                writeQueue.Remove(t.sector);
                                Console.WriteLine("Time : " + elapsed);
                            }
                            else
                            {
                                Console.WriteLine("@@@@@@@@@@@@@@@@@@@Issue 안됐는데 WRITE - Complete 함.");
                            }  
                        }

                    }

                    else
                    {
                        Console.WriteLine("STRANGE@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ " + rwbsString + " " + converted_action);
                        return;
                    }
                    
                    
                    //Console.WriteLine("seq " + t.sequence);
                    offset = offset + BLK_STRUCT_SIZE;

                }

                endIndex = buflength % BLK_STRUCT_SIZE;

                Buffer.BlockCopy(connectServer.szDataSum, offset, temp, 0, endIndex); //총 array의 offset 만큼 뒷부분을 temp 에다가 복사해둔다
                connectServer.szDataSum = new byte[connectServer.szDataSum.Length]; //초기화
                Buffer.BlockCopy(temp, 0, connectServer.szDataSum, 0, endIndex); //다시 넣는다. (szDataSum 에서 endIndex 이후의 뒷부분을 깨끗하게 하기 위함)
                //test--;
                //MessageBox.Show("hi"+szData.ToString());
                //MessageBox.Show(e.BytesTransferred.ToString());
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                connectServer.szData = new byte[10000];
                args.SetBuffer(connectServer.szData, 0, 9600);
                args.UserToken = connectServer.m_ClientSocket;
                args.Completed
                    += new EventHandler<SocketAsyncEventArgs>(getData);
                connectServer.m_ClientSocket.ReceiveAsync(args);

            }
            else
            {
                //Console.WriteLine("else 문" + e.BytesTransferred.ToString());
                Console.WriteLine("Connection ended! Press Enter to quit");
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                //m_ClientSocket.Remove(ClientSocket);
            }
        }
    }
}
