using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Samples.Section1.Async
{
    //指定のURIからテクスチャをダウンロードして
    //Rawイメージに設定する
    public class DownloadTextureUniTask : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        private void Start()
        {
            //このGameObjectに紐づいたCancellationTokenを取得
            var token = this.GetCancellationTokenOnDestroy();
            
            //テクスチャのセットアップを実行
            SetupTextureAsync(token).Forget();
        }

        private async UniTaskVoid SetupTextureAsync(CancellationToken token)
        {
            try
            {
                var uri = "https://www.dendai.ac.jp/common/images/global/og_img.png"; //表示したい画像へのアドレス

                //UniRxのRetryを使いたいので、UniTaskからObservableへ変換する
                var observable = Observable
                    .Defer(() =>
                    {
                        return GetTextureAsync(uri, token)
                            .ToObservable();
                    })
                    .Retry(3);

                //Observableもawaitで待ち受け可能
                var texture = await observable;
                _rawImage.texture = texture;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.Log(e);
            }
        }

        private async UniTask<Texture> GetTextureAsync(string uri,CancellationToken token)
        {
            using (var uwr = UnityWebRequestTexture.GetTexture(uri))
            {
                await uwr.SendWebRequest().WithCancellation(token);
                return ((DownloadHandlerTexture) uwr.downloadHandler).texture;
            }
        }
    }
}
