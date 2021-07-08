using UnityEngine.Networking;

namespace PlanBuild.Services
{
    class ValheimBlueprintStoreService
    {

        public static void DeleteBlueprint()
        {
            var uwr = UnityWebRequest.Delete("uri");
            HttpService.SendRequest(uwr, value =>
            {
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }

        public static void DownloadBlueprint()
        {
            var uwr = UnityWebRequest.Get("uri");
            HttpService.SendRequest(uwr, value =>
            {
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }

        public static void UploadBlueprint(Blueprints.Blueprint blueprint)
        {
            var uwr = UnityWebRequest.Post("uri", blueprint.ToString());
            HttpService.SendRequest(uwr, value =>
            {
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }


        public static void UpdateBlueprint(Blueprints.Blueprint blueprint)
        {
            var uwr = UnityWebRequest.Put("uri", blueprint.ToString());
            HttpService.SendRequest(uwr, value =>
            {
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }
    }
}
