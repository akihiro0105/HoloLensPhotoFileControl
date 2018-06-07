using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhotoFileControl;

public class PhotoFileControlSample : MonoBehaviour {
    public GameObject panel;

	// Use this for initialization
	void Start () {
        StartCoroutine(PhotoView());
	}

    private IEnumerator PhotoView()
    {
        // View List
        string[] list = null;
        yield return PhotoControl.ViewPhotos((l) => { list = l; });
        for (int i = 0; i < list.Length; i++) Debug.Log(list[i]);

        // Load Photo
        Texture2D tex = new Texture2D(1, 1);
        yield return PhotoControl.LoadPhoto(list[0], (t) => { tex = t; });
        panel.GetComponent<Renderer>().material.mainTexture = tex;

        // Save Photo
        yield return PhotoControl.SavePhoto("test", tex, null);
    }
}
