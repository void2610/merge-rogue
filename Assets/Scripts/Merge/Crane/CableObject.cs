using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CableObject : MonoBehaviour
{
    [SerializeField] private GameObject cablePartPrefab;
    [SerializeField] private GameObject startObj;
    [SerializeField] private GameObject endObj;
    [SerializeField] private float cablePartWeight;
    [SerializeField] private float cablePartSize;
    [SerializeField] private float cableWidth;
    [SerializeField] private int cableLength;
    
    private const int MAXLENGTH = 1000;

    private List<GameObject> _vertices = new();
    private LineRenderer _lineRender;

    private void Start()
    {
        _lineRender = GetComponent<LineRenderer>();
        _lineRender.positionCount = _vertices.Count;
        _lineRender.startWidth = cableWidth;
        _lineRender.endWidth = cableWidth;

        //開始時点で糸を出す
        for (var i = 0; i < cableLength; i++) AddLine();
    }

    private void Update()
    {
        var idx = 0;
        _lineRender.positionCount = _vertices.Count;
        //糸の描画
        foreach (var v in _vertices)
        {
            _lineRender.SetPosition(idx, v.transform.position);
            idx++;
        }
    }

    //糸を1単位出す
    public void AddLine()
    {
        if (_vertices.Count > MAXLENGTH) return;

        //CablePartの新規オブジェクト作成
        var newCablePart = (GameObject)Instantiate(cablePartPrefab, transform.position, Quaternion.identity, this.gameObject.transform);
        newCablePart.name = "CablePart_" + (_vertices.Count + 1).ToString();

        var cablePartScript = newCablePart.GetComponent<CablePart>();
        cablePartScript.Weight = cablePartWeight;
        cablePartScript.Size = cablePartSize;

        _vertices.Add(newCablePart);
        //先端オブジェクトにくっつける
        startObj.GetComponent<HingeJoint2D>().connectedBody = newCablePart.gameObject.GetComponent<Rigidbody2D>();

        Vector2 startPos = startObj.gameObject.transform.position;
        newCablePart.transform.position = new Vector2(startPos.x, startPos.y);
        if (transform.childCount > 1)
        {
            newCablePart.gameObject.GetComponent<HingeJoint2D>().connectedBody
                = _vertices[_vertices.Count - 2].GetComponent<Rigidbody2D>();
        }
        else
        {
            newCablePart.GetComponent<HingeJoint2D>().connectedBody
                = endObj.GetComponent<Rigidbody2D>();
        }
    }

    //糸を1単位巻き取る
    public void Reel()
    {
        if (_vertices.Count <= 1) return;

        Destroy(_vertices[_vertices.Count - 1]);
        _vertices.RemoveAt(_vertices.Count - 1);
        var obj = _vertices[_vertices.Count - 1];

        //先端オブジェクトにくっつけなおす
        startObj.GetComponent<HingeJoint2D>().connectedBody = obj.gameObject.GetComponent<Rigidbody2D>();

        Vector2 startPos = startObj.gameObject.transform.position;
        obj.transform.position = new Vector2(startPos.x, startPos.y);
    }
}
