using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Samples.Section1
{
    public class DownloadTexture : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        
        private void Start()
        {
            var uri = "https://www.dendai.ac.jp/common/images/global/og_img.png";   //表示したい画像へのアドレス
            
            //テクスチャを取得する
            //ただし例外発生時は計3回まで試行する
            GetTextureAsync(uri)
                .OnErrorRetry(
                    onError: (Exception _) => { },
                    retryCount: 3
                ).Subscribe(
                    result => { _rawImage.texture = result; },
                    error => { Debug.LogError(error); }
                ).AddTo(this);

        }

        //コルーチンを起動して、その結果をObservableで返す
        private IObservable<Texture> GetTextureAsync(string uri)
        {
            return Observable.FromCoroutine<Texture>(observer =>
            {
                return GetTextureCoroutine(observer, uri);
            });
        }

        //コルーチンでテクスチャをダウンロードする
        private IEnumerator GetTextureCoroutine(IObserver<Texture> observer, string uri)
        {
            using (var uwr = UnityWebRequestTexture.GetTexture(uri))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)   //UnityWebRequest.isNetworkErrorとUnityWebRequest.isHttpErrorは非推奨になったらしい
                {
                    //エラーが起きたらOnErrorメッセージを発行する
                    observer.OnError(new Exception(uwr.error));
                    yield break;
                }

                var result = ((DownloadHandlerTexture) uwr.downloadHandler).texture;
                //成功したらOnNext/OnCompletedメッセージを発行する
                observer.OnNext(result);
                observer.OnCompleted();
            }
        }
    }
}
