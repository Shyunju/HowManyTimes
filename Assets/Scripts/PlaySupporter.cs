using UGESystem;
using UnityEngine;

public class PlaySupporter : Singleton<PlaySupporter>
{
    [SerializeField]
    private GameObject _descriptionBox;
    public GameObject DescriptionBox{get{return _descriptionBox;}}
}
