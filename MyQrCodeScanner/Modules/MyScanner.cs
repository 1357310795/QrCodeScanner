using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public MyResult(result_status status)
        {
            this.status = status;
            this.data = new List<CodeWithLocation>();
            this.haslocation = false;
        }

        public MyResult(result_status status, ZBar.Symbol data)
        {
            this.status = status;
            this.data = new List<CodeWithLocation>();
            this.data.Add(new CodeWithLocation(data.Data, data.Type.ToString(), data.Location));
            this.haslocation = false;
        }

        public MyResult(result_status status, List<CodeWithLocation> data)
        {
            this.status = status;
            this.data = data;
            this.haslocation = false;
        }
        public MyResult(result_status status, string data)
        {
            this.status = status;
            this.data = new List<CodeWithLocation>();
            this.data.Add(new CodeWithLocation(data,"QRCODE",new List<System.Windows.Point>() { new System.Windows.Point(0, 0)}));
            this.haslocation = false;
        }
    }

    public class MyScanner
    {
        public static string method = "";

        public static MyResult ScanCode(Bitmap img)
        {
            if (method=="")
                method = IniHelper.GetKeyValue("main", "decodelib", "Zbar", IniHelper.inipath);

            try
            {
                if (method == "Zxing")
                    return DecodeByZxing(img);
                else if (method == "Zbar")
                    return DecodeByZbar(img);
                else
                    throw new Exception("找不到方法");
            }
            catch(Exception ex)
            {
                return new MyResult(result_status.error, ex.ToString());
                Console.WriteLine(ex);
            }
        }

        public static MyResult ScanCode(System.Windows.Media.Imaging.BitmapSource img)
        {
            if (method == "")
                method = IniHelper.GetKeyValue("main", "decodelib", "Zbar", IniHelper.inipath);

            try
            {
                if (method == "Zxing")
                    return DecodeByZxing(img);
                else if (method == "Zbar")
                    return DecodeByZbar(img);
                else
                    throw new Exception("找不到方法");
            }
            catch (Exception ex)
            {
                return new MyResult(result_status.error, ex.ToString());
                Console.WriteLine(ex);
            }
        }

        private static MyResult DecodeByZxing(Bitmap img)
        {
            if (img == null)
            {
                return new MyResult(result_status.nocode, "");
            }
            ZXing.BarcodeReader reader = new ZXing.BarcodeReader();

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
                return new MyResult(result_status.ok, result.Text);
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
                return new MyResult(result_status.ok, result.Text);
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
                scanner.SetConfiguration(ZBar.SymbolType.CODE39, ZBar.Config.Enable, 1);
                scanner.SetConfiguration(ZBar.SymbolType.CODE128, ZBar.Config.Enable, 1);
                scanner.SetConfiguration(ZBar.SymbolType.QRCODE, ZBar.Config.Enable, 1);

                List<ZBar.Symbol> symbols = new List<ZBar.Symbol>();
                symbols = scanner.Scan(pImg);

                if (symbols != null && symbols.Count > 0)
                {
                    var tmp = new MyResult(result_status.ok);
                    tmp.haslocation = true;
                    foreach (ZBar.Symbol symbol in symbols)
                        tmp.data.Add(new CodeWithLocation(symbol.Data,symbol.Type.ToString(), symbol.Location));
                    return tmp;
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
                scanner.SetConfiguration(ZBar.SymbolType.CODE39, ZBar.Config.Enable, 1);
                scanner.SetConfiguration(ZBar.SymbolType.CODE128, ZBar.Config.Enable, 1);
                scanner.SetConfiguration(ZBar.SymbolType.QRCODE, ZBar.Config.Enable, 1);

                List<ZBar.Symbol> symbols = new List<ZBar.Symbol>();
                symbols = scanner.Scan(pImg);

                if (symbols != null && symbols.Count > 0)
                {
                    var tmp = new MyResult(result_status.ok);
                    tmp.haslocation = true;
                    foreach (ZBar.Symbol symbol in symbols)
                        tmp.data.Add(new CodeWithLocation(symbol.Data,symbol.Type.ToString(), symbol.Location));
                    return tmp;
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
                    break;
                case 1:
                    return points[0];
                    break;
                case 2:
                    return new System.Windows.Point((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);
                    break;
                default:
                    return GeometryCenter(points);
                    break;
            }
        }

        private static System.Windows.Point GeometryCenter(IReadOnlyList<System.Windows.Point> points)
        {
            var center = new System.Windows.Point(points.Average(x => x.X), points.Average(x => x.Y));
            return center;
        }
    }

}