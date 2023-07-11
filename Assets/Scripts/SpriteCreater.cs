using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCreater : MonoBehaviour
{
    [SerializeField] SpriteRenderer testSprite;
    void Start()
    {
        CreateSquare();
    }

    void CreateSquare()
    {
        //(Width, height)
        Texture2D texture2D = new Texture2D(128, 128);

        //最初から色を付けたい場合はテクスチャの各ピクセルに色を入れていく
        //単色の場合はあとからカラーを指定して変更することもできる。その場合は以下Sprite.Createまで不要
        int pixelAmount = 128 * 128; //テクスチャの高さや幅はピクセル数を表すので総ピクセルは両辺の積
        Color[] changedPixels = new Color[pixelAmount];
        for (int i = 0; i < pixelAmount; i++)
        {
            changedPixels.SetValue(Color.white, i);
        }

        texture2D.SetPixels(changedPixels);
        texture2D.Apply();

        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

        testSprite.sprite = sprite;
    }
}
