using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;
using ThreadState = System.Threading.ThreadState;

namespace iSpyApplication
{
    public class RemoteCommandEventArgs : EventArgs
    {
        public string Command;
        public int ObjectId;
        public int ObjectTypeId;

        // Constructor
        public RemoteCommandEventArgs(string command, int objectid, int objecttypeid)
        {
            Command = command;
            ObjectId = objectid;
            ObjectTypeId = objecttypeid;
        }
    }

    public class LocalServer
    {
        private static readonly List<Socket> MySockets = new List<Socket>();
        private static int _socketindex;
        private readonly MainForm _parent;
        public string ServerRoot;
        private Hashtable _mimetypes;
        private TcpListener _myListener;
        public int NumErr;
        private Thread _th;

        //The constructor which make the TcpListener start listening on the
        //given port. It also calls a Thread on the method StartListen(). 
        public LocalServer(MainForm parent)
        {
            _parent = parent;
        }

        public Hashtable MimeTypes
        {
            get
            {
                if (_mimetypes == null)
                {
                    _mimetypes = new Hashtable();
                    var sr = new StreamReader(ServerRoot + @"data\mime.Dat");
                    string sLine;
                    while ((sLine = sr.ReadLine()) != null)
                    {
                        sLine.Trim();

                        if (sLine.Length > 0)
                        {
                            //find the separator
                            int iStartPos = sLine.IndexOf(";");

                            // Convert to lower case
                            sLine = sLine.ToLower();

                            string sMimeExt = sLine.Substring(0, iStartPos);
                            string sMimeType = sLine.Substring(iStartPos + 1);
                            _mimetypes.Add(sMimeExt, sMimeType);
                        }
                    }
                    sr.Dispose();
                }
                return _mimetypes;
            }
        }

        public bool Running
        {
            get
            {
                if (_th == null)
                    return false;
                return _th.IsAlive;
            }
        }

        public string StartServer()
        {
            string message = "";
            try
            {
                if (MainForm.Conf.IPMode=="IPv6")
                {
                    _myListener = new TcpListener(IPAddress.IPv6Any, MainForm.Conf.LANPort)
                                      {ExclusiveAddressUse = true};
                     _myListener.AllowNatTraversal(true);
                }
                else
                {
                    _myListener = new TcpListener(IPAddress.Any, MainForm.Conf.LANPort)
                                      {ExclusiveAddressUse = true};
                }
                
                _myListener.Start(200);
                NumErr = 0;
                //start the thread which calls the method 'StartListen'
                if (_th != null)
                {
                    while (_th.ThreadState == ThreadState.AbortRequested)
                    {
                        Application.DoEvents();
                    }
                }
                _th = new Thread(StartListen);
                _th.Start();
            }
            catch (Exception e)
            {
                message = e.Message;
                MainForm.LogExceptionToFile(e);
            }
            return message;
        }

        public void StopServer()
        {
            foreach (Socket mySocket in MySockets)
            {
                if (mySocket != null)
                {
                    try
                    {
                        mySocket.Shutdown(SocketShutdown.Both);
                        mySocket.Close();
                    }
                    catch
                    {
                    }
                }
            }
            Application.DoEvents();
            if (_myListener != null)
            {
                try
                {
                    _myListener.Stop();
                    _myListener = null;
                }
                catch
                {
                }
            }
            Application.DoEvents();
            if (_th != null)
            {
                try
                {
                    if (_th.ThreadState == ThreadState.Running)
                        _th.Abort();
                }
                catch
                {
                }
                Application.DoEvents();
                _th = null;
            }
        }

        /// <summary>
        /// This function takes FileName as Input and returns the mime type..
        /// </summary>
        /// <param name="sRequestedFile">To indentify the Mime Type</param>
        /// <returns>Mime Type</returns>
        public string GetMimeType(string sRequestedFile)
        {
            if (sRequestedFile == "")
                return "";
            String sMimeType = "";

            // Convert to lowercase
            sRequestedFile = sRequestedFile.ToLower();

            int iStartPos = sRequestedFile.LastIndexOf(".");
            if (iStartPos == -1)
                return "text/javascript";
            string sFileExt = sRequestedFile.Substring(iStartPos);

            try
            {
                sMimeType = MimeTypes[sFileExt].ToString();
            }
            catch (Exception)
            {
                MainForm.LogErrorToFile("No mime type for request " + sRequestedFile);
            }


            return sMimeType;
        }


        public void SendHeader(string sHttpVersion, string sMimeHeader, int iTotBytes, string sStatusCode, int cacheDays,
                               ref Socket socket)
        {
            String sBuffer = "";

            // if Mime type is not provided set default to text/html
            if (sMimeHeader.Length == 0)
            {
                sMimeHeader = "text/html"; // Default Mime Type is text/html
            }

            sBuffer += sHttpVersion + sStatusCode + "\r\n";
            sBuffer += "Server: iSpy\r\n";
            sBuffer += "Content-Type: " + sMimeHeader + "\r\n";
            //sBuffer += "X-Content-Type-Options: nosniff\r\n";
            sBuffer += "Accept-Ranges: bytes\r\n";
            sBuffer += "Access-Control-Allow-Origin: *\r\n";
            if (iTotBytes > -1)
                sBuffer += "Content-Length: " + iTotBytes + "\r\n";
            //sBuffer += "Cache-Control:Date: Tue, 25 Jan 2011 08:18:53 GMT\r\nExpires: Tue, 08 Feb 2011 05:06:38 GMT\r\nConnection: keep-alive\r\n";
            if (cacheDays > 0)
            {
                //this is needed for video content to work in chrome/android
                DateTime d = DateTime.UtcNow;
                sBuffer += "Cache-Control: Date: " + d.ToUniversalTime().ToString("r") +
                           "\r\nLast-Modified: Tue, 01 Jan 2011 12:00:00 GMT\r\nExpires: " +
                           d.AddDays(cacheDays).ToUniversalTime().ToString("r") + "\r\nConnection: keep-alive\r\n";
            }

            sBuffer += "\r\n";

            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);

            SendToBrowser(bSendData, socket);
            //Console.WriteLine("Total Bytes : " + iTotBytes);
        }

        public void SendHeaderWithRange(string sHttpVersion, string sMimeHeader, int iStartBytes, int iEndBytes,
                                        int iTotBytes, string sStatusCode, int cacheDays, Socket socket)
        {
            String sBuffer = "";

            // if Mime type is not provided set default to text/html
            if (sMimeHeader.Length == 0)
            {
                sMimeHeader = "text/html"; // Default Mime Type is text/html
            }

            sBuffer += sHttpVersion + sStatusCode + "\r\n";
            sBuffer += "Server: iSpy\r\n";
            sBuffer += "Content-Type: " + sMimeHeader + "\r\n";
            //sBuffer += "X-Content-Type-Options: nosniff\r\n";
            sBuffer += "Accept-Ranges: bytes\r\n";
            sBuffer += "Content-Range: bytes " + iStartBytes + "-" + iEndBytes + "/" + (iTotBytes) + "\r\n";
            sBuffer += "Content-Length: " + (iEndBytes - iStartBytes + 1) + "\r\n";
            if (cacheDays > 0)
            {
                //this is needed for video content to work in chrome/android
                DateTime d = DateTime.UtcNow;
                sBuffer += "Cache-Control: Date: " + d.ToUniversalTime().ToString("r") +
                           "\r\nLast-Modified: Tue, 01 Jan 2011 12:00:00 GMT\r\nExpires: " +
                           d.AddDays(cacheDays).ToUniversalTime().ToString("r") + "\r\nConnection: keep-alive\r\n";
            }

            sBuffer += "\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);

            SendToBrowser(bSendData, socket);
            //Console.WriteLine("Total Bytes : " + iTotBytes);
        }


