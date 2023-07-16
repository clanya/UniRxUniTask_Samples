using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Samples.Section1
{
    public class ThrottleButton : MonoBehaviour
    {
        private void Start()
        {
            //Update()のタイミングでAttackボタンが押されているか判定
            //Attackボタンが押されたらSubscribe()の処理を実行
            //そのあと30フレームの間ボタン入力を無視する
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Space))    //わかりやすいようにSpaceにします
                .ThrottleFirstFrame(30)
                .Subscribe(_ =>
                {
                    Debug.Log("Attack!");
                });
        }
    }
}