using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhotoFileControl;

public class PhotoFileControlSample : MonoBehaviour
{
    [SerializeField] private Renderer panelRenderer;

	// Use this for initialization
	void Start () {
        StartCoroutine(PhotoView());
	}

    private IEnumerator PhotoView()
    {
        // 画像の一覧を取得
        string[] list = null;
        yield return PhotoControl.ViewPhotos((l) => list = l);
        for (var i = 0; i < list.Length; i++) Debug.Log(list[i]);

        // 画像を取得
        var tex = new Texture2D(1, 1);
        yield return PhotoControl.LoadPhoto(list[0], (t) => tex = t);
        panelRenderer.material.mainTexture = tex;

        // Textureを保存
        yield return PhotoControl.SavePhoto("test", tex, null);
    }
}
