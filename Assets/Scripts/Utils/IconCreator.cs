using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IconCreator
{
    static Camera cam;
    public static Sprite CreateSprite(GameObject obj, Vector3 camOffset, Vector3 objRotation, Rect rect, float resolutionMultiplier = 1) {


        GameObject preview = Object.Instantiate(obj);
        preview.transform.rotation = Quaternion.Euler(objRotation);

        RenderTexture rt = new RenderTexture((int)(rect.width * resolutionMultiplier), (int)(rect.height * resolutionMultiplier), 32);
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        if(cam == null) {
            GameObject go = new GameObject();
            go.transform.position = camOffset;

            cam = go.AddComponent<Camera>();
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
        
        cam.targetTexture = rt;
        if (cam != null) cam.enabled = true;

        cam.Render();

        RenderTexture currentRenderTexture = RenderTexture.active;
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        cam.targetTexture = null;
        RenderTexture.active = currentRenderTexture;
        Object.DestroyImmediate(preview);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, rt.width, rt.height), Vector2.zero);


        cam.enabled = false;

        return sprite;
    }
}
