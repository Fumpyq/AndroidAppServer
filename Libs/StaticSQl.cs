using ADCHGKUser4.Controllers.Libs;

namespace AndroidAppServer.Libs
{
    public static  class StaticSQl
    {
        public static Dictionary<string, ContainerEnum> ContainersTypes = new Dictionary<string, ContainerEnum>(6);


        public static void Init() {

            var conts = SQL.GetContainers();

            foreach(var v in conts)
            {
                ContainersTypes.Add(v.guid, v);
            }
            Log.Text("Container Types Loaded");
        }

        public static ContainerEnum GetContainer(string guid) {
            if (!ContainersTypes.ContainsKey(guid)) throw new Exception($" НЕ ИЗВЕСТНЫЙ ТИП КОНТЕЙНЕРА ! StaticSQl. ContainersTypes :{guid}");
            return ContainersTypes[guid];
                }

    }
}
