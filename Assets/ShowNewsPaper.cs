using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowNewsPaper : MonoBehaviour
{
    [SerializeField]
    private Image _nowNewsPaper;
    [SerializeField]
    private List<Image> _newsPaper;
    [SerializeField]
    private TMP_Text _totalPageText;
    [SerializeField]
    private TMP_Text _nowPageText;
    private int _page = 1;


    void Start()
    {
        _newsPaper = new List<Image>();
    }
    void OnEnable()
    {
        _page = 1;
        _nowNewsPaper = _newsPaper[0];
        _nowPageText.text = _page.ToString();
        _totalPageText.text = _newsPaper.Count.ToString();
    }
    public void NextPage()
    {
        if(_page + 1 < _newsPaper.Count)
        {
            _page++;
            _nowNewsPaper = _newsPaper[_page-1];
            _nowPageText.text = _page.ToString();
        }
        return;
    }
    public void PrevPage()
    {
        if(_page -1 >= 1)
        {
            _page--;
            _nowNewsPaper = _newsPaper[_page-1];
            _nowPageText.text = _page.ToString();
        }
        return;
    }
}
