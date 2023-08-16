//===================================================
// ファイル名	：Fade.cs
// 概要			：シーンのフェード
// 作成者		：藤森 悠輝
// 作成日		：2018.12.21
// 
//---------------------------------------------------
// 更新履歴：
// 2018/12/21 [藤森 悠輝] スクリプト作成
// 2018/12/21 [藤森 悠輝] フェードに必要なメソッド作成
//
//===================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    //変数宣言-------------------------------------------------------------------------------------------
    [SerializeField] private Image fadeCurtain; //フェードに使う画像用
	public bool didFade;
	public bool changeColor = false;
	public float ChangeTime;
	//---------------------------------------------------------------------------------------------------

	/**
     * @fn void Start()
     * @brief 初期化
     * @param	無し
     * @return	無し
     */
	private void Start()
    { 
        fadeCurtain = transform.Find("FadeCurtain").gameObject.GetComponent<Image>();
		fadeCurtain.color = new Color(fadeCurtain.color.r, fadeCurtain.color.g, fadeCurtain.color.b, 1.0f);
    }

	/**
     * @fn IEnumerator FadeIn()
     * @brief フェードイン
     * @param	fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator FadeIn(float fadeTime = 3.0f)
    {
		while (fadeCurtain.color.a < 1f)
		{
			fadeCurtain.color += new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeCurtain.color.a >= 1f)
			{
				didFade = false;
				if(SceneManager.GetActiveScene().name == "Ranking")
				{
					ChangeColor(fadeTime);
					break;
				}
				else
				{
					break;
				}
			}
			yield return true;
		}
		yield return true;
	}

	public void ChangeColor(float fadeTime)
	{
		while (fadeCurtain.color.r <= 255f)
		{
			fadeCurtain.color += new Color(ChangeTime, ChangeTime, ChangeTime, 1f);
		}
		if(fadeCurtain.color.r >= 255f)
		{
			changeColor = true;
		}
	}

	/**
     * @fn IEnumerator FadeOut()
     * @brief フェードアウト
     * @param	fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator FadeOut(float fadeTime = 3.0f)
    {
		while (fadeCurtain.color.a > 0f)
        {
			fadeCurtain.color -= new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeCurtain.color.a <= 0)
			{
				didFade = true;
				break;
			}
			yield return true;
		}
		yield return true;
	}

	public IEnumerator FadeInOut(float fadeInTime = 3.0f, float fadeOutTime = 3.0f)
    {
		while (fadeCurtain.color.a < 1f)
        {
			fadeCurtain.color += new Color(0f, 0f, 0f, Time.deltaTime / fadeInTime);
			yield return true;
		}
		yield return true;
		while (fadeCurtain.color.a > 0f)
        {
			fadeCurtain.color -= new Color(0f, 0f, 0f, Time.deltaTime / fadeOutTime);
			if (fadeCurtain.color.a <= 0)
			{
				break;
			}
			yield return true;
		}
		yield return true;
	}

	public float FadeAlfaReturn()
	{
		return fadeCurtain.color.a;
	}

	/**
     * @fn IEnumerator ImageFadeIn()
     * @brief 送られてきた画像をフェードインさせる
     * @param	Image fadeImage 他のオブジェクトで管理している画像情報
	 *			fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator ImageFadeIn(Image fadeImage, float fadeTime = 3.0f)
    {
        while(fadeImage.color.a < 1f)
        {
            fadeImage.color += new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeImage.color.a >= 1)
			{
				break;
			}
			yield return true;
		}
		yield return true;
    }

	/**
     * @fn IEnumerator ImageFadeOut()
     * @brief 送られてきた画像をフェードアウトさせる
     * @param	Image fadeImage 他のオブジェクトで管理している画像情報
	 *			fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator ImageFadeOut(Image fadeImage, float fadeTime = 3.0f)
    {
		while (fadeImage.color.a > 0f)
        {
            fadeImage.color -= new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeImage.color.a <= 0)
			{
				break;
			}
			yield return true;
		}
		yield return true;
	}
}