        /// <summary>
        /// Overloaded Function, takes string, convert to bytes and calls 
        /// overloaded sendToBrowserFunction.
        /// </summary>
        /// <param name="sData">The data to be sent to the browser(client)</param>
        /// <param name="socket">Socket reference</param>
        public void SendToBrowser(String sData, Socket socket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), socket);
        }


        /// <summary>
        /// Sends data to the browser (client)
        /// </summary>
        /// <param name="bSendData">Byte Array</param>
        /// <param name="socket">Socket reference</param>
        public void SendToBrowser(Byte[] bSendData, Socket socket)
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Blocking = true;

                    int sent = socket.Send(bSendData);
                    if (sent < bSendData.Length)
                    {
                        //Debug.WriteLine("Only sent " + sent + " of " + bSendData.Length);
                    }
                    if (sent == -1)
                        MainForm.LogExceptionToFile(new Exception("Socket Error cannot Send Packet"));
                }
            }
            catch (Exception e)
            {
                //Debug.WriteLine("Send To Browser Error: " + e.Message);
                MainForm.LogExceptionToFile(e);
            }
        }

        public bool ThumbnailCallback()
        {
            return false;
        }


        //This method Accepts new connection and
        //First it receives the welcome massage from the client,
        //Then it sends the Current date time to the Client.
        public void StartListen()
        {
            String sRequest;
            String sMyWebServerRoot = ServerRoot;
            String sPhysicalFilePath;


            while (Running && NumErr < 5)
            {
                //Accept a new connection
                try
                {
                    Socket mySocket = _myListener.AcceptSocket();
                    mySocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);

                    if (MySockets.Count() < _socketindex + 1)
                    {
                        MySockets.Add(mySocket);
                    }
                    else
                        MySockets[_socketindex] = mySocket;

                    if (mySocket.Connected)
                    {
                        mySocket.NoDelay = true;
                        mySocket.ReceiveBufferSize = 8192;
                        mySocket.ReceiveTimeout = MainForm.Conf.ServerReceiveTimeout;
                        try
                        {
                            //make a byte array and receive data from the client 
                            string sHttpVersion;
                            string sFileName;
                            string resp;
                            String sMimeType;
                            bool bServe, bHasAuth;

                            var bReceive = new Byte[1024];
                            mySocket.Receive(bReceive);
                            string sBuffer = Encoding.ASCII.GetString(bReceive);

                            if (sBuffer.Substring(0, 3) != "GET")
                            {
                                goto Finish;
                            }
                            String sRequestedFile;
                            String sErrorMessage;
                            String sLocalDir;
                            String sDirName;
                            ParseRequest(sMyWebServerRoot, sBuffer, out sRequest, out sRequestedFile, out sErrorMessage,
                                         out sLocalDir, out sDirName, out sPhysicalFilePath, out sHttpVersion,
                                         out sFileName, out sMimeType, out bServe, out bHasAuth, ref mySocket);
                            if (!bServe)
                            {
                                resp = "Denied('Access this server through ispyconnect.com')";
                                SendHeader(sHttpVersion, "text/javascript", resp.Length, " 200 OK", 0, ref mySocket);
                                SendToBrowser(resp, mySocket);
                                goto Finish;
                            }

                            resp = ProcessCommandInternal(sRequest);

                            if (resp != "")
                            {
                                SendHeader(sHttpVersion, "text/javascript", resp.Length, " 200 OK", 0, ref mySocket);
                                SendToBrowser(resp, mySocket);
                            }
                            else //not a js request
                            {
                                if (!bHasAuth)
                                {
                                    resp = "Denied('Access this server through ispyconnect.com')";
                                    SendHeader(sHttpVersion, "text/javascript", resp.Length, " 200 OK", 0, ref mySocket);
                                    SendToBrowser(resp, mySocket);
                                    goto Finish;
                                }
                                string cmd = sRequest.Trim('/').ToLower();
                                int i = cmd.IndexOf("?");
                                if (i>-1)
                                    cmd = cmd.Substring(0,i );
                                if (cmd.StartsWith("get /"))
                                    cmd = cmd.Substring(5);

                                int oid, otid;
                                int.TryParse(GetVar(sRequest, "oid"), out oid);
                                int.TryParse(GetVar(sRequest, "ot"), out otid);
                                switch(cmd)
                                {
                                    case "logfile":
                                        SendLogFile(sHttpVersion, ref mySocket);
                                        break;
                                    case "livefeed":
                                        SendLiveFeed(sPhysicalFilePath, sHttpVersion, ref mySocket);
                                        break;
                                    case "loadimage":
                                        SendImage(sPhysicalFilePath, sHttpVersion, ref mySocket);
                                        break;
                                    case "floorplanfeed":
                                        SendFloorPlanFeed(sPhysicalFilePath, sHttpVersion, ref mySocket);
                                        break;
                                    case "audiofeed":
                                        SendAudioFeed(sBuffer, sPhysicalFilePath, ref mySocket);
                                        break;
                                    case "loadclip.flv":
                                    case "loadclip.fla":
                                    case "loadclip.mp3":
                                    case "loadclip.mp4":
                                        SendClip(sPhysicalFilePath, sBuffer, sHttpVersion, ref mySocket);
                                        break;
                                    default:
                                        if (sPhysicalFilePath.IndexOf('?') != -1)
                                        {
                                            sPhysicalFilePath = sPhysicalFilePath.Substring(0, sPhysicalFilePath.IndexOf('?'));
                                        }

                                        if (!File.Exists(sPhysicalFilePath))
                                        {
                                            ServeNotFound(sHttpVersion, ref mySocket);
                                        }
                                        else
                                        {
                                            ServeFile(sHttpVersion, sPhysicalFilePath, sMimeType, ref mySocket);
                                        }
                                        break;
                                }
                            }
                            
                            Finish:
                            NumErr = 0;
                        }
                        catch (SocketException ex)
                        {
                            //Debug.WriteLine("Server Error (socket): " + ex.Message);
                            MainForm.LogExceptionToFile(ex);
                            NumErr++;
                        }

                        if (MySockets.Count() == _socketindex + 1)
                        {
                            mySocket.Close();
                            //mySocket = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("Server Error (generic): " + ex.Message);
                    MainForm.LogExceptionToFile(ex);
                    NumErr++;
                }
            }
        }

        private void SendClip(String sPhysicalFilePath, string sBuffer, string sHttpVersion, ref Socket mySocket)
        {
            int oid = Convert.ToInt32(GetVar(sPhysicalFilePath, "oid"));
            int ot =  Convert.ToInt32(GetVar(sPhysicalFilePath, "ot"));

            
            string dir = MainForm.Conf.MediaDirectory;
            if (ot==1)
            {
                dir += @"audio\"+MainForm.Microphones.Single(p => p.id == oid).directory + @"\";
            }
            if (ot==2)
            {
                dir += @"video\"+MainForm.Cameras.Single(p => p.id == oid).directory + @"\";
            }
            string fn = dir+GetVar(sPhysicalFilePath, "fn");

            int iStartBytes = 0;
            int iEndBytes = 0;
            bool isrange = false;

            if (sBuffer.IndexOf("Range: bytes=") != -1)
            {
                string[] headers = sBuffer.Split(Environment.NewLine.ToCharArray());
                foreach (string h in headers)
                {
                    if (h.StartsWith("Range:"))
                    {
                        string[] range = (h.Substring(h.IndexOf("=") + 1)).Split('-');
                        iStartBytes = Convert.ToInt32(range[0]);
                        if (range[1] != "")
                        {
                            iEndBytes = Convert.ToInt32(range[1]);
                        }
                        else
                        {
                            iEndBytes = -1;
                        }
                        isrange = true;
                        break;
                    }
                }
            }


            var fi = new FileInfo(fn);
            int iTotBytes = Convert.ToInt32(fi.Length);
            if (iEndBytes == -1)
                iEndBytes = iTotBytes - 1;
            var fs =
                new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
            // Create a reader that can read bytes from the FileStream.

            var reader = new BinaryReader(fs);
            byte[] bytes;
            if (!isrange)
            {
                bytes = new byte[fs.Length];
                while ((reader.Read(bytes, 0, bytes.Length)) != 0)
                {
                }
            }
            else
            {
                bytes = new byte[iEndBytes - iStartBytes + 1];
                reader.BaseStream.Seek(iStartBytes, SeekOrigin.Begin);
                bytes = reader.ReadBytes(bytes.Length);
            }

            reader.Close();
            fs.Close();
            string sMimeType = GetMimeType(fn);
            if (isrange)
            {
                SendHeaderWithRange(sHttpVersion, sMimeType, iStartBytes, iEndBytes, iTotBytes, " 206 Partial Content",
                                    20, mySocket);
            }
            else
                SendHeader(sHttpVersion, sMimeType, iTotBytes, " 200 OK", 20, ref mySocket);


            SendToBrowser(bytes, mySocket);
        }
        private void ServeNotFound(string sHttpVersion, ref Socket mySocket)
        {
            const string resp = "iSpy server is running";
            SendHeader(sHttpVersion, "", resp.Length, " 200 OK", 0, ref mySocket);
            SendToBrowser(resp, mySocket);
        }

        public static List<String> AllowedIPs;

        private void ParseRequest(String sMyWebServerRoot, string sBuffer, out String sRequest,
                                  out String sRequestedFile, out String sErrorMessage, out String sLocalDir,
                                  out String sDirName, out String sPhysicalFilePath, out string sHttpVersion,
                                  out string sFileName, out String sMimeType, out bool bServe, out bool bHasAuth, ref Socket mySocket)
        {
            sErrorMessage = "";
            string sClientIP = mySocket.RemoteEndPoint.ToString();

            sClientIP = sClientIP.Substring(0, sClientIP.LastIndexOf(":")).Trim();
            sClientIP = sClientIP.Replace("[", "").Replace("]", "");
            bServe = AllowedIPs.Contains(sClientIP);

            int iStartPos = sBuffer.IndexOf("HTTP", 1);

            sHttpVersion = sBuffer.Substring(iStartPos, 8);
            sRequest = sBuffer.Substring(0, iStartPos - 1);
            sRequest.Replace("\\", "/");

            //if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
            //{
            //    sRequest = sRequest + "/";
            //}

            if (sRequest.IndexOf("command.txt") != -1)
            {
                sRequest = sRequest.Replace("Video/", "Video|");
                sRequest = sRequest.Replace("Audio/", "Audio|");
            }
            iStartPos = sRequest.LastIndexOf("/") + 1;
            sRequestedFile = sRequest.Substring(iStartPos);
            GetDirectoryPath(sRequest, sMyWebServerRoot, out sLocalDir, out sDirName);


            if (sLocalDir.Length == 0)
            {
                sErrorMessage = "<H2>Error!! Requested Directory does not exists</H2><Br>";
                SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", 0, ref mySocket);
                SendToBrowser(sErrorMessage, mySocket);
                throw new Exception("Requested Directory does not exist (" + sLocalDir + ")");
            }

            ParseMimeType(sRequestedFile, out sFileName, out sMimeType);

            sPhysicalFilePath = (sLocalDir + sRequestedFile).Replace("%20", " ").ToLower();

            bHasAuth = sPhysicalFilePath.EndsWith("crossdomain.xml") || CheckAuth(sPhysicalFilePath);
            if (!bServe)
                bServe = bHasAuth;
        }

        private void ServeFile(string sHttpVersion, string sFileName, String sMimeType,
                               ref Socket mySocket)
        {
            var fi = new FileInfo(sFileName);
            int iTotBytes = Convert.ToInt32(fi.Length);
            
            var fs =
                new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            var reader = new BinaryReader(fs);

            var bytes = new byte[fs.Length];
            while ((reader.Read(bytes, 0, bytes.Length)) != 0)
            {
            }
            reader.Close();
            fs.Close();

            SendHeader(sHttpVersion, sMimeType, iTotBytes, " 200 OK", 20, ref mySocket);
            SendToBrowser(bytes, mySocket);
        }

        private static string GetVar(string url, string var)
        {
            url = url.ToLower();
            var = var.ToLower();

            int i = url.IndexOf("&"+var);
            if (i == -1)
                i = url.IndexOf("?" + var);
            if (i == -1)
            {
                i = url.IndexOf(var);
                if (i == -1)
                    return "";
                i--;
            }

            string txt = url.Substring(i + var.Length + 1).Trim('=');
            if (txt.IndexOf("&") != -1)
                txt = txt.Substring(0, txt.IndexOf("&"));

            return txt;
        }

        internal string ProcessCommandInternal(string sRequest)
        {
            string cmd = sRequest.Trim('/').ToLower().Trim();
            string resp = "";
            int i = cmd.IndexOf("?");
            if (i!=-1)
                cmd = cmd.Substring(0, i);
            if (cmd.StartsWith("get /"))
                cmd = cmd.Substring(5);

            int oid, otid;
            int.TryParse(GetVar(sRequest, "oid"), out oid);
            int.TryParse(GetVar(sRequest, "ot"), out otid);
            string func = GetVar(sRequest, "jsfunc").Replace("%27","'");
            string fn = GetVar(sRequest, "fn");
            string temp="";

            long sdl = 0, edl = 0;
            string sd, ed;
            int pageSize, ordermode, page;

            switch (cmd)
            {
                case "command.txt": //legacy (test connection)
                case "connect":
                    resp = MainForm.Identifier+",OK";
                    break;
                case "recordswitch":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        
                        if (vw != null)
                        {
                            bool sw = !vw.Recording;
                            resp = vw.RecordSwitch(sw) + ",OK";
                        }
                        else
                            resp = "stopped,Microphone not found,OK";
                    }
                    if (otid == 2)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        
                        if (cw != null)
                        {
                            bool sw = !cw.Recording;
                            resp = cw.RecordSwitch(sw) + ",OK";
                        }
                        else
                            resp = "stopped,Camera not found,OK";
                    }
                    break;
                case "record":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            resp = vw.RecordSwitch(true) + ",OK";
                        }
                        else
                            resp = "Microphone not found,OK";
                    }
                    if (otid == 2)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            resp = cw.RecordSwitch(true) + ",OK";
                        }
                        else
                            resp = "Camera not found,OK";
                    }
                    if (otid == 0)
                    {
                        _parent.RecordAll(true);
                    }
                    break;
                case "alert":
                    if (otid == 1)
                    {
                        var vl = _parent.GetVolumeLevel(oid);
                        if (vl != null)
                        {
                            vl.UpdateLevel(1);
                            resp = "OK";
                        }
                        else
                            resp = "Microphone not found,OK";
                    }
                    
                    if (otid == 2)
                    {
                        var cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.CameraAlarm(this, EventArgs.Empty);
                            resp = "OK";
                        }
                        else
                            resp = "Camera not found,OK";
                    }
                    
                    break;
                case "recordoff":
                    if (otid == 1)
                    {
                        var vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            resp = vw.RecordSwitch(false) + ",OK";
                        }
                        else
                            resp = "Microphone not found,OK";
                    }
                    if (otid == 2)
                    {
                        var cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            resp = cw.RecordSwitch(false) + ",OK";
                        }
                        else
                            resp = "Camera not found,OK";
                    }
                    if (otid == 0)
                    {
                        _parent.RecordAll(false);
                    }
                    break;
                case "ping":
                    resp = "OK";
                    break;
                case "allon":
                    _parent.SwitchObjects(false, true);
                    resp = "OK";
                    break;
                case "alloff":
                    _parent.SwitchObjects(false, false);
                    resp = "OK";
                    break;
                case "recordondetecton":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            vw.Micobject.detector.recordondetect = true;
                        }
                    }
                    if (otid == 2)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.Camobject.detector.recordondetect = true;
                        }
                    }
                    if (otid==0)
                    {
                        _parent.RecordOnDetect(true);
                    }

                    break;
                case "recordondetectoff":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            vw.Micobject.detector.recordondetect = false;
                        }
                    }
                    if (otid == 2)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.Camobject.detector.recordondetect = false;
                        }
                    }
                    if (otid == 0)
                    {
                        _parent.RecordOnDetect(false);
                    }
                    break;
                case "alerton":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            vw.Micobject.alerts.active = true;
                        }
                    }
                    if (otid == 2)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.Camobject.alerts.active = true;
                        }
                    }
                    if (otid == 0)
                    {
                        _parent.AlertsActive(true);
                    }

                    break;
                case "alertoff":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            vw.Micobject.alerts.active = false;
                        }
                    }
                    if (otid == 2)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.Camobject.alerts.active = false;
                        }
                    }
                    if (otid == 0)
                    {
                        _parent.AlertsActive(false);
                    }
                    break;
                case "allscheduledon":
                    _parent.SwitchObjects(true, true);
                    resp = "OK";
                    break;
                case "allscheduledoff":
                    _parent.SwitchObjects(true, false);
                    resp = "OK";
                    break;
                case "applyschedule":
                    _parent.ApplySchedule();
                    resp = "OK";
                    break;
                case "bringonline":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            vw.Enable();
                        }
                    }
                    else
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.Enable();
                        }
                    }
                    resp = "OK";
                    break;
                case "smscmd":
                case "executecmd":
                    int commandIndex = Convert.ToInt32(GetVar(sRequest,"id"));
                    objectsCommand oc = MainForm.RemoteCommands.SingleOrDefault(p => p.id == commandIndex);

                    if (oc != null)
                    {
                        try
                        {
                            if (oc.command.StartsWith("ispy "))
                            {
                                string cmd2 = oc.command.Substring(5).ToLower();
                                ProcessCommandInternal(cmd2);
                            }
                            else
                                Process.Start(oc.command);
                            resp = "Command Executed.,OK";
                        }
                        catch (Exception ex)
                        {
                            MainForm.LogExceptionToFile(ex);
                            resp = "Command Failed: " + ex.Message + ",OK";
                        }
                    }
                    else
                        resp = "OK";
                    break;
                case "takeoffline":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            vw.Disable();
                        }
                    }
                    else
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            cw.Disable();
                        }
                    }
                    resp = "OK";
                    break;
                case "deletefile":
                    if (otid == 1)
                    {
                        try
                        {
                            string subdir = GetDirectory(1, oid);
                            File.Delete(MainForm.Conf.MediaDirectory + "audio\\" + subdir + @"\" + fn);
                            _parent.GetVolumeLevel(oid).FileList.RemoveAll(p => p.Filename == fn);
                        }
                        catch (Exception e)
                        {
                            MainForm.LogExceptionToFile(e);
                        }
                        
                    }
                    if (otid == 2)
                    {
                        try
                        {
                            string subdir = GetDirectory(2, oid);

                            var fi =
                                new FileInfo(MainForm.Conf.MediaDirectory + "video\\" + subdir + @"\" +
                                                fn);
                            string ext = fi.Extension.Trim();
                            try
                            {
                                File.Delete(MainForm.Conf.MediaDirectory + "video\\" + subdir + @"\" +
                                            fn);
                                _parent.GetCameraWindow(oid).FileList.RemoveAll(p => p.Filename == fn);
                            }
                            catch
                            {
                            }
                            try
                            {
                                File.Delete(MainForm.Conf.MediaDirectory + "video\\" + subdir + @"\thumbs\" +
                                            fn.Replace(ext, ".jpg"));
                            }
                            catch
                            {
                            }
                            try
                            {
                                File.Delete(MainForm.Conf.MediaDirectory + "video\\" + subdir + @"\thumbs\" +
                                            fn.Replace(ext, "_large.jpg"));
                            }
                            catch
                            {
                            }
                        }
                        catch (Exception e)
                        {
                            MainForm.LogExceptionToFile(e);
                        }
                    }
                    resp = "OK";
                    break;
                case "deleteall":
                    string objdir = GetDirectory(otid, oid);

                    Helper.DeleteAllContent(otid, objdir);
                    if (otid == 1)
                        _parent.GetVolumeLevel(oid).FileList.Clear();
                    if (otid == 2)
                        _parent.GetCameraWindow(oid).FileList.Clear();

                    resp = "OK";
                    break;
                case "uploadyoutube":
                    bool @public = Convert.ToBoolean(GetVar(sRequest, "public"));
                    resp = YouTubeUploader.AddUpload(oid, fn, @public) + ",OK";
                    break;
                case "sendbyemail":
                    string email = GetVar(sRequest, "email");
                    string message = GetVar(sRequest, "message").Replace("%20"," ");
                    resp = YouTubeUploader.AddUpload(oid, fn, true, email, message) + ",OK";
                    break;
                case "removeobject":
                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        if (vw != null)
                        {
                            _parent.RemoveMicrophone(vw);
                        }
                    }
                    else
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        if (cw != null)
                        {
                            _parent.RemoveCamera(cw);
                        }
                    }
                    MainForm.WSW.ForceSync();
                    MainForm.NeedsSync = false;
                    resp = "OK";
                    break;
                case "addobject":
                    int sourceIndex = Convert.ToInt32(GetVar(sRequest, "stid"));
                    int width = Convert.ToInt32(GetVar(sRequest, "w"));
                    int height = Convert.ToInt32(GetVar(sRequest, "h"));
                    string name = GetVar(sRequest, "name");
                    string url = GetVar(sRequest, "url").Replace("\\", "/");
                    _parent.AddObjectExternal(otid, sourceIndex, width, height, name, url);
                    MainForm.WSW.ForceSync();
                    MainForm.NeedsSync = false;
                    resp = "OK";
                    break;
                case "changesetting":
                    string field = GetVar(sRequest,"field");
                    string value = GetVar(sRequest, "value");

                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        switch (field)
                        {
                            case "notifyondisconnect":
                                vw.Micobject.settings.notifyondisconnect = Convert.ToBoolean(value);
                                break;
                            case "recordondetect":
                                vw.Micobject.detector.recordondetect = Convert.ToBoolean(value);
                                if (vw.Micobject.detector.recordondetect)
                                    vw.Micobject.detector.recordonalert = false;
                                break;
                            case "recordonalert":
                                vw.Micobject.detector.recordonalert = Convert.ToBoolean(value);
                                if (vw.Micobject.detector.recordonalert)
                                    vw.Micobject.detector.recordondetect = false;
                                break;
                            case "scheduler":
                                vw.Micobject.schedule.active = Convert.ToBoolean(value);
                                break;
                            case "alerts":
                                vw.Micobject.alerts.active = Convert.ToBoolean(value);
                                break;
                            case "sendemailonalert":
                                vw.Micobject.notifications.sendemail = Convert.ToBoolean(value);
                                break;
                            case "sendsmsonalert":
                                vw.Micobject.notifications.sendsms = Convert.ToBoolean(value);
                                break;
                            case "minimuminterval":
                                int mi;
                                int.TryParse(value, out mi);
                                vw.Micobject.alerts.minimuminterval = mi;
                                break;
                            case "accessgroups":
                                vw.Micobject.settings.accessgroups = value;
                                break;
                        }
                    }
                    else
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        switch (field)
                        {
                            case "youtube":
                                cw.Camobject.settings.youtube.autoupload = Convert.ToBoolean(value);
                                break;
                            case "notifyondisconnect":
                                cw.Camobject.settings.notifyondisconnect = Convert.ToBoolean(value);
                                break;
                            case "ftp":
                                cw.Camobject.ftp.enabled = Convert.ToBoolean(value);
                                break;
                            case "recordondetect":
                                cw.Camobject.detector.recordondetect = Convert.ToBoolean(value);
                                if (cw.Camobject.detector.recordondetect)
                                    cw.Camobject.detector.recordonalert = false;
                                break;
                            case "recordonalert":
                                cw.Camobject.detector.recordonalert = Convert.ToBoolean(value);
                                if (cw.Camobject.detector.recordonalert)
                                    cw.Camobject.detector.recordondetect = false;
                                break;
                            case "scheduler":
                                cw.Camobject.schedule.active = Convert.ToBoolean(value);
                                break;
                            case "alerts":
                                cw.Camobject.alerts.active = Convert.ToBoolean(value);
                                break;
                            case "sendemailonalert":
                                cw.Camobject.notifications.sendemail = Convert.ToBoolean(value);
                                break;
                            case "sendsmsonalert":
                                cw.Camobject.notifications.sendsms = Convert.ToBoolean(value);
                                if (cw.Camobject.notifications.sendsms)
                                    cw.Camobject.notifications.sendmms = false;
                                break;
                            case "sendmmsonalert":
                                cw.Camobject.notifications.sendmms = Convert.ToBoolean(value);
                                if (cw.Camobject.notifications.sendmms)
                                    cw.Camobject.notifications.sendsms = false;
                                break;
                            case "emailframeevery":
                                int gi;
                                int.TryParse(value, out gi);
                                cw.Camobject.notifications.emailgrabinterval = gi;
                                break;
                            case "timelapseon":
                                cw.Camobject.recorder.timelapseenabled = Convert.ToBoolean(value);
                                break;
                            case "timelapse":
                                int tl;
                                int.TryParse(value, out tl);
                                cw.Camobject.recorder.timelapse = tl;
                                break;
                            case "timelapseframes":
                                int tlf;
                                int.TryParse(value, out tlf);
                                cw.Camobject.recorder.timelapseframes = tlf;
                                break;
                            case "ptz":
                                if (value != "")
                                {
                                    try
                                    {
                                        cw.CalibrateCount = 0;
                                        cw.Calibrating = true;

                                        if (value.StartsWith("ispydir_"))
                                            cw.PTZ.SendPTZCommand((Enums.PtzCommand)Convert.ToInt32(value.Replace("ispydir_", "")));
                                        else
                                            cw.PTZ.SendPTZCommand(value,true);
                                    }
                                    catch (Exception ex)
                                    {
                                        MainForm.LogErrorToFile(LocRm.GetString("Validate_Camera_PTZIPOnly") + ": " +
                                                                ex.Message);
                                    }
                                }
                                break;
                            case "minimuminterval":
                                int mi;
                                int.TryParse(value, out mi);
                                cw.Camobject.alerts.minimuminterval = mi;
                                break;
                            case "accessgroups":
                                cw.Camobject.settings.accessgroups = value;
                                break;
                        }
                    }
                    resp = "OK";
                    break;
                case "closeaudio":
                    try
                    {
                        VolumeLevel vl = _parent.GetVolumeLevel(oid);
                        vl.CloseStream = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Server Error (livefeed): " + ex.Message);
                        MainForm.LogExceptionToFile(ex);
                    }

                    resp = "OK";
                    break;
                case "getcontentlist":
                    page = Convert.ToInt32(GetVar(sRequest, "page"));
                    
                    sd = GetVar(sRequest, "startdate");
                    ed = GetVar(sRequest, "enddate");
                    pageSize = Convert.ToInt32(GetVar(sRequest, "pagesize"));
                    ordermode = Convert.ToInt32(GetVar(sRequest, "ordermode"));
                    if (sd!="")
                        sdl = Convert.ToInt64(sd);
                    if (ed != "")
                        edl = Convert.ToInt64(ed);


                    switch (otid)
                    {
                        case 1:
                            VolumeLevel vl = _parent.GetVolumeLevel(oid);
                            List<FilesFile> lFi = vl.FileList;
                            if (sdl > 0)
                                lFi = lFi.FindAll(f => f.CreatedDateTicks > sdl).ToList();
                            if (edl > 0)
                                lFi = lFi.FindAll(f => f.CreatedDateTicks < edl).ToList();
                            func = func.Replace("resultcount", lFi.Count.ToString());

                            switch (ordermode)
                            {
                                case 1:
                                    //default
                                    break;
                                case 2:
                                    lFi = lFi.OrderByDescending(p => p.DurationSeconds).ToList();
                                    break;
                                case 3:
                                    lFi = lFi.OrderByDescending(p => p.MaxAlarm).ToList();
                                    break;
                            }
                                

                            var lResults = lFi.Skip(pageSize * page).Take(pageSize).ToList();
                            temp = lResults.Aggregate("", (current, fi) => current + (fi.Filename + "|" + FormatBytes(fi.SizeBytes) + "|" + String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}", fi.MaxAlarm) + ","));
                            resp = temp.Trim(',');
                            break;
                        case 2:
                            CameraWindow cw = _parent.GetCameraWindow(oid);
                            List<FilesFile> lFi2 = cw.FileList;
                            if (sdl > 0)
                                lFi2 = lFi2.FindAll(f => f.CreatedDateTicks > sdl).ToList();
                            if (edl > 0)
                                lFi2 = lFi2.FindAll(f => f.CreatedDateTicks < edl).ToList();
                            func = func.Replace("resultcount", lFi2.Count.ToString());

                            switch (ordermode)
                            {
                                case 1:
                                    //default
                                    break;
                                case 2:
                                    lFi2 = lFi2.OrderByDescending(p => p.DurationSeconds).ToList();
                                    break;
                                case 3:
                                    lFi2 = lFi2.OrderByDescending(p => p.MaxAlarm).ToList();
                                    break;
                            }
                    
                            var lResults2 = lFi2.Skip(pageSize*page).Take(pageSize).ToList();
                            temp = lResults2.Aggregate("", (current, fi) => current + (fi.Filename + "|" + FormatBytes(fi.SizeBytes) + "|" + String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}", fi.MaxAlarm) + ","));
                            resp = temp.Trim(',');
                            break;

                    }          
                    break;
                case "getcontentcounts":
                    sd = GetVar(sRequest, "startdate");
                    ed = GetVar(sRequest, "enddate");
                    if (sd != "")
                        sdl = Convert.ToInt64(sd);
                    if (ed != "")
                        edl = Convert.ToInt64(ed);
                    string oclall = "";
                    foreach (objectsCamera oc1 in MainForm.Cameras)
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oc1.id);

                        List<FilesFile> lFi2 = cw.FileList;
                        if (sdl > 0)
                            lFi2 = lFi2.FindAll(f => f.CreatedDateTicks > sdl).ToList();
                        if (edl > 0)
                            lFi2 = lFi2.FindAll(f => f.CreatedDateTicks < edl).ToList();
                        oclall += "2," + oc1.id + "," + lFi2.Count + "|";

                    }
                    foreach (objectsMicrophone om1 in MainForm.Microphones)
                    {
                        VolumeLevel vl = _parent.GetVolumeLevel(om1.id);
                        List<FilesFile> lFi = vl.FileList;
                        if (sdl > 0)
                            lFi = lFi.FindAll(f => f.CreatedDateTicks > sdl).ToList();
                        if (edl > 0)
                            lFi = lFi.FindAll(f => f.CreatedDateTicks < edl).ToList();
                        oclall += "1," + om1.id + "," + lFi.Count + "|";
                    }
                    resp = oclall.Trim('|');
                    break;
                case "getfloorplanalerts":
                    foreach (objectsFloorplan ofp in MainForm.FloorPlans)
                    {
                        FloorPlanControl fpc = _parent.GetFloorPlan(ofp.id);
                        if (fpc != null && fpc.ImgPlan != null)
                        {
                            temp += ofp.id + "," + fpc.LastAlertTimestamp + ","+fpc.LastRefreshTimestamp+"," + fpc.LastOid + "," + fpc.LastOtid + "|";
                        }
                    }
                    resp = temp.Trim('|');
                    break;
                case "getfloorplans":
                    foreach(objectsFloorplan ofp in MainForm.FloorPlans)
                    {
                        FloorPlanControl fpc = _parent.GetFloorPlan(ofp.id);
                        if (fpc != null && fpc.ImgPlan != null)
                        {
                            temp += ofp.id + "," + ofp.name.Replace(",", "").Replace("|", "").Replace("^", "") + "," +
                                      ofp.width + "," + ofp.height + "|";

                            temp = ofp.objects.@object.Aggregate(temp,
                                                                   (current, ofpo) =>
                                                                   current +
                                                                   (ofpo.id + "," + ofpo.type + "," + (ofpo.x) + "," +
                                                                    (ofpo.y) + "_"));
                            temp = temp.Trim('_');
                            temp += "^";
                        }
                    }
                    resp = temp.Replace("\"", "");
                    break;
                case "getgraph":
                    FilesFile ff = null;
                    switch (otid)
                    {
                        case 1:
                            VolumeLevel vl = _parent.GetVolumeLevel(oid);
                            if (vl!=null)
                            {
                                ff = vl.FileList.FirstOrDefault(p => p.Filename == fn);
                            }
                            break;
                        case 2:
                            CameraWindow cw = _parent.GetCameraWindow(oid);
                            if (cw != null)
                            {
                                ff = cw.FileList.FirstOrDefault(p => p.Filename == fn);
                            }
                            break;
                    }
                    if (ff!=null)
                    {
                        func = func.Replace("data","\""+ff.AlertData+"\"");
                        func = func.Replace("duration", "\"" + ff.DurationSeconds + "\"");
                        func = func.Replace("threshold", String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}", ff.TriggerLevel));
                    }
                    else
                    {
                        func = func.Replace("data", "\"\"");
                        func = func.Replace("duration", "0");
                        func = func.Replace("threshold", "0");

                    }
                    resp = "OK";
                    break;
                case "graphall":
                    List<FilesFile> ffs = null;          
                    switch (otid)
                    {
                        case 1:
                            VolumeLevel vl = _parent.GetVolumeLevel(oid);
                            if (vl != null)
                            {
                                ffs = vl.FileList.ToList();
                            }
                            break;
                        case 2:
                            CameraWindow cw = _parent.GetCameraWindow(oid);
                            if (cw != null)
                            {
                                ffs = cw.FileList.ToList();
                            }
                            break;
                    }
                    if (ffs != null)
                    {

                        sd = GetVar(sRequest, "startdate");
                        ed = GetVar(sRequest, "enddate");

                        if (sd!="")
                            sdl = Convert.ToInt64(sd);
                        if (ed != "")
                            edl = Convert.ToInt64(ed);

                        if (sdl > 0)
                            ffs = ffs.FindAll(f => f.CreatedDateTicks > sdl).ToList();
                        if (edl > 0)
                            ffs = ffs.FindAll(f => f.CreatedDateTicks < edl).ToList();

                        foreach (FilesFile f in ffs)
                        {
                            temp += (long)(f.CreatedDateTicks.UnixTicks()) + "|" + String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}", f.MaxAlarm) + "|" + f.DurationSeconds + "|" + f.Filename + ",";
                        }
                        func = func.Replace("data", "\"" + temp.Trim(',') + "\"");
                        
                    }
                    else
                    {
                        func = func.Replace("data", "\"\"");

                    }
                    resp = "OK";
                    break;
                case "massdelete":
                    string[] files = GetVar(sRequest, "filelist").Trim('|').Split('|');
                    string dir = "audio";
                    if (otid == 2)
                        dir = "video";

                    string folderpath = MainForm.Conf.MediaDirectory + dir + "\\" +
                                   GetDirectory(otid, oid) + "\\";

                    VolumeLevel vlUpdate = null;
                    CameraWindow cwUpdate = null;
                    if (otid == 1)
                    {
                        vlUpdate = _parent.GetVolumeLevel(oid);
                    }
                    if (otid == 2)
                    {
                        cwUpdate = _parent.GetCameraWindow(oid);
                    }
                    foreach(string fn3 in files)
                    {
                        var fi = new FileInfo(folderpath +
                                                     fn3);
                        string ext = fi.Extension.Trim();
                        try
                        {
                            File.Delete(folderpath + fn3);
                        }
                        catch
                        {
                        }
                        if (otid == 2)
                        {
                            try
                            {
                                File.Delete(folderpath + "thumbs\\" + fn3.Replace(ext, ".jpg"));

                            }
                            catch
                            {
                            }
                            try
                            {
                                File.Delete(folderpath + "thumbs\\" + fn3.Replace(ext, "_large.jpg"));
                            }
                            catch
                            {
                            }
                        }
                        if (otid==1)
                        {
                            if (vlUpdate != null)
                            {
                                string filename1 = fn3;
                                vlUpdate.FileList.RemoveAll(p => p.Filename == filename1);
                            }
                        }
                        if (otid == 2)
                        {
                            if (cwUpdate != null)
                            {
                                string filename1 = fn3;
                                cwUpdate.FileList.RemoveAll(p => p.Filename == filename1);
                            }
                        }
                    }
                    resp = "OK";
                    break;
                case "getcontrolpanel":
                    int port = Convert.ToInt32(GetVar(sRequest, "port"));

                    string disabled = "";
                    if (!MainForm.Conf.Subscribed)
                        disabled = " disabled=\"disabled\" title=\"Not Subscribed\"";

                    if (otid == 1)
                    {
                        VolumeLevel vw = _parent.GetVolumeLevel(oid);
                        string html = "<table cellspacing=\"3px\">";
                        string strChecked = "";


                        if (vw.Micobject.alerts.active) strChecked = "checked=\"checked\"";
                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("Alerts") + "</strong></td></tr>";
                        html += "<tr><td>" + LocRm.GetString("AlertsEnabled") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'alerts',this.checked)\" " + strChecked + "/></td></tr>";

                        strChecked = "";
                        if (vw.Micobject.notifications.sendemail) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendEmailOnAlert") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'sendemailonalert',this.checked)\" " + strChecked + "/> " +
                                 vw.Micobject.settings.emailaddress + "</td></tr>";

                        strChecked = "";
                        if (vw.Micobject.notifications.sendsms) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendSmsOnAlert") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'sendsmsonalert',this.checked)\" " + strChecked + "/> " + vw.Micobject.settings.smsnumber +
                                 "</td></tr>";

                        strChecked = "";
                        if (vw.Micobject.settings.notifyondisconnect) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendEmailOnDisconnect") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'notifyondisconnect',this.checked)\" " + strChecked + "/></td></tr>";

                        html += "<tr><td>" + LocRm.GetString("DistinctAlertInterval") +
                                 "</td><td><input style=\"width:50px\" type=\"text\" value=\"" +
                                 vw.Micobject.alerts.minimuminterval + "\" onblur=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'minimuminterval',this.value)\"/> " + LocRm.GetString("Seconds") + "</td></tr>";

                        html += "<tr><td>" + LocRm.GetString("AccessGroups") +
                                 "</td><td><input style=\"width:100px\" type=\"text\" value=\"" +
                                 vw.Micobject.settings.accessgroups + "\" onblur=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'accessgroups',this.value)\"/></td></tr>";

                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("Scheduler") + "</strong></td></tr>";
                        strChecked = "";
                        if (vw.Micobject.schedule.active) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("ScheduleActive") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'scheduler',this.checked)\" " + strChecked + "/>";

                        string schedule = "";
                        for (int index = 0; index < vw.ScheduleDetails.Length; index++)
                        {
                            string s = vw.ScheduleDetails[index];
                            if (s != "")
                            {
                                schedule += s + "<br/>";
                            }
                        }
                        if (schedule != "")
                            html +=
                                "<div style=\"width:450px;height:100px;overflow-y:auto;background-color:#ddd;padding:5px\">" +
                                schedule + "</div>";
                        html += "</td></tr>";

                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("RecordingSettings") + "</strong></td></tr>";
                        strChecked = "";
                        if (vw.Micobject.detector.recordondetect) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("RecordOnDetect") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'recordondetect',this.checked)\" " + strChecked + "/></td></tr>";

                        strChecked = "";
                        if (vw.Micobject.detector.recordonalert) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("RecordOnAlert") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'recordonalert',this.checked)\" " + strChecked + "/></td></tr>";


                        html += "</table>";
                        resp += html.Replace("\"", "\\\"");
                    }
                    else
                    {
                        CameraWindow cw = _parent.GetCameraWindow(oid);
                        string html = "<table cellspacing=\"3px\">";
                        if (cw.Camobject.ptz > -1)
                        {
                            html += "<tr><td>PTZ</td><td><select id=\"ddlPTZ_" + otid + "_" + oid + "_" + port +
                                     "\">";


                            PTZSettingsCamera ptz = MainForm.PTZs.Single(p => p.id == cw.Camobject.ptz);
                            html += "<optgroup label=\"Directional\">";
                            html += "<option value=\"" + ptz.Commands.Center + "\">Center</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.Left + "\">Left</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.Upleft + "\">Left Up</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.Up + "\">Up</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.UpRight + "\">Right Up</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.Right + "\">Right</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.DownRight + "\">Right Down</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.Down + "\">Down</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.DownLeft + "\">Left Down</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.ZoomIn + "\">Zoom In</option>";
                            html += "<option value=\"ISPYDIR_" + (int)Enums.PtzCommand.ZoomOut + "\">Zoom Out</option>";
                            html += "</optgroup>";
                            html += "<optgroup label=\"Extended\">";
                            html = ptz.Commands.ExtendedCommands.Aggregate(html, (current, extcmd) => current + ("<option value=\"" + extcmd.Value + "\">" + extcmd.Name.Trim() + "</option>"));
                            html += "</optgroup>";
                            html += "</select> <input type=\"button\" onmousedown=\"PTZEXTENDED=(GEID('ddlPTZ_" + otid +
                                     "_" + oid + "_" + port + "').selectedIndex>10);send_changesetting(" + otid +
                                     "," + oid + "," + port + ",'ptz',SelectedValue('ddlPTZ_" + otid + "_" +
                                     oid + "_" + port + "'))\" onmouseup=\"if (!PTZEXTENDED) {send_changesetting(" +
                                     otid + "," + oid + "," + port + ",'ptz','" + ptz.Commands.Stop +
                                     "');}\" value=\"" + LocRm.GetString("OK") + "\"> " + LocRm.GetString("holdtopan") +
                                     "</td></tr>";
                        }
                        string strChecked = "";
                        if (cw.Camobject.alerts.active) strChecked = "checked=\"checked\"";
                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("Alerts") + "</strong></td></tr>";
                        html += "<tr><td>" + LocRm.GetString("AlertsEnabled") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'alerts',this.checked)\" " + strChecked + "/></td></tr>";

                        strChecked = "";
                        if (cw.Camobject.notifications.sendemail) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendEmailOnAlert") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'sendemailonalert',this.checked)\" " + strChecked + "/> " + cw.Camobject.settings.emailaddress +
                                 "</td></tr>";


                        strChecked = "";
                        if (cw.Camobject.notifications.sendsms) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendSmsOnAlert") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'sendsmsonalert',this.checked)\" " + strChecked + "/> " + cw.Camobject.settings.smsnumber +
                                 "</td></tr>";

                        strChecked = "";
                        if (cw.Camobject.notifications.sendmms) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendAsMmsWithImage2Credit") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'sendmmsonalert',this.checked)\" " + strChecked + "/> " + cw.Camobject.settings.smsnumber +
                                 "</td></tr>";

                        strChecked = "";
                        if (cw.Camobject.settings.notifyondisconnect) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("SendEmailOnDisconnect") + "</td><td><input type=\"checkbox\"" +
                                 disabled + " onclick=\"send_changesetting(" + otid + "," + oid + "," + port +
                                 ",'notifyondisconnect',this.checked)\" " + strChecked + "/></td></tr>";

                        html += "<tr><td>" + LocRm.GetString("EmailFrameEvery") +
                                 "</td><td><input style=\"width:50px\" type=\"text\"" + disabled + " value=\"" +
                                 cw.Camobject.notifications.emailgrabinterval + "\" onblur=\"send_changesetting(" +
                                 otid + "," + oid + "," + port + ",'emailframeevery',this.value)\"/> " +
                                 LocRm.GetString("Minutesenter0ForNoEmails") + "</td></tr>";

                        html += "<tr><td>" + LocRm.GetString("DistinctAlertInterval") +
                                 "</td><td><input style=\"width:50px\" type=\"text\" value=\"" +
                                 cw.Camobject.alerts.minimuminterval + "\" onblur=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'minimuminterval',this.value)\"/> " +LocRm.GetString("Seconds")+ "</td></tr>";

                        html += "<tr><td>" + LocRm.GetString("AccessGroups") +
                                 "</td><td><input style=\"width:100px\" type=\"text\" value=\"" +
                                 cw.Camobject.settings.accessgroups + "\" onblur=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'accessgroups',this.value)\"/></td></tr>";


                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("Scheduler") + "</strong></td></tr>";
                        strChecked = "";
                        if (cw.Camobject.schedule.active) strChecked = "checked=\"checked\"";

                        html += "<tr><td valign=\"top\">" + LocRm.GetString("ScheduleActive") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'scheduler',this.checked)\" " + strChecked + "/>";
                        string schedule = "";
                        for (int index = 0; index < cw.ScheduleDetails.Length; index++)
                        {
                            string s = cw.ScheduleDetails[index];
                            if (s != "")
                            {
                                schedule += s + "<br/>";
                            }
                        }
                        if (schedule != "")
                            html +=
                                "<div style=\"width:450px;height:100px;overflow-y:auto;background-color:#ddd;padding:5px\">" +
                                schedule + "</div>";
                        html += "</td></tr>";

                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("RecordingSettings") + "</strong></td></tr>";
                        strChecked = "";
                        if (cw.Camobject.detector.recordondetect) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("RecordOnDetect") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'recordondetect',this.checked)\" " + strChecked + "/></td></tr>";

                        strChecked = "";
                        if (cw.Camobject.detector.recordonalert) strChecked = "checked=\"checked\"";

                        html += "<tr><td>" + LocRm.GetString("RecordOnAlert") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'recordonalert',this.checked)\" " + strChecked + "/></td></tr>";


                        html += "<tr><td colspan=\"2\"><strong>" + LocRm.GetString("TimelapseRecording") +
                                 "</strong></td></tr>";

                        strChecked = "";
                        if (cw.Camobject.recorder.timelapseenabled) strChecked = "checked=\"checked\"";
                        html += "<tr><td>" + LocRm.GetString("TimelapseRecording") +
                                 "</td><td><input type=\"checkbox\" onclick=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'timelapseon',this.checked)\" " + strChecked + "/></td></tr>";
                        html += "<tr><td>" + LocRm.GetString("Movie") +
                                 "</td><td><input style=\"width:50px\" type=\"text\" value=\"" +
                                 cw.Camobject.recorder.timelapse + "\" onblur=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'timelapse',this.value)\"/> " +
                                 LocRm.GetString("savesAFrameToAMovieFileNS") + "</td></tr>";
                        html += "<tr><td>" + LocRm.GetString("Images") +
                                 "</td><td><input style=\"width:50px\" type=\"text\" value=\"" +
                                 cw.Camobject.recorder.timelapseframes + "\" onblur=\"send_changesetting(" + otid + "," +
                                 oid + "," + port + ",'timelapseframes',this.value)\"/> " +
                                 LocRm.GetString("savesAFrameEveryNSecondsn") + "</td></tr>";

                        html += "</table>";
                        resp += html.Replace("\"", "\\\"");
                    }
                    break;
                case "getcmdlist":
                    resp = MainForm.RemoteCommands.Aggregate(resp, (current, rc) => current + (rc.id + "|" + rc.name.Replace("|", " ").Replace(",", " ") + ","));
                    resp = resp.Trim(',');
                    break;
            }
            if (func!="")
                resp = func.Replace("result", "\"" + resp + "\"");
            return resp;
        }

        private static string GetDirectory(int objectTypeId, int objectId)
        {
            if (objectTypeId == 1)
            {
                return MainForm.Microphones.Single(p => p.id == objectId).directory;
            }
            return MainForm.Cameras.Single(p => p.id == objectId).directory;
        }

        private static void GetDirectoryPath(String sRequest, String sMyWebServerRoot, out String sLocalDir,
                                             out String sDirName)
        {
            sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);
            if (sDirName == "/")
                sLocalDir = sMyWebServerRoot;
            else
            {
                if (sDirName.ToLower().StartsWith(@"/video/"))
                {
                    sLocalDir = MainForm.Conf.MediaDirectory + "video\\";
                    string sfile = sRequest.Substring(sRequest.LastIndexOf("/") + 1);
                    int iind = Convert.ToInt32(sfile.Substring(0, sfile.IndexOf("_")));
                    sLocalDir += GetDirectory(2, iind) + "\\";
                    if (sfile.Contains(".jpg"))
                        sLocalDir += "thumbs\\";
                }
                else
                {
                    if (sDirName.ToLower().StartsWith(@"/audio/"))
                    {
                        sLocalDir = MainForm.Conf.MediaDirectory + "audio\\";
                        string sfile = sRequest.Substring(sRequest.LastIndexOf("/") + 1);
                        int iind = Convert.ToInt32(sfile.Substring(0, sfile.IndexOf("_")));
                        sLocalDir += GetDirectory(1, iind) + "\\";
                    }
                    else
                        sLocalDir = sMyWebServerRoot + sDirName.Replace("/", @"\");
                }
            }
        }

        private void ParseMimeType(String sRequestedFile, out string sFileName, out String sMimeType)
        {
            sFileName = sRequestedFile;


            if (sFileName.IndexOf("?") != -1)
                sFileName = sFileName.Substring(0, sFileName.IndexOf("?"));
            if (sFileName.IndexOf("&") != -1)
                sFileName = sFileName.Substring(0, sFileName.IndexOf("&"));
            
            sMimeType = GetMimeType(sFileName);
            if (sMimeType=="")
                sMimeType = "text/javascript";
        }

        private static bool CheckAuth(String sPhysicalFilePath)
        {
            string auth = "";
            if (sPhysicalFilePath.IndexOf("auth=") != -1)
            {
                auth = sPhysicalFilePath.Substring(sPhysicalFilePath.IndexOf("auth=") + 5).Trim('\\');
            }

            if (auth.IndexOf("&") != -1)
                auth = auth.Substring(0, auth.IndexOf("&"));
            if (auth.IndexOf("?") != -1)
                auth = auth.Substring(0, auth.IndexOf("?"));
            if (auth.IndexOf("/") != -1)
                auth = auth.Substring(0, auth.IndexOf("/"));
            if (auth.IndexOf("\\") != -1)
                auth = auth.Substring(0, auth.IndexOf("\\"));

            return auth == MainForm.Identifier;
        }

        private void SendLogFile(string sHttpVersion, ref Socket mySocket)
        {
            var fi = new FileInfo(Program.AppDataPath + "log_" + MainForm.NextLog + ".htm");
            int iTotBytes = Convert.ToInt32(fi.Length);
            var fs =
                new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

            var reader = new BinaryReader(fs);

            var bytes = new byte[iTotBytes];
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            bytes = reader.ReadBytes(bytes.Length);


            reader.Close();
            fs.Close();

            SendHeader(sHttpVersion, "text/html", iTotBytes, " 200 OK", 20, ref mySocket);
            SendToBrowser(bytes, mySocket);
        }

        private void SendLiveFeed(String sPhysicalFilePath, string sHttpVersion, ref Socket mySocket)
        {
            string cameraId = GetVar(sPhysicalFilePath, "oid");
            var imageStream = new MemoryStream();
            try
            {
                CameraWindow cw = _parent.GetCameraWindow(Convert.ToInt32(cameraId));
                if (cw==null)
                {
                    var ms = new MemoryStream();
                    Properties.Resources.camremoved.Save(ms,ImageFormat.Jpeg);
                    var imageContent = new Byte[ms.Length];
                    ms.Position = 0;
                    // load the byte array with the image
                    ms.Read(imageContent, 0, (int)ms.Length);

                    // rewind the memory stream
                    SendHeader(sHttpVersion, ".jpg", imageContent.Length, " 200 OK", 0, ref mySocket);

                    SendToBrowser(imageContent, mySocket);
                }
                else
                {
                    if (!cw.Camobject.settings.active)
                    {
                        var ms = new MemoryStream();
                        Properties.Resources.camoff.Save(ms, ImageFormat.Jpeg);
                        var imageContent = new Byte[ms.Length];
                        ms.Position = 0;
                        // load the byte array with the image
                        ms.Read(imageContent, 0, (int)ms.Length);

                        // rewind the memory stream
                        SendHeader(sHttpVersion, ".jpg", imageContent.Length, " 200 OK", 0, ref mySocket);

                        SendToBrowser(imageContent, mySocket);
                    }
                    else
                    {
                        if (cw.Camera.LastFrameNull)
                        {
                            var ms = new MemoryStream();
                            Properties.Resources.camloading.Save(ms, ImageFormat.Jpeg);
                            var imageContent = new Byte[ms.Length];
                            ms.Position = 0;
                            // load the byte array with the image
                            ms.Read(imageContent, 0, (int)ms.Length);

                            // rewind the memory stream
                            SendHeader(sHttpVersion, ".jpg", imageContent.Length, " 200 OK", 0, ref mySocket);

                            SendToBrowser(imageContent, mySocket);
                        }
                        else
                        {
                            Bitmap b = cw.Camera.LastFrame;

                            int w = 320, h = 240;
                            bool done = false;
                            if (sPhysicalFilePath.IndexOf("thumb") != -1)
                            {
                                w = 96;
                                h = 72;
                            }
                            else
                            {
                                if (sPhysicalFilePath.IndexOf("full") != -1)
                                {
                                    b.Save(imageStream, ImageFormat.Jpeg);
                                    done = true;
                                }
                                else
                                {
                                    int j = sPhysicalFilePath.IndexOf("size=");
                                    if (j != -1)
                                    {
                                        string size = sPhysicalFilePath.Substring(j + 5);
                                        size = size.Substring(0, size.IndexOf("&"));
                                        string[] wh = size.Split('x');
                                        if (wh.Length == 2)
                                        {
                                            int.TryParse(wh[0].Trim(), out w);
                                            int.TryParse(wh[1].Trim(), out h);
                                        }
                                    }
                                }
                            }

                            if (!done)
                            {
                                Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
                                Image myThumbnail = b.GetThumbnailImage(w, h, myCallback, IntPtr.Zero);

                                // put the image into the memory stream

                                myThumbnail.Save(imageStream, ImageFormat.Jpeg);
                                myThumbnail.Dispose();
                            }


                            // make byte array the same size as the image

                            var imageContent = new Byte[imageStream.Length];
                            imageStream.Position = 0;
                            // load the byte array with the image
                            imageStream.Read(imageContent, 0, (int)imageStream.Length);

                            // rewind the memory stream


                            SendHeader(sHttpVersion, ".jpg", (int)imageStream.Length, " 200 OK", 0, ref mySocket);

                            SendToBrowser(imageContent, mySocket);
                            b.Dispose();
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Server Error (livefeed): " + ex.Message);
                MainForm.LogExceptionToFile(ex);
            }
            imageStream.Dispose();
        }

        private void SendImage(String sPhysicalFilePath, string sHttpVersion, ref Socket mySocket)
        {
            int oid = Convert.ToInt32(GetVar(sPhysicalFilePath, "oid"));
            string fn = GetVar(sPhysicalFilePath, "fn");
            var imageStream = new MemoryStream();
            try
            {
                CameraWindow cw = _parent.GetCameraWindow(Convert.ToInt32(oid));
                if (cw == null)
                {
                    var ms = new MemoryStream();
                    Properties.Resources.camremoved.Save(ms, ImageFormat.Jpeg);
                    var imageContent = new Byte[ms.Length];
                    ms.Position = 0;
                    // load the byte array with the image
                    ms.Read(imageContent, 0, (int)ms.Length);

                    // rewind the memory stream
                    SendHeader(sHttpVersion, ".jpg", imageContent.Length, " 200 OK", 0, ref mySocket);

                    SendToBrowser(imageContent, mySocket);
                }
                else
                {
                    string sFileName = MainForm.Conf.MediaDirectory + "Video/" + cw.Camobject.directory +
                                       "/thumbs/" + fn;

                    var fs =
                        new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    // Create a reader that can read bytes from the FileStream.

                    var reader = new BinaryReader(fs);

                    var bytes = new byte[fs.Length];
                    while ((reader.Read(bytes, 0, bytes.Length)) != 0)
                    {
                    }
                    SendHeader(sHttpVersion, ".jpg", bytes.Length, " 200 OK", 30, ref mySocket);

                    SendToBrowser(bytes, mySocket);
                }

            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Server Error (livefeed): " + ex.Message);
                MainForm.LogExceptionToFile(ex);
            }
            imageStream.Dispose();
        }

        private void SendFloorPlanFeed(String sPhysicalFilePath, string sHttpVersion, ref Socket mySocket)
        {
            string t = sPhysicalFilePath.Substring(sPhysicalFilePath.IndexOf("floorplanid=") + 12);
            string floorplanid = t.Substring(0, t.IndexOf("&"));
            var imageStream = new MemoryStream();
            try
            {
                FloorPlanControl fpc = _parent.GetFloorPlan(Convert.ToInt32(floorplanid));
                if (fpc == null)
                {
                    var ms = new MemoryStream();
                    Properties.Resources.camremoved.Save(ms, ImageFormat.Jpeg);
                    var imageContent = new Byte[ms.Length];
                    ms.Position = 0;
                    // load the byte array with the image
                    ms.Read(imageContent, 0, (int)ms.Length);

                    // rewind the memory stream
                    SendHeader(sHttpVersion, ".jpg", imageContent.Length, " 200 OK", 0, ref mySocket);

                    SendToBrowser(imageContent, mySocket);
                }
                else
                {
                    if (fpc.ImgPlan==null)
                    {
                        var ms = new MemoryStream();
                        Properties.Resources.camloading.Save(ms, ImageFormat.Jpeg);
                        var imageContent = new Byte[ms.Length];
                        ms.Position = 0;
                        // load the byte array with the image
                        ms.Read(imageContent, 0, (int)ms.Length);

                        // rewind the memory stream
                        SendHeader(sHttpVersion, ".jpg", imageContent.Length, " 200 OK", 0, ref mySocket);

                        SendToBrowser(imageContent, mySocket);
                    }
                    else
                    {
                        int w = 320, h = 240;
                        bool done = false;
                        if (sPhysicalFilePath.IndexOf("thumb") != -1)
                        {
                            w = 96;
                            h = 72;
                        }
                        else
                        {
                            if (sPhysicalFilePath.IndexOf("full") != -1)
                            {
                                fpc.ImgView.Save(imageStream, ImageFormat.Jpeg);
                                done = true;
                            }
                            else
                            {
                                int j = sPhysicalFilePath.IndexOf("size=");
                                if (j != -1)
                                {
                                    string size = sPhysicalFilePath.Substring(j + 5);
                                    size = size.Substring(0, size.IndexOf("&"));
                                    string[] wh = size.Split('x');
                                    if (wh.Length == 2)
                                    {
                                        int.TryParse(wh[0].Trim(), out w);
                                        int.TryParse(wh[1].Trim(), out h);
                                    }
                                }
                            }
                        }

                        if (!done)
                        {
                            Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
                            Image myThumbnail = fpc.ImgView.GetThumbnailImage(w, h, myCallback, IntPtr.Zero);

                            // put the image into the memory stream

                            myThumbnail.Save(imageStream, ImageFormat.Jpeg);
                            myThumbnail.Dispose();
                        }


                        // make byte array the same size as the image

                        var imageContent = new Byte[imageStream.Length];
                        imageStream.Position = 0;
                        // load the byte array with the image
                        imageStream.Read(imageContent, 0, (int)imageStream.Length);

                        // rewind the memory stream


                        SendHeader(sHttpVersion, ".jpg", (int)imageStream.Length, " 200 OK", 0, ref mySocket);

                        SendToBrowser(imageContent, mySocket);
                    }
                }

            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Server Error (livefeed): " + ex.Message);
                MainForm.LogExceptionToFile(ex);
            }
            imageStream.Dispose();
        }

        private void SendAudioFeed(String sBuffer, String sPhysicalFilePath, ref Socket mySocket)
        {
            string t = sPhysicalFilePath.Substring(sPhysicalFilePath.IndexOf("micid=") + 6);
            string micId = t.Substring(0, t.IndexOf("&"));
            try
            {
                VolumeLevel vl = _parent.GetVolumeLevel(Convert.ToInt32(micId));
                if (vl.Micobject.settings.active)
                {
                    String sResponse = "";

                    sResponse += "HTTP/1.1 200 OK\r\n";
                    sResponse += "Server: iSpy\r\n";

                    bool sendend = false;

                    int iStartBytes = 0;
                    if (sBuffer.IndexOf("Range: bytes=") != -1)
                    {
                        var headers = sBuffer.Split(Environment.NewLine.ToCharArray());
                        for (int index = 0; index < headers.Length; index++)
                        {
                            string h = headers[index];
                            if (h.StartsWith("Range:"))
                            {
                                string[] range = (h.Substring(h.IndexOf("=") + 1)).Split('-');
                                iStartBytes = Convert.ToInt32(range[0]);
                                break;
                            }
                        }
                    }
                    if (iStartBytes != 0)
                    {
                        sendend = true;
                    }


                    //SendHeader(sHttpVersion, "audio/wav", -1, " 200 OK", 0, ref mySocket);

                    if (vl.BroadcastSocket != null && vl.BroadcastSocket.Connected)
                    {
                        vl.CloseStream = true;
                        int i = 0;
                        while (vl.CloseStream && i < 5)
                        {
                            Thread.Sleep(250);
                            i++;
                        }
                        if (vl.BroadcastSocket.Connected)
                            vl.BroadcastSocket.Disconnect(false);
                    }


                    sResponse += "Content-Type: audio/x-wav\r\n";
                    sResponse += "Transfer-Encoding: chunked\r\n";
                    sResponse += "Connection: close\r\n";
                    sResponse += "\r\n";

                    Byte[] bSendData = Encoding.ASCII.GetBytes(sResponse);

                    SendToBrowser(bSendData, mySocket);

                    if (sendend)
                    {
                        SendToBrowser(Encoding.ASCII.GetBytes(0.ToString("X") + "\r\n"), mySocket);
                    }
                    else
                    {
                        vl.BroadcastSocket = mySocket;
                        _socketindex++;
                        vl.CloseStream = false;
                        vl.DataAvailable -= VlDataAvailable;
                        vl.BroadcastStream = new MemoryStream();
                        vl.BroadcastWriter = new WaveFileWriter(vl.BroadcastStream, vl.RecordingFormat);
                        vl.DataAvailable += VlDataAvailable;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Server Error (livefeed): " + ex.Message);
                MainForm.LogExceptionToFile(ex);
            }
        }

        private void VlDataAvailable(object sender, NewDataAvailableArgs eventArgs)
        {
            var vl = (VolumeLevel) sender;
            if (vl.BroadcastSocket.Connected)
            {
                vl.BroadcastWriter.WriteData(eventArgs.DecodedData, 0, eventArgs.DecodedData.Length);
                var bout = new byte[(int) vl.BroadcastStream.Length];
                vl.BroadcastStream.Seek(0, SeekOrigin.Begin);
                vl.BroadcastStream.Read(bout, 0, (int) vl.BroadcastStream.Length);
                vl.BroadcastStream.SetLength(0);
                vl.BroadcastStream.Seek(0, SeekOrigin.Begin);

                Byte[] bSendData = Encoding.ASCII.GetBytes(bout.Length.ToString("X") + "\r\n");

                SendToBrowser(bSendData, vl.BroadcastSocket);
                SendToBrowser(bout, vl.BroadcastSocket);
                bSendData = Encoding.ASCII.GetBytes("\r\n");
                SendToBrowser(bSendData, vl.BroadcastSocket);
                if (vl.CloseStream)
                {
                    vl.CloseStream = false;
                    SendToBrowser(Encoding.ASCII.GetBytes(0.ToString("X") + "\r\n"), vl.BroadcastSocket);
                    vl.BroadcastSocket.Disconnect(false);
                }
            }
            else
            {
                vl.DataAvailable -= VlDataAvailable;
                vl.BroadcastSocket.Close();
                vl.BroadcastWriter.Close();
                vl.BroadcastStream.Dispose();
                vl.BroadcastWriter.Dispose();
                vl.BroadcastStream = null;
            }
        }

        public string FormatBytes(long bytes)
        {
            const int scale = 1024;
            var orders = new[] {"GB", "MB", "KB", "Bytes"};
            var max = (long) Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:##.##} {1}",
                                         decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        internal string GetObjectList()
        {
            string resp = "";
            if (MainForm.Cameras != null)
            {
                foreach (objectsCamera oc in MainForm.Cameras)
                {
                    CameraWindow cw = _parent.GetCameraWindow(oc.id);
                    if (cw != null)
                    {
                        bool onlinestatus = true;
                        if (!oc.settings.active || cw.VideoSourceErrorState)
                        {
                            onlinestatus = false;
                        }
                        resp += "2," + oc.id + "," + onlinestatus.ToString().ToLower() + "," +
                                oc.name.Replace(",", "&comma;") + "," + GetStatus(onlinestatus) + "," +
                                oc.description.Replace(",", "&comma;").Replace("\n", " ") + "," +
                                oc.settings.accessgroups.Replace(",", "&comma;").Replace("\n", " ") +
                                Environment.NewLine;
                    }
                }
            }
            if (MainForm.Microphones != null)
            {
                for (int index = 0; index < MainForm.Microphones.Count; index++)
                {
                    objectsMicrophone om = MainForm.Microphones[index];
                    resp += "1," + om.id + "," + om.settings.active.ToString().ToLower() + "," +
                            om.name.Replace(",", "&comma;") + "," + GetStatus(om.settings.active) + "," +
                            om.description.Replace(",", "&comma;").Replace("\n", " ") + "," +
                            om.settings.accessgroups.Replace(",", "&comma;").Replace("\n", " ") + Environment.NewLine;
                }
            }

            resp += "OK";
            return resp;
        }

        internal static string GetStatus(bool active)
        {
            string sts = "Online";
            if (!active)
            {
                sts = "Offline";
            }
            return sts;
        }
    }
}