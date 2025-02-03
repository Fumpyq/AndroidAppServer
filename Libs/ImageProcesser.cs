
using System.Drawing;
using System.IO;
using PhotoSauce.MagicScaler;
namespace AndroidAppServer.Libs
{
    public class ImageProcesser
    {
        //private static Dictionary<string, FileFormat> FormatTable = new Dictionary<string, FileFormat>
        //{
        //    {".jpeg",FileFormat.Auto}
        //};
        //private static bool TryGetFileFormat(string FileName,out FileFormat format) { 
        //    string ext = Path.GetExtension(FileName).ToLower();

        //    return FormatTable.TryGetValue(ext, out format);
        //}
        public static Stream CropHeight(Stream image,int newW,int newH, string FileName = null)
        {
            int tryCount = 3;
            while (tryCount > 0)
            {
                try
                {
                    const int quality = 100;

                    var settings = new ProcessImageSettings()
                    {
                        Width = newW,
                        Height = newH,
                        ResizeMode = CropScaleMode.Crop,                        
                    };
                    
                    var output = new MemoryStream();

                    MagicImageProcessor.ProcessImage(image, output, settings);
                    //output.Flush();
                    output.Position = 0;
                    return output;
                }
                catch (Exception ex) { }
            }
            return null;

        }
        public static MemoryStream ResizeImage(Stream image, int size, string FileName=null)
        {
            int tryCount = 3;
            while (tryCount > 0)
            {
                try
                {
                    const int quality = 75;

                    var settings = new ProcessImageSettings()
                    {
                        Width = size,
                        Height = size,
                        ResizeMode = CropScaleMode.Max,
                        SaveFormat = FileFormat.Auto,
                        JpegQuality = quality,
                        JpegSubsampleMode = ChromaSubsampleMode.Subsample420
                    };

                    var output = new MemoryStream();

                    MagicImageProcessor.ProcessImage(image, output, settings);
                    //output.Flush();
                    output.Position = 0;
                    return output;
                }
                catch (Exception ex) { tryCount--; }
            }
            return null;
            
        }
    }
}
