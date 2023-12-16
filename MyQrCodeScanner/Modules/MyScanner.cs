using MyQrCodeScanner.Modules;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace MyQrCodeScanner
{
    public enum result_status
    {
        ok,
        nocode,
        error
    }
    public class CodeWithLocation
    {
        public string type;
        public string data;
        public System.Windows.Point center;
        public List<System.Windows.Point> points;

        public CodeWithLocation(string data, string type, List<System.Windows.Point> points)
        {
            this.type = type;
            this.data = data;
            this.points = points;
            this.center = GeometryHelper.Center(points);
        }
    }
    public class MyResult
    {
        public result_status status;
        public List<CodeWithLocation> data;
        public bool haslocation;

        //public MyResult(result_status status)
        //{
        //    this.status = status;
        //    this.data = new List<CodeWithLocation>();
        //    this.haslocation = false;
        //}

        public MyResult(result_status status, List<ZBar.Symbol> results)
        {
            this.status = status;
            this.haslocation = true;
            this.data = new List<CodeWithLocation>();
            foreach (ZBar.Symbol symbol in results)
                data.Add(new CodeWithLocation(symbol.Data, symbol.Type.ToString(), symbol.Location.Select(p => new System.Windows.Point(p.X, p.Y)).ToList()));
            if (GlobalSettings.ignoreDup)
                CheckDup();
        }

        public MyResult(result_status status, Mat[] rects, string[] strs)
        {
            this.status = status;
            this.haslocation = true;
            this.data = new List<CodeWithLocation>();
            for (int i = 0; i < rects.Length; i++)
                data.Add(new CodeWithLocation(strs[i], "CODE", 
                    new List<System.Windows.Point>() { 
                        new System.Windows.Point(rects[i].Get<float>(0,0), rects[i].Get<float>(0,1)),
                        new System.Windows.Point(rects[i].Get<float>(1,0), rects[i].Get<float>(1,1)),
                        new System.Windows.Point(rects[i].Get<float>(2,0), rects[i].Get<float>(2,1)),
                        new System.Windows.Point(rects[i].Get<float>(3,0), rects[i].Get<float>(3,1)),
                    }));
            if (GlobalSettings.ignoreDup)
                CheckDup();
        }

        private void CheckDup()
        {
            HashSet<string> h = new HashSet<string>();
            List<CodeWithLocation> newlist = new List<CodeWithLocation>();
            foreach (var code in data)
            {
                if (!h.Contains(code.data))
                {
                    h.Add(code.data);
                    newlist.Add(code);
                }
            }
            data = newlist;
        }

        public MyResult(result_status status, ZXing.Result[] results)
        {
            this.status = status;
            this.haslocation = true;
            this.data = new List<CodeWithLocation>();
            foreach (ZXing.Result result in results)
                data.Add(new CodeWithLocation(result.Text, result.BarcodeFormat.ToString(), ZxingPointExtensions.ZxingPointToPoint(result.ResultPoints)));
            if (GlobalSettings.ignoreDup)
                CheckDup();
        }

        public MyResult(result_status status, ZXing.Result data)
        {
            this.status = status;
            this.data = new List<CodeWithLocation>();
            this.data.Add(new CodeWithLocation(data.Text, data.BarcodeFormat.ToString(), ZxingPointExtensions.ZxingPointToPoint(data.ResultPoints)));
            this.haslocation = true;
        }

        public MyResult(result_status status, string data)
        {
            this.status = status;
            this.data = new List<CodeWithLocation>();
            this.data.Add(new CodeWithLocation(data,"CODE",new List<System.Windows.Point>() { new System.Windows.Point(0, 0)}));
            this.haslocation = false;
        }
    }

    public class MyScanner
    {
        const string _wechat_QCODE_detector_prototxt_path = "data/wechat_qrcode/detect.prototxt";
        const string _wechat_QCODE_detector_caffe_model_path = "data/wechat_qrcode/detect.caffemodel";
        const string _wechat_QCODE_super_resolution_prototxt_path = "data/wechat_qrcode/sr.prototxt";
        const string _wechat_QCODE_super_resolution_caffe_model_path = "data/wechat_qrcode/sr.caffemodel";

        public static MyResult ScanCode(Bitmap img)
        {
            var method = GlobalSettings.selectedengine;

            try
            {
                if (method == 1)
                    return DecodeByZxing(img);
                else if (method == 0)
                    return DecodeByZbar(img);
                else if (method == 2)
                    return DecodeByZxingMulti(img);
                else if (method == 3)
                    return DecodeByOpenCV(img);
                else
                    throw new Exception("找不到方法");
            }
            catch(Exception ex)
            {
                return new MyResult(result_status.error, ex.ToString());
            }
        }

        public static MyResult ScanCode(System.Windows.Media.Imaging.BitmapSource img)
        {
            var method = GlobalSettings.selectedengine;
            try
            {
                if (method == 1)
                    return DecodeByZxing(img);
                else if (method == 0)
                    return DecodeByZbar(img);
                else if (method == 2)
                    return DecodeByZxingMulti(img);
                else if (method == 3)
                    return DecodeByOpenCV(BitmapHelper.GetBitmap(img));
                else
                    throw new Exception("找不到方法");
            }
            catch (Exception ex)
            {
                return new MyResult(result_status.error, ex.ToString());
            }
        }

        private static MyResult DecodeByOpenCV(Bitmap img)
        {
            if (img == null)
            {
                return new MyResult(result_status.nocode, "");
            }
            WeChatQRCode decoder = WeChatQRCode.Create(_wechat_QCODE_detector_prototxt_path, _wechat_QCODE_detector_caffe_model_path, _wechat_QCODE_super_resolution_prototxt_path, _wechat_QCODE_super_resolution_caffe_model_path);

            Mat[] rects;
            string[] texts;
            try
            {
                Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img);
                decoder.DetectAndDecode(mat, out rects, out texts);
                mat.Dispose();
            }
            catch (ZXing.ReaderException ex)
            {
                //MessageBox.Show(resultstr, "内部错误");
                return new MyResult(result_status.error, ex.ToString());
            }

            if (rects.Length != 0)
            {
                return new MyResult(result_status.ok, rects, texts);
            }
            else
                return new MyResult(result_status.nocode, "");
        }

        private static MyResult DecodeByZxing(Bitmap img)
        {
            if (img == null)
            {
                return new MyResult(result_status.nocode, "");
            }
            ZXing.BarcodeReader reader = new ZXing.BarcodeReader();
            reader.AutoRotate = true;

            ZXing.Result result = null;
            try
            {
                result = reader.Decode(img);
                img.Dispose();
            }
            catch (ZXing.ReaderException ex)
            {
                //MessageBox.Show(resultstr, "内部错误");
                return new MyResult(result_status.error, ex.ToString());
            }

            if (result != null)
            {
                return new MyResult(result_status.ok, result);
            }
            else
                return new MyResult(result_status.nocode,"");
        }

        private static MyResult DecodeByZxing(System.Windows.Media.Imaging.BitmapSource img)
        {
            if (img == null)
            {
                return new MyResult(result_status.nocode, "");
            }
            ZXing.Presentation.BarcodeReader reader = new ZXing.Presentation.BarcodeReader();
            reader.AutoRotate = true;

            ZXing.Result result = null;
            try
            {
                result = reader.Decode(img);
                
            }
            catch (ZXing.ReaderException ex)
            {
                //MessageBox.Show(resultstr, "内部错误");
                return new MyResult(result_status.error, ex.ToString());
            }

            if (result != null)
            {
                return new MyResult(result_status.ok, result);
            }
            else
                return new MyResult(result_status.nocode, "");
        }

        private static MyResult DecodeByZxingMulti(Bitmap img)
        {
            if (img == null)
            {
                return new MyResult(result_status.nocode, "");
            }
            ZXing.BarcodeReader reader = new ZXing.BarcodeReader();
            reader.AutoRotate = true;

            ZXing.Result[] results = null;
            try
            {
                results = reader.DecodeMultiple(img);
                img.Dispose();
            }
            catch (ZXing.ReaderException ex)
            {
                //MessageBox.Show(resultstr, "内部错误");
                return new MyResult(result_status.error, ex.ToString());
            }

            if (results != null && results.Length > 0)
            {
                return new MyResult(result_status.ok, results);
            }
            else
                return new MyResult(result_status.nocode, "");
        }

        private static MyResult DecodeByZxingMulti(System.Windows.Media.Imaging.BitmapSource img)
        {
            if (img == null)
            {
                return new MyResult(result_status.nocode, "");
            }
            ZXing.Presentation.BarcodeReader reader = new ZXing.Presentation.BarcodeReader();
            reader.AutoRotate = true;

            ZXing.Result[] results = null;
            try
            {
                results = reader.DecodeMultiple(img);
            }
            catch (ZXing.ReaderException ex)
            {
                //MessageBox.Show(resultstr, "内部错误");
                return new MyResult(result_status.error, ex.ToString());
            }

            if (results != null && results.Length > 0)
            {
                return new MyResult(result_status.ok, results);
            }
            else
                return new MyResult(result_status.nocode, "");
        }

        private static MyResult DecodeByZbar(Bitmap img)
        {
            Bitmap pImg = MakeGrayscale3(img);
            using (ZBar.ImageScanner scanner = new ZBar.ImageScanner())
            {
                //scanner.SetConfiguration(ZBar.SymbolType.None, ZBar.Config.Enable, 0);
                //scanner.SetConfiguration(ZBar.SymbolType.CODE39, ZBar.Config.Enable, 1);
                //scanner.SetConfiguration(ZBar.SymbolType.CODE128, ZBar.Config.Enable, 1);
                //scanner.SetConfiguration(ZBar.SymbolType.QRCODE, ZBar.Config.Enable, 1);

                List<ZBar.Symbol> symbols = new List<ZBar.Symbol>();
                symbols = scanner.Scan(pImg);
                pImg.Dispose();

                if (symbols != null && symbols.Count > 0)
                {
                    return new MyResult(result_status.ok, symbols);
                }
                else
                {
                    return new MyResult(result_status.nocode, "");
                }
            }
        }

        private static MyResult DecodeByZbar(System.Windows.Media.Imaging.BitmapSource img)
        {
            Bitmap pImg = MakeGrayscale3(BitmapHelper.GetBitmap(img));
            using (ZBar.ImageScanner scanner = new ZBar.ImageScanner())
            {
                //scanner.SetConfiguration(ZBar.SymbolType.None, ZBar.Config.Enable, 0);
                //scanner.SetConfiguration(ZBar.SymbolType.CODE39, ZBar.Config.Enable, 1);
                //scanner.SetConfiguration(ZBar.SymbolType.CODE128, ZBar.Config.Enable, 1);
                //scanner.SetConfiguration(ZBar.SymbolType.QRCODE, ZBar.Config.Enable, 1);

                List<ZBar.Symbol> symbols = new List<ZBar.Symbol>();
                symbols = scanner.Scan(pImg);
                pImg.Dispose();

                if (symbols != null && symbols.Count > 0)
                {
                    return new MyResult(result_status.ok, symbols);
                }
                else
                {
                    return new MyResult(result_status.nocode, "");
                }
            }
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(
              new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            original.Dispose();
            return newBitmap;
        }

    }

    public class GeometryHelper
    {
        public static System.Windows.Point Center(IReadOnlyList<System.Windows.Point> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            switch (points.Count)
            {
                case 0:
                    throw new ArgumentException("至少需要有一个点才能计算几何中心。", nameof(points));
                case 1:
                    return points[0];
                case 2:
                    return new System.Windows.Point((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);
                default:
                    return GeometryCenter(points);
            }
        }

        private static System.Windows.Point GeometryCenter(IReadOnlyList<System.Windows.Point> points)
        {
            var center = new System.Windows.Point(points.Average(x => x.X), points.Average(x => x.Y));
            return center;
        }
    }

    public static class ZxingPointExtensions
    {
        public static List<System.Windows.Point> ZxingPointToPoint(ZXing.ResultPoint[] rp)
        {
            List<System.Windows.Point> res=new List<System.Windows.Point>();
            foreach (ZXing.ResultPoint p in rp)
            {
                if (p != null)
                    res.Add(new System.Windows.Point(p.X, p.Y));
            }
                
            return res;
        }
    }
}