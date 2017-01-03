using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using tesseract;
using Image = System.Drawing.Image;

namespace iSpyApplication
{
    public class ANPR
    {
        private TesseractProcessor _ocr = null;
        public bool OCRInited = false;
        public TesseractProcessor OCR
        {
            get
            {
                if (_ocr != null)
                    return _ocr;
                try
                {
                    _ocr = new TesseractProcessor();
                    _ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
                    _ocr.Init(Program.AppPath + "tessdata\\", "eng", (int) Enums.EOcrEngineMode.TesseractOnly);
                    _ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
                    OCRInited = true;
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                }
                return _ocr;
            }
        }

        private static UnmanagedImage Reprocess(UnmanagedImage source)
        {
            var threshold = new OtsuThreshold();
            var thresholded = threshold.Apply(source);

            const int offset = 23;

            var blobCounter = new BlobCounter
                                          {
                                              ObjectsOrder = ObjectsOrder.Area,
                                              MaxHeight = source.Height - offset,
                                              MaxWidth = source.Width - offset,
                                              MinHeight = 14,
                                              MinWidth = 70

                                          };
            blobCounter.ProcessImage(thresholded);
            Blob[] blobs = blobCounter.GetObjectsInformation();


            var shapeChecker = new SimpleShapeChecker();
            foreach (var b in blobs)
            {
                List<IntPoint> corners;
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(b);


                shapeChecker.IsQuadrilateral(edgePoints, out corners);
                if (corners.Count == 4)
                {
                    var topLeft = corners.OrderBy(p => p.X).Take(2).OrderBy(p => p.Y).First();
                    var topRight = corners.OrderByDescending(p => p.X).Take(2).OrderBy(p => p.Y).First();
                    var bottomRight = corners.OrderByDescending(p => p.X).Take(2).OrderByDescending(p => p.Y).First();
                    var bottomLeft = corners.OrderBy(p => p.X).Take(2).OrderByDescending(p => p.Y).First();


                    //parallellize
                    if (topLeft.X < bottomLeft.X)
                        bottomLeft.X = topLeft.X;
                    else
                        topLeft.X = bottomLeft.X;

                    if (topRight.X > bottomRight.X)
                        bottomRight.X = topRight.X;
                    else
                        topRight.X = bottomRight.X;

                    var orderedcorners = new List<IntPoint>
                                             {
                                                 topLeft,
                                                 topRight,
                                                 bottomRight,
                                                 bottomLeft
                                             };

                    var fltr = new QuadrilateralTransformation(orderedcorners);// {UseInterpolation = true};
                    if (fltr.NewWidth > 80 && fltr.NewHeight > 12)
                    {
                        // apply the filter
                        var extract = fltr.Apply(thresholded);
                        return extract;
                    }
                    return source;

                }
            }
            return source;

        }

