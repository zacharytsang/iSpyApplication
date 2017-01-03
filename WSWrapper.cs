using System;
using System.Timers;
using System.Windows.Forms;
using iSpyApplication.iSpyWS;
using Timer = System.Timers.Timer;

namespace iSpyApplication
{
    public class WsWrapper
    {
        private readonly Timer _reconnect;
        internal iSpy Wsa = new iSpy();
        private static string _externalIP = "";

        private bool _websitelive = true;


        public WsWrapper()
        {
            _reconnect = new Timer {Interval = 60*1000};
            _reconnect.Elapsed += ReconnectElapsed;
            //WSA.Url = "http://localhost:11145/Webservices/iSpy.asmx";
            Wsa.Url = MainForm.Website+"/webservices/ispy.asmx";
        }

        public string DownMessage
        {
            get { return LocRm.GetString("iSpyDown"); }
        }

        public string WebservicesDisabledMessage
        {
            get { return LocRm.GetString("WebservicesDisabled"); }
        }

        public bool WebsiteLive
        {
            get { return _websitelive; }
            set
            {
                _websitelive = value;
                if (!_websitelive)
                {
                    MainForm.LogErrorToFile("Disconnected");
                    if (!_reconnect.Enabled)
                        _reconnect.Start();
                }
            }
        }

        private void ReconnectElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                string s = Wsa.Ping();
                if (s == "OK")
                {
                    _reconnect.Stop();
                    MainForm.LogMessageToFile("Reconnecting...");
                    if (MainForm.Conf.ServicesEnabled)
                    {
                        try
                        {
                            s = Connect(MainForm.Conf.Loopback);
                            if (s == "OK")
                            {
                                MainForm.StopAndStartServer();
                                ForceSync(MainForm.IPAddress, MainForm.Conf.LANPort, MainForm.MWS.GetObjectList());
                            }
                            WebsiteLive = true;
                            MainForm.LogMessageToFile("Connected");
                        }
                        catch (Exception ex)
                        {
                            MainForm.LogExceptionToFile(ex);
                            _reconnect.Start();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
        }

        public string SendAlert(string emailAddress, string subject, string message)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.SendAlert2(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword,
                                        emailAddress, subject, message);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string SendContent(string emailAddress, string subject, string message)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.SendContent(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword,
                                         emailAddress, subject, message);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string SendAlertWithImage(string emailAddress, string subject, string message, byte[] imageData)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.SendAlertWithImage3(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword,
                                                 emailAddress, subject, message, imageData);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string SendFrameGrab(string emailAddress, string subject, string message, byte[] imageData)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.SendFrameGrab3(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword,
                                            emailAddress, subject, message, imageData);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        
        public string ExternalIPv4(bool refresh)
        {
            if (_externalIP != "" && !refresh)
                return _externalIP;
            if (WebsiteLive)
            {
                try
                {
                    _externalIP = Wsa.RemoteAddress();
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return _externalIP;
            }
            if (_externalIP != "")
                return _externalIP;

            return LocRm.GetString("Unavailable");
        }

        public string ProductLatestVersion(int productId)
        {
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.ProductLatestVersionGet(productId);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string SendSms(string smsNumber, string message)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.SendSMS(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, smsNumber,
                                     message);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string SendMms(string mobileNumber, string message, byte[] imageData)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                try
                {
                    r = Wsa.SendMMS2(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword,
                                      mobileNumber, message, imageData);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string ForceSync()
        {
            return ForceSync(MainForm.IPAddress, MainForm.Conf.LANPort,
                             MainForm.MWS.GetObjectList());
        }

        public string PingServer()
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            bool islive = _websitelive;
            try
            {
                int port = MainForm.Conf.ServerPort;
                if (MainForm.Conf.IPMode == "IPv6")
                    port = MainForm.Conf.LANPort;

                r = Wsa.PingAlive2(port, MainForm.Conf.IPMode == "IPv4", MainForm.IPAddressExternal);

                if (r == "OK")
                {
                    _websitelive = true;
                    if (MainForm.Conf.ServicesEnabled)
                    {
                        if (!MainForm.MWS.Running)
                        {
                            MainForm.StopAndStartServer();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _websitelive = false;
                MainForm.LogExceptionToFile(ex);
            }

            if (!islive && _websitelive)
            {
                MainForm.WSW.Connect();
                MainForm.WSW.ForceSync();
            }
            if (WebsiteLive)
                return r;

            return DownMessage;
        }

        private string ForceSync(string internalIPAddress, int internalPort, string settings)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                int port = MainForm.Conf.ServerPort;
                if (MainForm.Conf.IPMode == "IPv6")
                    port = MainForm.Conf.LANPort;
                try
                {
                    r = Wsa.ForceSync3(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, port,
                                        internalIPAddress, internalPort, settings, MainForm.Conf.IPMode == "IPv4", MainForm.IPAddressExternal);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string Disconnect()
        {

            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                int port = MainForm.Conf.ServerPort;
                if (MainForm.Conf.IPMode == "IPv6")
                    port = MainForm.Conf.LANPort;
                try
                {
                    r = Wsa.Disconnect(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, port);
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string Connect()
        {
            return Connect(MainForm.LoopBack);
        }

        public string Connect(bool tryLoopback)
        {
            if (!MainForm.Conf.ServicesEnabled)
                return WebservicesDisabledMessage;
            string r = "";
            if (WebsiteLive)
            {
                int port = MainForm.Conf.ServerPort;
                if (MainForm.Conf.IPMode == "IPv6")
                    port = MainForm.Conf.LANPort;

                try
                {
                    r = Wsa.Connect5(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, port,
                                      MainForm.Identifier, tryLoopback, Application.ProductVersion,
                                      MainForm.Conf.ServerName, MainForm.Conf.IPMode=="IPv4", MainForm.IPAddressExternal);
                    if (r == "OK" && tryLoopback)
                        MainForm.LoopBack = true;
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                    WebsiteLive = false;
                }
                if (WebsiteLive && r != "OK")
                    return LocRm.GetString(r);
                if (WebsiteLive)
                    return r;
            }
            return DownMessage;
        }

        public string[] TestConnection(string username, string password, bool tryLoopback)
        {
            var r = new string[] {};

            int port = MainForm.Conf.ServerPort;
            if (MainForm.Conf.IPMode == "IPv6")
                port = MainForm.Conf.LANPort;
            try
            {
                _websitelive = true;
                r = Wsa.TestConnection4(username, password, port, MainForm.Identifier, tryLoopback, MainForm.Conf.IPMode=="IPv4", MainForm.IPAddressExternal);
            }
            catch (Exception ex)
            {
                _websitelive = false;
                MainForm.LogExceptionToFile(ex);
            }
            if (_websitelive)
            {
                if (r.Length == 1 && r[0] != "OK") //login failed
                    r[0] = LocRm.GetString(r[0]);
                if (r.Length > 3 && r[3] != "")
                {
                    r[3] = LocRm.GetString(r[3]);
                }
                return r;
            }
            return new[] {DownMessage};
        }
    }
}