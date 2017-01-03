using System;
using System.Collections.Generic;
using System.IO;

namespace iSpyApplication
{
    public static class Helper
    {
        public static double CalculateSensitivity(double percent)
        {
            const double minimum = 0.00000001;
            const double maximum = 0.1;
            return minimum + ((maximum - minimum)/100)*Convert.ToDouble(100 - percent);
        }

        public static string ZeroPad(int i)
        {
            if (i < 10)
                return "0" + i;
            return i.ToString();
        }

        public static string GetMotionDataPoints(string motionData)
        {
            string[] elements = motionData.Trim(',').Split(',');
            if (elements.Length <= 500)
                return String.Join(",", elements);
            double interval = (elements.Length / 500d);
            string newdata = "";
            int iMax = 0;
            for(double i=0;i<elements.Length;i+=interval)
            {
                int ind = (int) Math.Round(i,0);
                if (ind < elements.Length)
                {
                    int iCurrent = ind;
                    if (iCurrent > iMax)
                        iMax = iCurrent;
                    if (i%interval == 0)
                    {
                        newdata += iMax + ",";
                        iMax = 0;
                    }
                }
            }
            return newdata.Trim(',');

        }

        public static void DeleteAllContent(int objectTypeId, string directoryName)
        {
            if (objectTypeId == 1)
            {
                var lFi = new List<FileInfo>();
                var dirinfo = new DirectoryInfo(MainForm.Conf.MediaDirectory + "audio\\" +
                                              directoryName + "\\");

                lFi.AddRange(dirinfo.GetFiles());
                lFi = lFi.FindAll(f => f.Extension.ToLower() == ".wav" || f.Extension.ToLower() == ".mp3" || f.Extension == ".fla");

                foreach (FileInfo fi in lFi)
                {
                    try
                    {
                        File.Delete(fi.FullName);
                    }
                    catch
                    {
                        // Debug.WriteLine("Server Error (deleteall video): " + e.Message);
                    }
                }

            }
            if (objectTypeId == 2)
            {
                var lFi = new List<FileInfo>();
                var dirinfo = new DirectoryInfo(MainForm.Conf.MediaDirectory + "video\\" +
                                              directoryName + "\\");

                lFi.AddRange(dirinfo.GetFiles());
                lFi = lFi.FindAll(f => f.Extension.ToLower() == ".avi" || f.Extension.ToLower() == ".mp4" || f.Extension == ".flv");

                foreach (FileInfo fi in lFi)
                {
                    try
                    {
                        File.Delete(fi.FullName);
                    }
                    catch
                    {
                        // Debug.WriteLine("Server Error (deleteall video): " + e.Message);
                    }
                }
                Array.ForEach(Directory.GetFiles(MainForm.Conf.MediaDirectory + "video\\" +
                                              directoryName + "\\thumbs\\"), delegate(string path)
                                              {
                                                  try
                                                  {
                                                      File.Delete(path);
                                                  }
                                                  catch
                                                  {
                                                  }
                                              });

            }

        }
        // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
        public static double UnixTicks(this DateTime dt)
        {
            var d1 = new DateTime(1970, 1, 1);
            var d2 = dt.ToUniversalTime();
            var ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static double UnixTicks(this long ticks)
        {
            var d1 = new DateTime(1970, 1, 1);
            var d2 = new DateTime(ticks);
            var ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }
    }
}