        public string ExtractNumberplate(Bitmap img, out Rectangle platearea, string area)
        {
            if (!string.IsNullOrEmpty(area))
            {
                var vals = area.Split(',');
                var arw = (img.Width / 100d);
                var arh = (img.Height / 100d);
                var r = new Rectangle(
                    (int)arw * (Convert.ToInt32(vals[0])),
                    (int)arh * (Convert.ToInt32(vals[1])),
                    (int)arw * (Convert.ToInt32(vals[2])),
                    (int)arh * (Convert.ToInt32(vals[3]))
                    );
                var c = new Crop(r);
                img = c.Apply(img);
            }
            platearea = Rectangle.Empty;

            Image ocrResult = new Bitmap(90, 1000);
            Graphics gResult = Graphics.FromImage(ocrResult);
            gResult.Clear(Color.White);


            UnmanagedImage grayImage = UnmanagedImage.Create(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            Grayscale.CommonAlgorithms.BT709.Apply(UnmanagedImage.FromManagedImage(img), grayImage);

            var threshold = new OtsuThreshold();
            var thresholded = threshold.Apply(grayImage);

            var ced = new CannyEdgeDetector();
            var edged = ced.Apply(thresholded);

            //find the plate


            var blobCounter = new BlobCounter
            {
                MinHeight = 14,
                MinWidth = 70,
                CoupledSizeFiltering = false,
                FilterBlobs = true,
                ObjectsOrder = ObjectsOrder.Area
            };

            blobCounter.ProcessImage(edged);
            Blob[] bloblist = blobCounter.GetObjectsInformation();

            List<Blob> blobs = bloblist.OrderBy(p => p.Area).ToList();
            var shapeChecker = new SimpleShapeChecker();

            foreach (var b in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(b);
                List<IntPoint> corners;
                shapeChecker.IsQuadrilateral(edgePoints, out corners);
                if (corners.Count == 4)
                {
                    //Drawing.Polygon(img, corners, Color.Green);

                    double whRatio = (double)b.Rectangle.Width / b.Rectangle.Height;
                    //check ratio - is this a lengthways rectangle shape?
                    if (3 < whRatio && whRatio < 10.0)
                    {
                        Rectangle r = b.Rectangle;
                        r.Inflate(3, 3);
                        var crop = new Crop(r);

                        var extract = crop.Apply(grayImage);

                        if (extract.Width > 300)
                        {
                            var ar = 300d / Convert.ToDouble(extract.Width);
                            var rz = new ResizeBicubic(300, Convert.ToInt32(ar * extract.Height));
                            extract = rz.Apply(extract);
                        }

                        extract = Reprocess(extract);

                        var inv = new Invert();
                        inv.ApplyInPlace(extract);

                        double zoom = 75d / extract.Height;
                        int shapes = 0;
                        int nw = Convert.ToInt32(extract.Width*zoom);
                        int nh = 75;
                        if (nw > 120)
                        {
                            var rz2 = new ResizeBicubic(nw, nh);
                            extract = rz2.Apply(extract);


                            var charFinder = new BlobCounter
                                                    {
                                                        MinHeight = Convert.ToInt32(extract.Height*0.6),
                                                        MinWidth = 3,
                                                        MaxWidth = extract.Width/5,
                                                        ObjectsOrder = ObjectsOrder.XY,
                                                        CoupledSizeFiltering = false,
                                                        FilterBlobs = true
                                                    };

                            charFinder.ProcessImage(extract);
                            Blob[] letters = charFinder.GetObjectsInformation();


                            int offsety = 5;
                            inv.ApplyInPlace(extract);
                            Bitmap plate = extract.ToManagedImage();
                                
                            gResult.Clear(Color.White);

                            foreach (var alpha in letters)
                            {
                                var c = new Crop(alpha.Rectangle);
                                Bitmap letter = c.Apply(plate);
                                var rect = new Rectangle(5, offsety, letter.Width, letter.Height);

                                try
                                {
                                    gResult.DrawImage(letter, rect);
                                    shapes++;
                                    offsety += letter.Height + 40;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }
                        }

                        if (shapes > 2)
                        {
                            OCR.Clear();
                            OCR.ClearResults();
                            OCR.ClearAdaptiveClassifier();

                            _ocr.Apply(ocrResult);
                            List<Word> result = _ocr.RetriveResultDetail();
                            string res = "";
                            foreach (Word ocrWord in result)
                            {
                                if (ocrWord.Confidence < 160)
                                    res += char.ConvertFromUtf32(Convert.ToInt32(ocrWord.Text));
                                else
                                {
                                    if (ocrWord.Confidence < 200)
                                        res += "*";
                                    // "= ocrWord.Text.Aggregate(res, (current, t) => current + "*");
                                }
                            }
                            gResult.Dispose();
                            return res;
                        }
                    }
                }

            }
            gResult.Dispose();
            //pbThresholded.Image = thresholded.ToManagedImage();
            return "";
        }

        public bool NumberPlateInList(string numberplate, string list, int percentAccuracy, out string matched)
        {
            var storedplates = list.Split(',');
            var dsc = new DistanceCheck(numberplate,percentAccuracy);
            foreach (string s in storedplates)
            {
                if (dsc.Check(s))
                {
                    matched = s;
                    return true;
                }
            }
            matched = "";
            return false;
        }


        public class DistanceCheck
        {
            readonly int maxDistance;
            readonly string s1;

            public DistanceCheck(string s1, int percCorrect)
            {
                maxDistance = s1.Length - (int)(s1.Length * percCorrect / 100);
                this.s1 = s1.ToLower();
            }

            public bool Check(string s2)
            {
                s2 = s2.ToLower();

                int nDiagonal = s1.Length - Math.Min(s1.Length, s2.Length);
                int mDiagonal = s2.Length - Math.Min(s1.Length, s2.Length);

                if (s1.Length == 0) return s2.Length <= maxDistance;
                if (s2.Length == 0) return s1.Length <= maxDistance;

                int[,] matrix = new int[s1.Length + 1, s2.Length + 1];

                for (int i = 0; i <= s1.Length; matrix[i, 0] = i++) ;
                for (int j = 0; j <= s2.Length; matrix[0, j] = j++) ;

                int cost;

                for (int i = 1; i <= s1.Length; i++)
                {
                    for (int j = 1; j <= s2.Length; j++)
                    {
                        if (s2.Substring(j - 1, 1) == s1.Substring(i - 1, 1))
                        {
                            cost = 0;
                        }
                        else
                        {
                            cost = 1;
                        }

                        int valueAbove = matrix[i - 1, j];
                        int valueLeft = matrix[i, j - 1] + 1;
                        int valueAboveLeft = matrix[i - 1, j - 1];
                        matrix[i, j] = Min(valueAbove + 1, valueLeft + 1, valueAboveLeft + cost);
                    }

                    if (i >= nDiagonal)
                    {
                        if (matrix[nDiagonal, mDiagonal] > maxDistance)
                        {
                            return false;
                        }
                        else
                        {
                            nDiagonal++;
                            mDiagonal++;
                        }
                    }
                }

                return true;
            }

            private int Min(int n1, int n2, int n3)
            {
                return Math.Min(n1, System.Math.Min(n2, n3));
            }
        }

    }
}
