using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GetAllChildren
{
    public static List<Transform> GetAll(this Transform tf)
    {
        List<Transform> allChildren = new List<Transform>();
        GetChildren(tf, ref allChildren);
        return allChildren;
    }

    //子要素を取得してリストに追加
    public static void GetChildren(Transform tf, ref List<Transform> allChildren)
    {
        Transform children = tf.GetComponentInChildren<Transform>();
        //子要素がいなければ終了
        if (children.childCount == 0)
        {
            return;
        }
        foreach (Transform ob in children)
        {
            allChildren.Add(ob);
            GetChildren(ob, ref allChildren);
        }
    }
}
