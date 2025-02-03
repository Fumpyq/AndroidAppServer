using ADCHGKUser4.Controllers.Libs;
using System.IO;
using System.IO.Compression;

namespace AndroidAppServer.Libs
{
    public static class DebugGatherHandler
    {
        public static string saveDirrectory => Log.AppDirrectory + "//AppDebugLogs";

       

        public static void WriteDebug(string user_Guid, string log_text, byte[] screenShoot)
        {
            try
            {
                if (!Directory.Exists(saveDirrectory)) { Directory.CreateDirectory(saveDirrectory); }
             
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            var demoFile = archive.CreateEntry("log.txt");
                            using (var entryStream = demoFile.Open())
                            using (var streamWriter = new StreamWriter(entryStream))
                            {
                                streamWriter.Write(log_text);
                            }
                        

                            demoFile = archive.CreateEntry("ScreenShot.jpg");
                        Log.Text("image lenght:" + MathF.Round(screenShoot.Length /1024.0f,2) +"kb");
                        using (var ms = new MemoryStream(screenShoot))
                        {
                            var res= ImageProcesser.CropHeight(ms,240,480);
                            using (var entryStream = demoFile.Open()) res.CopyTo(entryStream);
                        }


                            var dir = saveDirrectory + $"//{user_Guid}";
                            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
                            var path = dir + $"//{DateTime.Now.ToString("dd MM yyyyTHH mm ss")}";
                            Log.Text($"Debug File savedAt {path}");
                            archive.Dispose();
                            File.WriteAllBytes(path + ".7z", memoryStream.GetBuffer());
                            // archive.no
                            // archive.ExtractToDirectory(path);

                        }

                    }
                
                
            }
            catch (Exception ex){ Log.Error("Debug Gather", ex); }
        }
    }
}
