using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace PlanBuild.Services
{
    class HttpService : MonoBehaviour
    {
        private static HttpService _instance;

        public static HttpService Instance { get { return _instance; } }


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public static void SendRequest(UnityWebRequest uwr, System.Action<UnityWebRequest> callback)
        {
            Instance.StartCoroutine(SendRequestCoroutine(uwr, callback));
        }

        private static IEnumerator SendRequestCoroutine(UnityWebRequest uwr, System.Action<UnityWebRequest> callback)
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Jotunn.Logger.LogError("Error While Sending: " + uwr.error);
            }
            else
            {
                if (uwr.isHttpError)
                {
                    Jotunn.Logger.LogError($"Request failed, response code: {uwr.responseCode}");
                    yield break;
                }
                callback(uwr);
            }
        }
    }
}
