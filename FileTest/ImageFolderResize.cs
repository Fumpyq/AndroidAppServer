using AndroidAppServer.Libs;

namespace AndroidAppServer.FileTest
{
    public static class ImageFolderResize
    {
        public static void ResizeAllInFolder(string FolderPath)
        {
            var files = Directory.GetFiles(FolderPath);

            foreach(var f in files.Where(x => Path.GetExtension(x) == ".png"))
            {
               
                using (FileStream fs = new FileStream(f, FileMode.OpenOrCreate)) {
                    var REs = ImageProcesser.ResizeImage(fs, 40);
                    fs.Seek(0, SeekOrigin.Begin);
                    REs.WriteTo(fs);

                }
               
            }
        }
    }
}
