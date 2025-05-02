using UnityEngine;

public class CablePart : MonoBehaviour
{
    public float Weight;
    public float Size;

    void Start()
    {
        //重さの設定
        gameObject.GetComponent<Rigidbody2D>().mass = Weight;
        //サイズ設定
        gameObject.transform.localScale = new Vector2(Size, Size);
        //HingeJoint2Dの設定
        gameObject.GetComponent<HingeJoint2D>().autoConfigureConnectedAnchor = false;
        gameObject.GetComponent<HingeJoint2D>().anchor = new Vector2(Size / 4, Size / 4);
        //JOINTの暴れ対策
        gameObject.GetComponent<Rigidbody2D>().inertia = 0.1f;
    }
}