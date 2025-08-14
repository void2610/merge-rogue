using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RelicDataList", menuName = "Scriptable Objects/RelicDataList")]
public class RelicDataList : ScriptableObject
{
    [FormerlySerializedAs("relicDataList")] [SerializeField] 
    public List<RelicData> list = new ();

    public void Register()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(list, sortKeySelector: data => data.name);
#endif
    }
}
