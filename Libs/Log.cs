
using AndroidAppServer.Libs;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.IO.Compression;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using static AndroidAppServer.Libs.Login;

namespace ADCHGKUser4.Controllers.Libs
{
    public static class Log
    {


        public class LoginScope:IDisposable
        {
            public int key;
            public LoginTokenInfo LoginData;
            internal LoginScope(string token,int thread)
            {
                key = thread;
                if (Login.DecodeToken(token,out var l ,out var p,out var v))
                {
                    LoginData = new LoginTokenInfo(l,p,v);
                };
            }

            public void Dispose()
            {
                ScopedLog.TryRemove(this.key,out _);
            }
        }
        public static void GetUserThreadLog(string token)
        {
            var ls = new LoginScope(token, Environment.CurrentManagedThreadId);
            ScopedLog.TryAdd(ls.key, ls);
        }

        public static ConcurrentDictionary<int, LoginScope> ScopedLog = new ConcurrentDictionary<int, LoginScope>();



        //public static string GetSberApiDescriptionForLog(string action) => action switch
        //{
        //    "register.do"=> "регистрация заказа в сбере, возврат: orderid - Guid заказа, formurl - Ссылка оплаты",
        //    "getOrderStatusExtended.do" =>
        //};
        public static Dictionary<string, NamedPipeServerStream> MultipleConsoles = new Dictionary<string, NamedPipeServerStream>();
        public static void AddNewLogConsole()
        {
            
        }
        public static string AppDirrectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static bool _IsLogEnabled { get; set; } = true;
        public static bool IsLogEnabled { get => _IsLogEnabled;
            set {

                _IsLogEnabled = value;
                if (!_IsLogEnabled) Warning("Log Disabled",force:true);
                else
                    Warning("Log Enabled",force: true);
            }
                }
       // public static 
        public static void sql(string e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine(e, ConsoleColor.Green, BackColor,force);
        }
        static StringBuilder sb = new StringBuilder();
        public static void sql(SqlCommand cmd, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            StringBuilder sbb = new StringBuilder();
            sbb.AppendLine();
            sbb.AppendLine($"[{cmd.CommandType}] {cmd.CommandText}");

            foreach (SqlParameter v in cmd.Parameters)
            {
                sbb.AppendLine($"{v.ParameterName} = {v.Value}");
            }
            WriteLine(sbb.ToString(), ConsoleColor.Green, BackColor, force);
            sbb.Clear();
        }
        public static void Error(string e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine("Error: "+e, ConsoleColor.Red, BackColor, force);
        }
        public static void Error(Exception e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine("Error: " + e.Message+" |\n"+e.StackTrace, ConsoleColor.Red, BackColor, force);
        }
        public static void Error(string title, Exception e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine($"Error: [{title}] " + e.Message + " |\n" + e.StackTrace, ConsoleColor.Red, BackColor, force);
        }
        public static void ApiCall(string e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine($"[API CALL] " + e, ConsoleColor.Cyan, BackColor, force);
        }
        public static void Json(object e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            if (IsLogEnabled || force) WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented), ConsoleColor.White, BackColor, force);
        }
        public static void Message(string e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine(e, ConsoleColor.Cyan, BackColor, force);
        }
        public static void Text(string e, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine(e, ConsoleColor.White, BackColor, force);
        }
        public static void Action(string e,string user, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine($"[IA] {user}-> "+e, ConsoleColor.White, BackColor, force);
        }
        public static void Action(string action,string e, string user, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine($"[{action}] {user}-> " + e, ConsoleColor.White, BackColor, force);
        }
        public enum LogLevel
        {
               deffault = 0,
               notDangerButInteresting= 1,
               DangerLikeBug = 2,
                HOW=5
        }
        public static void Warning(string e, LogLevel logLevel = LogLevel.deffault, ConsoleColor BackColor = ConsoleColor.Black, bool force = false)
        {
            WriteLine($"Warn{String.Concat(Enumerable.Repeat("!", (int)logLevel))}: "+e, ConsoleColor.Yellow, BackColor, force);
        }
        public static string LeftAling(string str,int size)
        {
            return  String.Format($"{{0,-{size}}}", str);
        }
        public static string CentreAling(string str, int size)
        { 
            return String.Format($"{{0,-{size}}}",
                String.Format("{0," + ((size + str.Length) / 2).ToString() + "}", str));
        }
        public static string RightAling(string str, int size)
        {
            return  String.Format($"{{0,{size}}}", str);
        }
        public static string Clamp(string str, int size)
        {
            return str.Length > size ? str.Substring(0, size) :str;
        }
        private static StringBuilder LogStringBuilder = new StringBuilder();
        private static DateTime DateFrom = new DateTime(2024,01, 01);
        public static void InitFileWriter()
        {

            string path = "";
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            // do some work

            void CurrentDomain_ProcessExit(object sender, EventArgs e)
            {
                OnShutdown();
            }

            Task.Run(() =>
            {


                while (true)
                {
                    try
                    {
                        var Now =  DateTime.Now;
                        //var  = DateTime.Now;
                        while ((Now.Month > DateFrom.Month) || Now.Year > DateFrom.Year)
                        {
                            var Dir = path + $"/{DateFrom.ToString("MM-yyyy")}";
                            if (Directory.Exists(Dir) && !File.Exists(Dir + ".zip"))
                            {
                                
                                ZipFile.CreateFromDirectory(Dir, Dir+ ".zip");
                            }
                            DateFrom = DateFrom.AddMonths(1);
                        }
                      
                        string newText = "";
                        lock (Console.Out)
                        {
                            newText = Log.LogStringBuilder.ToString();
                            Log.LogStringBuilder.Clear();
                        }
                        File.AppendAllText(GetPath(), newText);

                        Thread.Sleep(10000);

                    }
                    catch (Exception e)
                    {
                       // Log.Error(e);
                        Thread.Sleep(2500);
                    }
                }
            });
            void OnShutdown()
            {
                File.AppendAllText(GetPath(), Log.LogStringBuilder.ToString());
            }
            string GetPath()
            {
                var Dir = path + $"/{DateTime.Now.ToString("MM-yyyy")}";
                Directory.CreateDirectory(Dir);
                return Dir + $"/{DateTime.Now.ToString("dd-MM-yyyy")}_log.txt";
            }
            Log.Text("Log File Writer Inited");
        }



        public static void WriteLine(string message, ConsoleColor TextColor, ConsoleColor BackGroundColor,bool force=false)
        {


            if (IsLogEnabled||force)
                lock (Console.Out)
                {


                    ConsoleColor cTmp = Console.ForegroundColor;
                    ConsoleColor cTmp2 = Console.BackgroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(DateTime.Now.ToString("[dd-MM H:mm:ss] "));
                    Console.ForegroundColor = TextColor;
                    Console.BackgroundColor = BackGroundColor;

                   // LogStringBuilder.AppendLine(DateTime.Now.ToString("[dd-MM H:mm:s] ") + message);
                    Console.WriteLine(message);
                    LogStringBuilder.AppendLine(DateTime.Now.ToString("[dd-MM H:mm:s] ") + message);
                    Console.ForegroundColor = cTmp;
                    Console.BackgroundColor = cTmp2;
                }
        }
        public static void System(string e, ConsoleColor BackColor = ConsoleColor.Black)
        {



            WriteLine(e, ConsoleColor.Magenta, BackColor);

        }
    }
}
