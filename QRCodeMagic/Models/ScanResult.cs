using OpenCvSharp;
using QRCodeMagic.Helpers;
using QRCodeMagic.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace QRCodeMagic.Models
{
    public enum ScanResultState
    {
        OK,
        NoCode,
        Error
    }
    public class CodeWithLocation
    {
        public string Type { get; set; }
        public string Data { get; set; }
        public System.Windows.Point Center { get; set; }
        public List<System.Windows.Point> Points { get; set; }

        public CodeWithLocation(string data, string type, List<System.Windows.Point> points)
        {
            this.Type = type;
            this.Data = data;
            this.Points = points;
            this.Center = GeometryHelper.Center(points);
        }
    }
    public class ScanResult
    {
        public ScanResultState State { get; set; }
        public string Message { get; set; }
        public List<CodeWithLocation> Data { get; set; }
        public bool HasLocation { get; set; }

        //Zbar multi
        public ScanResult(ScanResultState status, List<ZBar.Symbol> results)
        {
            this.State = status;
            this.HasLocation = true;
            this.Data = new List<CodeWithLocation>();
            foreach (ZBar.Symbol symbol in results)
                Data.Add(new CodeWithLocation(symbol.Data, symbol.Type.ToString(), symbol.Location.Select(p => new System.Windows.Point(p.X, p.Y)).ToList()));
            if (GlobalSettings.ignoreDup)
                CheckDup();
        }

        //OpenCV multi
        public ScanResult(ScanResultState status, Mat[] rects, string[] strs)
        {
            this.State = status;
            this.HasLocation = true;
            this.Data = new List<CodeWithLocation>();
            for (int i = 0; i < rects.Length; i++)
                Data.Add(new CodeWithLocation(strs[i], "CODE",
                    new List<System.Windows.Point>() {
                        new System.Windows.Point(rects[i].Get<float>(0,0), rects[i].Get<float>(0,1)),
                        new System.Windows.Point(rects[i].Get<float>(1,0), rects[i].Get<float>(1,1)),
                        new System.Windows.Point(rects[i].Get<float>(2,0), rects[i].Get<float>(2,1)),
                        new System.Windows.Point(rects[i].Get<float>(3,0), rects[i].Get<float>(3,1)),
                    }));
            if (GlobalSettings.ignoreDup)
                CheckDup();
        }

        //ZXing multi
        public ScanResult(ScanResultState status, ZXing.Result[] results)
        {
            this.State = status;
            this.HasLocation = true;
            this.Data = new List<CodeWithLocation>();
            foreach (ZXing.Result result in results)
                Data.Add(new CodeWithLocation(
                    result.Text, 
                    result.BarcodeFormat.ToString(), 
                    result.ResultPoints.Select(p => new System.Windows.Point(p.X, p.Y)).ToList()
                    ));
            if (GlobalSettings.ignoreDup)
                CheckDup();
        }

        //Zxing single
        public ScanResult(ScanResultState status, ZXing.Result data)
        {
            this.State = status;
            this.Data = new List<CodeWithLocation>();
            this.Data.Add(new CodeWithLocation(
                data.Text, 
                data.BarcodeFormat.ToString(), 
                data.ResultPoints.Select(p => new System.Windows.Point(p.X, p.Y)).ToList()
                ));
            this.HasLocation = true;
        }

        //Error or NoCode
        public ScanResult(ScanResultState status, string message)
        {
            this.State = status;
            this.Message = message;
            this.HasLocation = false;
        }

        private void CheckDup()
        {
            HashSet<string> h = new HashSet<string>();
            List<CodeWithLocation> newlist = new List<CodeWithLocation>();
            foreach (var code in Data)
            {
                if (!h.Contains(code.Data))
                {
                    h.Add(code.Data);
                    newlist.Add(code);
                }
            }
            Data = newlist;
        }
    }
}
