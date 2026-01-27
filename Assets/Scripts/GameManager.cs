using System.Collections;
using UGESystem;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private int _likeability = 0;
    private int _amount = 10;
    [SerializeField]
    private GameObject _descriptText;
    public int Likeability {get {return _likeability;} private set { _likeability = value; } }

    public void AddLikeablility()
    {
        Likeability += _amount;
        StartCoroutine(DescriptLikeablilityCO());
    }
    private IEnumerator DescriptLikeablilityCO()
    {
        _descriptText.SetActive(true);
        yield return new WaitForSeconds(5f);
        _descriptText.SetActive(false);
    }
}
