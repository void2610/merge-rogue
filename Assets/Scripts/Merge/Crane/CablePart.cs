using UnityEngine;
using UnityEngine.Serialization;

public class CablePart : MonoBehaviour
{
    public float weight;
    public float size;

    private void Start()
    {
        //重さの設定
        gameObject.GetComponent<Rigidbody2D>().mass = weight;
        //サイズ設定
        gameObject.transform.localScale = new Vector2(size, size);
        //HingeJoint2Dの設定
        gameObject.GetComponent<HingeJoint2D>().autoConfigureConnectedAnchor = false;
        gameObject.GetComponent<HingeJoint2D>().anchor = new Vector2(size / 4, size / 4);
        //JOINTの暴れ対策
        gameObject.GetComponent<Rigidbody2D>().inertia = 0.1f;
    }
}