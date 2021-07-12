using PlanBuild.Blueprints;
using PlanBuild.Services.Models;
using UnityEngine.Networking;

namespace PlanBuild.Services
{
    class ValheimBlueprintStoreService
    {
        public static void GetBlueprints()
        {
            var uwr = UnityWebRequest.Get($"{Constants.ValheimBlueprintStoreEndpoint}");
            HttpService.SendRequest(uwr, value =>
            {
                //callback
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }

        public static void DeleteBlueprint(string id)
        {
            var uwr = UnityWebRequest.Delete($"{Constants.ValheimBlueprintStoreEndpoint}/${id}");
            HttpService.SendRequest(uwr, value =>
            {
                //callback
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }

        public static void DownloadBlueprint(string id)
        {
            var uwr = UnityWebRequest.Get($"{Constants.ValheimBlueprintStoreEndpoint}/${id}");
            HttpService.SendRequest(uwr, value =>
            {
                //callback
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }

        public static void UploadBlueprint(Blueprint blueprint)
        {
            var uwr = UnityWebRequest.Post($"{Constants.ValheimBlueprintStoreEndpoint}/${blueprint.ID}", SimpleJson.SimpleJson.SerializeObject(BlueprintUpload.Convert(blueprint)));
            HttpService.SendRequest(uwr, value =>
            {
                //callback
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }


        public static void UpdateBlueprint(Blueprint blueprint)
        {
            var uwr = UnityWebRequest.Put($"{Constants.ValheimBlueprintStoreEndpoint}/${blueprint.ID}", SimpleJson.SimpleJson.SerializeObject(BlueprintUpload.Convert(blueprint)));
            HttpService.SendRequest(uwr, value =>
            {
                //callback
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }
    }
}
