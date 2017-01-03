using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace iSpyApplication
{
    public class PTZController
    {
        private readonly CameraWindow _cameraControl;
        private HttpWebRequest _request;
        const double Arc = Math.PI / 8;
        private string _nextcommand = "";

        public PTZController(CameraWindow cameraControl)
        {
            _cameraControl = cameraControl;
        }

        public void SendPTZDirection(double angle, int repeat)
        {
            for (int i = 0; i < repeat; i++)
            {
                SendPTZDirection(angle);
            }
        }

        public void SendPTZDirection(double angle)
        {
            if (_cameraControl.Camobject.settings.ptzrotate90)
            {
                angle -= (Math.PI/2);
                if (angle < -Math.PI)
                {
                    angle += (2*Math.PI);
                }
            }

            if (_cameraControl.Camobject.settings.ptzflipx)
            {
                if (angle <= 0)
                    angle = -Math.PI - angle;
                else
                    angle = Math.PI - angle;
            }
            if (_cameraControl.Camobject.settings.ptzflipy)
            {
                angle = angle*-1;
            }

            PTZSettingsCamera ptz = MainForm.PTZs.SingleOrDefault(q => q.id == _cameraControl.Camobject.ptz);

            string command = ptz.Commands.Center;
            string diag = "";

            if (angle < Arc && angle > -Arc)
            {
                command = ptz.Commands.Left;

            }
            if (angle >= Arc && angle < 3 * Arc)
            {
                command = ptz.Commands.LeftUp;
                diag = "leftup";
            }
            if (angle >= 3 * Arc && angle < 5 * Arc)
            {
                command = ptz.Commands.Up;
            }
            if (angle >= 5 * Arc && angle < 7 * Arc)
            {
                command = ptz.Commands.RightUp;
                diag = "rightup";
            }
            if (angle >= 7 * Arc || angle < -7 * Arc)
            {
                command = ptz.Commands.Right;
            }
            if (angle <= -5 * Arc && angle > -7 * Arc)
            {
                command = ptz.Commands.RightDown;
                diag = "rightdown";
            }
            if (angle <= -3 * Arc && angle > -5 * Arc)
            {
                command = ptz.Commands.Down;
            }
            if (angle <= -Arc && angle > -3 * Arc)
            {
                command = ptz.Commands.LeftDown;
                diag = "leftdown";
            }

            if (command=="") //some PTZ cameras dont have diagonal controls, this fixes that
            {
                switch (diag)
                {
                    case "leftup":
                        _nextcommand = ptz.Commands.Up;
                        SendPTZCommand(ptz.Commands.Left);
                        break;
                    case "rightup":
                        _nextcommand = ptz.Commands.Up;
                        SendPTZCommand(ptz.Commands.Right);
                        break;
                    case "rightdown":
                        _nextcommand = ptz.Commands.Down;
                        SendPTZCommand(ptz.Commands.Right);
                        break;
                    case "leftdown":
                        _nextcommand = ptz.Commands.Down;
                        SendPTZCommand(ptz.Commands.Left);
                        break;
                }
            }
            else
                SendPTZCommand(command);

        }

        public void SendPTZCommand(Enums.PtzCommand command)
        {
            SendPTZCommand(command,false);
        }

        public void SendPTZCommand(Enums.PtzCommand command, bool wait)
        {
            PTZSettingsCamera ptz = MainForm.PTZs.SingleOrDefault(q => q.id == _cameraControl.Camobject.ptz);
            if (ptz != null)
            {
                switch (command)
                {
                    case Enums.PtzCommand.Left:
                        SendPTZDirection(0);
                        break;
                    case Enums.PtzCommand.Upleft:
                        SendPTZDirection(Math.PI/4);
                        break;
                    case Enums.PtzCommand.Up:
                        SendPTZDirection(Math.PI / 2);
                        break;
                    case Enums.PtzCommand.UpRight:
                        SendPTZDirection(3 * Math.PI / 4);
                        break;
                    case Enums.PtzCommand.Right:
                        SendPTZDirection(Math.PI);
                        break;
                    case Enums.PtzCommand.DownRight:
                        SendPTZDirection(-3*Math.PI / 4);
                        break;
                    case Enums.PtzCommand.Down:
                        SendPTZDirection(-Math.PI / 2);
                        break;
                    case Enums.PtzCommand.DownLeft:
                        SendPTZDirection(-Math.PI / 4);
                        break;
                    case Enums.PtzCommand.ZoomIn:
                        SendPTZCommand(ptz.Commands.ZoomIn, wait);
                        break;
                    case Enums.PtzCommand.ZoomOut:
                        SendPTZCommand(ptz.Commands.ZoomOut, wait);
                        break;
                    case Enums.PtzCommand.Center:
                        SendPTZCommand(ptz.Commands.Center, wait);
                        break;
                    case Enums.PtzCommand.Stop:
                        SendPTZCommand(ptz.Commands.Stop, wait);
                        break;
                }
            }
        }
        public void SendPTZCommand(string cmd)
        {
            SendPTZCommand(cmd,false);
        }

        public void SendPTZCommand(string cmd, bool wait)
        {
            if (_request != null)
            {
                if (!wait)
                    return;
                _request.Abort();
            }
            //PTZSettingsCamera ptz = MainForm.PTZs.Single(p => p.id == _cameraControl.Camobject.ptz);
            //get URL of camera
            Uri uri;

            try
            {
                uri = new Uri(_cameraControl.Camobject.settings.videosourcestring);
            }
            catch (Exception e)
            {
                MainForm.LogExceptionToFile(e);
                return;
            }
            string baseurl = uri.AbsoluteUri.Replace(uri.AbsolutePath, "");

            string url = baseurl;

            if (!cmd.StartsWith("/"))
            {
                url += _cameraControl.Camobject.settings.ptzurlbase;

                if (cmd != "")
                {
                    string ext = "?";
                    if (url.IndexOf("?") != -1)
                        ext = "&";

                    url += ext + cmd;
                }
            }
            else
            {
                url += cmd;
            }          

            
            string un = _cameraControl.Camobject.settings.login;
            string pwd = _cameraControl.Camobject.settings.password;
            if (_cameraControl.Camobject.settings.login == string.Empty)
            {
                //get from url
                string details = url.Substring(url.IndexOf("/") + 2);
                if (details.IndexOf("@") != -1)
                {
                    string[] creds = details.Substring(0, details.IndexOf("@")).Split(':');
                    if (creds.Length == 2)
                    {
                        un = creds[0];
                        pwd = creds[1];
                    }
                }
            }
            _request = (HttpWebRequest) WebRequest.Create(url);
            _request.AllowAutoRedirect = true;
            _request.KeepAlive = true;
            _request.SendChunked = false;
            _request.AllowWriteStreamBuffering = true;
            _request.UserAgent = _cameraControl.Camobject.settings.useragent;
            //get credentials

            // set login and password
            if (un != string.Empty)
                SetBasicAuthHeader(_request, un, pwd);

            var myRequestState = new RequestState {Request = _request};
            _request.BeginGetResponse(FinishPTZRequest, myRequestState);
        }

        private void FinishPTZRequest(IAsyncResult result)
        {
            var myRequestState = (RequestState) result.AsyncState;
            WebRequest myWebRequest = myRequestState.Request;
            // End the Asynchronous request.
            try
            {
                myRequestState.Response = myWebRequest.EndGetResponse(result);
                myRequestState.Response.Close();
            }
            catch
            {
            }
            myRequestState.Response = null;
                myRequestState.Request = null;
            
            _request = null;
            if (_nextcommand!="")
            {
                string nc = _nextcommand;
                _nextcommand = "";
                SendPTZCommand(nc);
            }
        }

        public void SetBasicAuthHeader(WebRequest req, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
        }

        #region Nested type: RequestState

        public class RequestState
        {
            // This class stores the request state of the request.
            public WebRequest Request;
            public WebResponse Response;

            public RequestState()
            {
                Request = null;
                Response = null;
            }
        }

        #endregion
    }
}