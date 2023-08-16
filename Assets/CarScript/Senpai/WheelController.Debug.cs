using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

namespace AIM
{
    public partial class WheelController : MonoBehaviour
    {
        
        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) transformPosition = transform.position;

            Gizmos.color = Color.green;
            var forwardOffset = transform.forward * 0.07f;
            var springOffset = transform.up * spring.maxLength;
            Gizmos.DrawLine(transformPosition - forwardOffset, transformPosition + forwardOffset);
            Gizmos.DrawLine(transformPosition - springOffset - forwardOffset, transformPosition - springOffset + forwardOffset);
            Gizmos.DrawLine(transformPosition, transformPosition - springOffset);

            Vector3 interpolatedPos = Vector3.zero;

            if (!Application.isPlaying)
            {
                if (wheel.visual != null)
                {
                    wheel.worldPosition = wheel.visual.transform.position;
                    wheel.up = wheel.visual.transform.up;
                    wheel.forward = wheel.visual.transform.forward;
                    wheel.right = wheel.visual.transform.right;
                }
            }

            Gizmos.DrawSphere(wheel.worldPosition, 0.02f);

            // タイヤを描画
            Gizmos.color = Color.green;
            DrawWheelGizmo(wheel.tireRadius, wheel.width, wheel.worldPosition, wheel.up, wheel.forward, wheel.right);

            if (debug && Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.up));
                Gizmos.color = Color.green;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.forward));
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.right));
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.inside));

                if (spring.length < 0.01f) Gizmos.color = Color.red;
                else if (spring.length > spring.maxLength - 0.01f) Gizmos.color = Color.yellow;
                else Gizmos.color = Color.green;

                if (hasHit || true)
                {
                    float weightSum = 0f;
                    float minWeight = Mathf.Infinity;
                    float maxWeight = 0f;

                    foreach (WheelHit hit in wheelHits)
                        {
                            weightSum += hit.weight;
                            if (hit.weight < minWeight) minWeight = hit.weight;
                            if (hit.weight > maxWeight) maxWeight = hit.weight;
                        }

                    foreach (WheelHit hit in wheelHits)
                        {
                            float t = (hit.weight - minWeight) / (maxWeight - minWeight);
                            Gizmos.color = Color.Lerp(Color.black, Color.white, t);
                            Gizmos.DrawSphere(hit.point, 0.04f);
                            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                            Gizmos.DrawLine(hit.point, hit.point + wheel.up * hit.distanceFromTire);
                        }

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(new Ray(wheelHit.point, wheelHit.forwardDir));
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(new Ray(wheelHit.point, wheelHit.sidewaysDir));

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(wheelHit.point, 0.04f);
                    Gizmos.DrawLine(wheelHit.point, wheelHit.point + wheelHit.normal * 1f);

                    Gizmos.DrawSphere(forcePoint, 0.06f);

                    Gizmos.color = Color.yellow;
                    Vector3 alternateNormal = (wheel.worldPosition - wheelHit.point).normalized;
                    Gizmos.DrawLine(wheelHit.point, wheelHit.point + alternateNormal * 1f);

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawCube(spring.targetPoint, new Vector3(0.1f, 0.1f, 0.04f));
                }

                //2023/7/1 河合奏追記-----------------------------------------------------------------------------
                //前方向の力（トラクション）を線で可視化。
                Gizmos.color = ((int)vehicleFRSide == 1) ? Color.red : Color.blue;      //前輪：赤　後輪：青に設定
                Gizmos.DrawLine(vcForcePoint, vcForcePoint + traction);                 //力を加える座標を起点にし、力をベクトルとして表示

                //横方向の力（コーナリングフォース）を線で可視化。
                Gizmos.color = ((int)vehicleFRSide == 1) ? Color.green : Color.yellow;  //前輪：緑色　後輪：黄色に設定
                Gizmos.DrawLine(vcForcePoint, vcForcePoint + corneringforce);           //力を加える座標を起点にし、力をベクトルとして表示

                //Rigidbodyに加える力の総合力（tortalForce）を線で可視化。
                Gizmos.color = Color.cyan;                                              //シアンに設定
                Gizmos.DrawLine(vcForcePoint, vcForcePoint + vcForce);                  ////力を加える座標を起点にし、力をベクトルとして表示
                
                //現在Rigidbody上の各ホイール位置にかかっている力のベクトル方向を線で可視化。
                Gizmos.color = Color.white;                                                                     //白色に設定
                Gizmos.DrawLine(wheelHit.raycastHit.point, wheelHit.raycastHit.point + contactVelocity);        //車輪の接地点を起点に、力をベクトルとして表示

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(vcForcePoint, vcForcePoint + rollingResistance);
                
                //重心の位置を白色の球で可視化。
                Gizmos.DrawSphere(parentRigidbody.transform.position + parentRigidbody.centerOfMass, 0.5f);

                //Rigidbodyに加える力の位置を球で可視化。
                Gizmos.color = Color.gray;                                              //灰色に設定
                Gizmos.DrawSphere(vcForcePoint, vcForce.magnitude / vc.DebugFoceSize);           //力を加える座標を設定、力の大きさを半径に反映
                //--------------------------------------------------------------------------------------------------
            }
        }

        private void DrawWheelGizmo(float radius, float width, Vector3 position, Vector3 up, Vector3 forward, Vector3 right)
        {
            var halfWidth = width / 2.0f;
            float theta = 0.0f;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = position + up * y + forward * x;
            Vector3 newPos = pos;

            for (theta = 0.0f; theta <= Mathf.PI * 2; theta += Mathf.PI / 12.0f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = position + up * y + forward * x;

                Gizmos.DrawLine(pos - right * halfWidth, newPos - right * halfWidth);

                Gizmos.DrawLine(pos + right * halfWidth, newPos + right * halfWidth);

                Gizmos.DrawLine(pos - right * halfWidth, pos + right * halfWidth);

                Gizmos.DrawLine(pos - right * halfWidth, newPos + right * halfWidth);

                pos = newPos;

    
            }
        }

        /*//河合奏追加---------------------------------------------------------------------------------------
        //外部に数理を記録する
        void Writing(Vector3 v3, string sPath)
        {
            // 書き込みパス
            string path = "C:/Users/student/Desktop/AimRacing2022" + sPath;        //AddforceReport.txtが無ければ作成する

            //書き込み処理
            using (var fs = new StreamWriter(path, isAppend, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                //引数で受け取った数値を持っている車輪の位置を取得し、記録する
                if ((int)vehicleFRSide == 1) fs.Write("F");
                else fs.Write("R");
                if ((int)vehicleLRSide == 1) fs.Write("R");
                else fs.Write("L");
                fs.Write("：");

                //引数の記録
                fs.Write(v3);

                //車輪すべての記録をしたら改行する
                if ((int)vehicleFRSide == -1 && (int)vehicleLRSide == -1)
                {
                    fs.Write(fs.NewLine);
                }
            }

            //初回以降はつい気に変えておく
            if(!isAppend) isAppend = true;
        }
        //--------------------------------------------------------------------------------------*/
    }
}
