// 先輩方のコード CarSearch.cs を参考

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSearch : MonoBehaviour
{
	
	[SerializeField] string carName;					// 検索アセット名
	[SerializeField] string tagName;					// 検索タグ名

	public GameObject carObject;						// 車情報を格納
	public AIM.VehicleController vc;					// VehicleController 情報を格納
	public bool bIsCar;									// 車が格納されているか
	
	public bool bIsDesktopInputManager;                 // DesktopInputManager用接続管理フラグ

	// Update is called once per frame
	void Update()
	{
		// 車がすでに格納されている場合
		if (bIsCar)
        {
			// 処理をしない
			return;
		}

		// アセット名で検索
		if (GameObject.Find(carName))
		{
			// 車の各情報を格納
			carObject = GameObject.Find(carName);
			vc = carObject.GetComponent<AIM.VehicleController>();
			bIsCar = true;
		}
		else
		{
			// タグ名で検索
			if (GameObject.FindGameObjectWithTag(tagName))
			{
				// 車の各情報を格納
				carObject = GameObject.FindGameObjectWithTag(tagName);
				vc = carObject.GetComponent<AIM.VehicleController>();
				bIsCar = true;
			}
			else
			{
				// 見つからない場合ログを出力
				Debug.Log("車が見つかりません。");
				return;
			}
		}
	}
}
