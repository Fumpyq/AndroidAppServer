
namespace TcpStructs
{
    public enum TcpMessageType
    {

        HandShake = 0,
        DiskInfo = 1,
        Ping = 2,
        Set_User_Friendly_Id = 3,
        DriveInfo = 4,
        Get_Updates = 5,
        Sys_Info = 6,
        CreateDataBaseBackup= 7,
        DeleteFile= 8,
        FileInfo=9,
    }
    public enum TcpMessageTypev2
    {

        HandShake = 0,
        DownloadUpdates = 1,
        SendLogs = 2
    }
    public struct CopyFileReq
    {
        public string from;
        public string[] to;
    }
    public struct ShortFileInfo
    {
        public long size;
    }
    public struct CreateBackUpResponse
    {
        public string guid{get;set;}
        
        public string Fullpath{get;set;}
        public string[] Additionalpaths{get;set;}
        public ShortFileInfo[] fileInfo{get;set;}
        public string exception{get;set;}
        public long size{get;set;}
    }

    public enum responseStatus
    {
        TimeOut=1,
        Success=2, 
        Error=3, 
    }
    public struct TcpMessageHeader
    {
        public TcpMessageType type { get; set; }
        public object response_guid { get; set; }

        public int dataSize { get; set; }
    }
    public struct SystemInfo
    {
        public float cpuUsage { get; set; }
        public MemoryMetrics memory { get; set; }
    }
    public class MemoryMetrics
    {
        public double Total { get; set; }
        public double Used { get; set; }
        public double Free { get; set; }
    }
    public struct HandShakePackage
    {
        public const string pingPocket = "p";
        public bool IsNull;
        public string Connection_Guid { get; set; }
        public string UserFriendlyName { get; set; }
        public static HandShakePackage Null => new HandShakePackage() { IsNull = true };
    }
}
