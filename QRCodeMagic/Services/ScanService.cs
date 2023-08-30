using OpenCvSharp;
using QRCodeMagic.Helpers;
using QRCodeMagic.Models;
using QRCodeMagic.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Teru.Code.Models;
using ZXing;

namespace QRCodeMagic.Services
{
    public class ScanService
    {
        const string _wechat_QCODE_detector_prototxt_path = "data/wechat_qrcode/detect.prototxt";
        const string _wechat_QCODE_detector_caffe_model_path = "data/wechat_qrcode/detect.caffemodel";
        const string _wechat_QCODE_super_resolution_prototxt_path = "data/wechat_qrcode/sr.prototxt";
        const string _wechat_QCODE_super_resolution_caffe_model_path = "data/wechat_qrcode/sr.caffemodel";

        //Zxing
        private static ZXing.BarcodeReaderGeneric zxing_reader = null;
        //ZBar
        private static ZBar.ImageScanner zbar_scanner = null;
        //Wechat OpenCV
        private static WeChatQRCode opencv_decoder = null;

        public static CommonResult IntiEngine()
        {
            try
            {
                zxing_reader = new BarcodeReaderGeneric();
                zbar_scanner = new ZBar.ImageScanner();
                opencv_decoder = WeChatQRCode.Create(_wechat_QCODE_detector_prototxt_path, _wechat_QCODE_detector_caffe_model_path, _wechat_QCODE_super_resolution_prototxt_path, _wechat_QCODE_super_resolution_caffe_model_path);
            }
            catch(Exception ex)
            {
                return new CommonResult(false, $"初始化引擎失败：\n{ex.Message}");
            }

            return new CommonResult(true, "操作成功");
        }

        public static ScanResult ScanCode(Bitmap img)
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
            catch (Exception ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }
        }

        public static ScanResult ScanCode(System.Windows.Media.Imaging.BitmapSource img)
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
                    return DecodeByOpenCV(BitmapHelper.ToBitmap(img));
                else
                    throw new Exception("找不到方法");
            }
            catch (Exception ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }
        }

        private static ScanResult DecodeByOpenCV(Bitmap img)
        {
            if (img == null)
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }

            Mat[] rects;
            Mat[] matrix;
            string[] texts;
            try
            {
                Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img);
                opencv_decoder.Detect(mat, out rects, out matrix, out texts);
                mat.Dispose();
            }
            catch (Exception ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }

            if (rects.Length != 0)
            {
                return new ScanResult(ScanResultState.OK, rects, matrix, texts);
            }
            else
                return new ScanResult(ScanResultState.NoCode, "");
        }

        private static ScanResult DecodeByZxing(Bitmap img)
        {
            if (img == null)
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }
            zxing_reader.AutoRotate = true;
            ZXing.Result result = null;
            try
            {
                result = zxing_reader.Decode(img);
                img.Dispose();
            }
            catch (ZXing.ReaderException ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }

            if (result != null)
            {
                return new ScanResult(ScanResultState.OK, result);
            }
            else
                return new ScanResult(ScanResultState.NoCode, "");
        }

        private static ScanResult DecodeByZxing(System.Windows.Media.Imaging.BitmapSource img)
        {
            if (img == null)
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }
            zxing_reader.AutoRotate = true;

            ZXing.Result result = null;
            try
            {
                result = zxing_reader.Decode(img);

            }
            catch (ZXing.ReaderException ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }

            if (result != null)
            {
                return new ScanResult(ScanResultState.OK, result);
            }
            else
                return new ScanResult(ScanResultState.NoCode, "");
        }

        private static ScanResult DecodeByZxingMulti(Bitmap img)
        {
            if (img == null)
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }
            zxing_reader.AutoRotate = true;

            ZXing.Result[] results = null;
            try
            {
                results = zxing_reader.DecodeMultiple(img);
                img.Dispose();
            }
            catch (ZXing.ReaderException ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }

            if (results != null && results.Length > 0)
            {
                return new ScanResult(ScanResultState.OK, results);
            }
            else
                return new ScanResult(ScanResultState.NoCode, "");
        }

        private static ScanResult DecodeByZxingMulti(System.Windows.Media.Imaging.BitmapSource img)
        {
            if (img == null)
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }
            zxing_reader.AutoRotate = true;

            ZXing.Result[] results = null;
            try
            {
                results = zxing_reader.DecodeMultiple(img);
            }
            catch (ZXing.ReaderException ex)
            {
                return new ScanResult(ScanResultState.Error, ex.ToString());
            }

            if (results != null && results.Length > 0)
            {
                return new ScanResult(ScanResultState.OK, results);
            }
            else
                return new ScanResult(ScanResultState.NoCode, "");
        }

        private static ScanResult DecodeByZbar(Bitmap img)
        {
            Bitmap pImg = img.MakeGrayscale3();
            List<ZBar.Symbol> symbols = new List<ZBar.Symbol>();
            symbols = zbar_scanner.Scan(pImg);
            pImg.Dispose();

            if (symbols != null && symbols.Count > 0)
            {
                return new ScanResult(ScanResultState.OK, symbols);
            }
            else
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }
        }

        private static ScanResult DecodeByZbar(System.Windows.Media.Imaging.BitmapSource img)
        {
            Bitmap pImg = BitmapHelper.ToBitmap(img).MakeGrayscale3();
            List<ZBar.Symbol> symbols = new List<ZBar.Symbol>();
            symbols = zbar_scanner.Scan(pImg);
            pImg.Dispose();

            if (symbols != null && symbols.Count > 0)
            {
                return new ScanResult(ScanResultState.OK, symbols);
            }
            else
            {
                return new ScanResult(ScanResultState.NoCode, "");
            }
        }
    }
}
