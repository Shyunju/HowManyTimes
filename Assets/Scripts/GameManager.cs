using System.Collections;
using TMPro;
using UGESystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private int _likeability = 0;
    private int _amount = 10;
    [SerializeField]
    private AudioClip _buttonEffectSound;
    private UGECharacterManager _characterManager;
    //private CharacterUpdateCommand _testCommand;
    private string _userName = "test";
    private AudioSource _audioSource;
    public string UserName { get { return _userName; } }
    public int Likeability {get {return _likeability;} private set { _likeability = value; } }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    public void AddLikeablility(GameObject descriptText)
    {
        Likeability += _amount;
        //Debug.Log(Likeability);
        StartCoroutine(DescriptLikeablilityCO(descriptText));
    }
    private IEnumerator DescriptLikeablilityCO(GameObject descriptText)
    {
        descriptText.SetActive(true);
        yield return new WaitForSeconds(5f);
        descriptText.SetActive(false);
    }
    public void SetUserName(TMP_Text name)
    {
        if(null != name.text.ToString())
        {
            _userName = name.text.ToString();
            
            SceneManager.LoadScene(0);
            
        }
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
    public void PlaySound()
    {
        _audioSource.PlayOneShot(_buttonEffectSound);
    }
}
