using System;
using PlanBuild.Blueprints;
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

        public static void UploadBlueprint(Blueprint blueprint)
        {
            var uwr = UnityWebRequest.Post("uri", Convert.ToBase64String(blueprint.ToBlob()));
            HttpService.SendRequest(uwr, value =>
            {
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }


        public static void UpdateBlueprint(Blueprint blueprint)
        {
            var uwr = UnityWebRequest.Put("uri", Convert.ToBase64String(blueprint.ToBlob()));
            HttpService.SendRequest(uwr, value =>
            {
                Jotunn.Logger.LogInfo(value.downloadHandler.text);
            });
        }
    }
}
