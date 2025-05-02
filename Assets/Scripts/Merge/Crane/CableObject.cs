using System.Collections.Generic;
using UnityEngine;

public class CableObject : MonoBehaviour
{
    //部品のプレハブ
    [SerializeField] GameObject CablePartPrefab = null;
    //接続先
    [SerializeField] GameObject StartObj = null;
    [SerializeField] GameObject EndObj = null;
    //糸一単位の重さ
    [SerializeField] float CablePartWeight = 1f;
    //糸一単位のサイズ（きめ細かさ）
    [SerializeField] float CablePartSize = 0.1f;
    //糸の太さ
    [SerializeField] float CableWidth = 0.05f;
    //糸の長さ
    [SerializeField] int CableLength = 100;
    //糸の最大長さ（無限に糸を伸ばせないようにする）
    const int MAXLENGTH = 1000;

    List<GameObject> vertices = new List<GameObject>();
    LineRenderer lineRender;

    void Start()
    {
        lineRender = GetComponent<LineRenderer>();
        lineRender.positionCount = vertices.Count;
        lineRender.startWidth = CableWidth;
        lineRender.endWidth = CableWidth;

        //開始時点で糸を出す
        for (int i = 0; i < CableLength; i++)
        {
            addLine();
        }
    }

    void Update()
    {
        int idx = 0;
        lineRender.positionCount = vertices.Count;
        //糸の描画
        foreach (GameObject v in vertices)
        {
            lineRender.SetPosition(idx, v.transform.position);
            idx++;
        }
    }

    //糸を1単位出す
    public void addLine()
    {
        if (vertices.Count > MAXLENGTH) return;

        //CablePartの新規オブジェクト作成
        GameObject newCablePart = (GameObject)Instantiate(
            CablePartPrefab,
            transform.position,
            Quaternion.identity,
            this.gameObject.transform);
        newCablePart.name = "CablePart_" + (vertices.Count + 1).ToString();

        CablePart cablePartScript = newCablePart.GetComponent<CablePart>();
        cablePartScript.Weight = CablePartWeight;
        cablePartScript.Size = CablePartSize;

        vertices.Add(newCablePart);
        //先端オブジェクトにくっつける
        StartObj.GetComponent<HingeJoint2D>().connectedBody
            = newCablePart.gameObject.GetComponent<Rigidbody2D>();

        Vector2 startPos = StartObj.gameObject.transform.position;
        newCablePart.transform.position
            = new Vector2(startPos.x,
            startPos.y);
        if (transform.childCount > 1)
        {
            newCablePart.gameObject.GetComponent<HingeJoint2D>().connectedBody
                = vertices[vertices.Count - 2].GetComponent<Rigidbody2D>();
        }
        else
        {
            newCablePart.GetComponent<HingeJoint2D>().connectedBody
                = EndObj.GetComponent<Rigidbody2D>();
        }
    }

    //糸を1単位巻き取る
    public void Reel()
    {
        if (vertices.Count <= 1) return;

        Destroy(vertices[vertices.Count - 1]);
        vertices.RemoveAt(vertices.Count - 1);
        GameObject obj = vertices[vertices.Count - 1];

        //先端オブジェクトにくっつけなおす
        StartObj.GetComponent<HingeJoint2D>().connectedBody
            = obj.gameObject.GetComponent<Rigidbody2D>();

        Vector2 startPos = StartObj.gameObject.transform.position;

        obj.transform.position
            = new Vector2(startPos.x,
            startPos.y);
    }
}
