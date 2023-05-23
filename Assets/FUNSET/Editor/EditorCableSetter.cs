using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Linq;

namespace FUNSET
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 深い階層まで子オブジェクトを名前で検索して GameObject 型で取得します
        /// </summary>
        /// <param name="self">GameObject 型のインスタンス</param>
        /// <param name="name">検索するオブジェクトの名前</param>
        /// <param name="includeInactive">非アクティブなオブジェクトも検索する場合 true</param>
        /// <returns>子オブジェクト</returns>
        public static GameObject FindDeep(
            this GameObject self,
            string name,
            bool includeInactive = false)
        {
            var children = self.GetComponentsInChildren<Transform>(includeInactive);
            foreach (var transform in children)
            {
                if (transform.name == name)
                {
                    return transform.gameObject;
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 
    /// ※このスクリプトの実行には、以下のスクリプトGameObjectExtensions.csが必要です。(Asset下のどこかにあれば機能します)
    /// https://baba-s.hatenablog.com/entry/2014/08/01/101104
    /// </summary>
    public class EditorCableSetter : EditorWindow
    {
        //メニューに項目追加
        [MenuItem("Tools/FUNSET/Create Cable(s)")]

        public static void open()
        {


            var window = EditorWindow.GetWindow(typeof(EditorCableSetter));
            window.maxSize = new Vector2(800f, 700f);
            window.minSize = new Vector2(600f, 350f);
        }

        List<GameObject> DropList = new List<GameObject>();


        Vector2 scrollPos1 = new Vector2(0, 0);

        public GameObject cableobj;
        public Transform cablepoint1;
        public Transform cablepoint2;
        public GameObject Elepole;

        float cabledistance;

        void OnGUI()
        {

            GUILayout.ExpandWidth(true);

            GUILayout.Label("始点・終点とケーブルオブジェクトを設定して、2点間に配置できます。");
            GUILayout.Label("**ちゅうい**\nアンドゥ不可");


            GUILayout.ExpandWidth(true);
            // 自身のSerializedObjectを取得
            var so = new SerializedObject(this);

            so.Update();

            // 第二引数をtrueにしたPropertyFieldで描画
            GUILayout.Label("ケーブル用のプレハブをセット\n(シーンオブジェクトは選択しないでください)");
            EditorGUILayout.PropertyField(so.FindProperty("cableobj"), true);

            so.ApplyModifiedProperties();
            //プロジェクトのプレハブかシーンオブジェクトか判別する方法がない

            var biginobj = new SerializedObject(this);

            biginobj.Update();

            GUILayout.Label("始点のオブジェクトをセット\n(シーン上のオブジェクトから選択してください)");
            EditorGUILayout.PropertyField(biginobj.FindProperty("cablepoint1"), true);

            biginobj.ApplyModifiedProperties();

            var elepoleobj = new SerializedObject(this);

            elepoleobj.Update();



            GUILayout.Label("電柱などのオブジェクトをセットすると\n始点と同名のオブジェクトを終点に自動セットします\n(シーン上のオブジェクトから選択してください)");
            EditorGUILayout.PropertyField(elepoleobj.FindProperty("Elepole"), true);

            bool clrelpoleobj = GUILayout.Button("Clear", GUILayout.Width(150), GUILayout.Height(30));
            if (clrelpoleobj) { Elepole = null; }

            elepoleobj.ApplyModifiedProperties();



            if (Elepole != null)
            {
                string cablepointname = cablepoint1.name;
                var ag = Elepole.FindDeep(cablepointname);
                if (ag != null) { cablepoint2 = ag.transform; } else { Elepole = null; }
            }

            if ((cablepoint1 != null) && (cablepoint2 != null))
            {
                //ポイント1とポイント2の距離を測る
                cabledistance = Vector3.Distance(cablepoint1.position, cablepoint2.position);
            }
            else
            {
                cabledistance = 0;
            }




            var endobj = new SerializedObject(this);

            endobj.Update();

            GUILayout.Label("終点のオブジェクトをセット\n(シーン上のオブジェクトから選択してください)");
            EditorGUILayout.PropertyField(endobj.FindProperty("cablepoint2"), true);

            endobj.ApplyModifiedProperties();

            GUILayout.Label("\nポイント1とポイント2の距離=" + cabledistance + "\n");



            if ((cableobj != null) && (cablepoint1 != null) && (cablepoint2 != null) && (cabledistance > 0.3f))
            {
                GUILayout.Label("**ちゅうい**");
                GUILayout.Label("アンドゥ不可");
                bool jikkou = GUILayout.Button("Execute CreatePrefabs", GUILayout.Width(250), GUILayout.Height(30));
                if (jikkou) { ExeCreateCablePrefab(); }

            }


        }

        void ExeCreateCablePrefab()
        {
            //パーツ長さを出す
            Bounds cableobjbounds = cableobj.GetComponent<MeshFilter>().sharedMesh.bounds;
            var caos = cableobjbounds.size;
            float cableobjlength = caos.z;

            Vector3 haba = cableobj.transform.localScale;
            haba.z = cabledistance / cableobjlength;


            //GameObject cableobject = Instantiate(cableobj);
            GameObject cableobject = PrefabUtility.InstantiatePrefab(cableobj) as GameObject;
            Undo.RegisterCreatedObjectUndo(cableobject, "Create New GameObject");

            cableobject.transform.parent = cablepoint1.transform;
            cableobject.transform.localPosition = new Vector3(0, 0, 0);
            cableobject.transform.LookAt(cablepoint2);

            cableobject.transform.localScale = haba;


        }
    }
}