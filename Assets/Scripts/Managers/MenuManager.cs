using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button[] _dunkLevelsButtons;
    [SerializeField] TextMeshProUGUI[] _dunkLevelsRecords;
    [SerializeField] Image[] _dunkWithoutDeath;

    int _maxDunkLevels;

    void Start()
    {
        Application.targetFrameRate = 60;
        _maxDunkLevels = _dunkLevelsRecords.Length;

        #region DUNK
        #region BEST
        if (JSON.Instance.GetDunkData.S_DunkBest.Count <= 0)
        {
            for (int i = 0; i < _maxDunkLevels; i++) JSON.Instance.GetDunkData.S_DunkBest.Add(0);
        }
        else
        {
            for (int i = 0; i < _dunkLevelsRecords.Length; i++)
            {
                if (_dunkLevelsRecords[i] != null)
                    _dunkLevelsRecords[i].text = JSON.Instance.GetDunkData.S_DunkBest[i].ToString();
            }
        }
        #endregion
        #region UNLOCK LEVELS
        if (JSON.Instance.GetDunkData.S_DunkLevels.Count <= 0)
        {
            JSON.Instance.GetDunkData.S_DunkLevels.Add(0);
            for (int i = 0; i < _dunkLevelsButtons.Length; i++)
            {
                if (i == 0) _dunkLevelsButtons[i].interactable = true;
                else _dunkLevelsButtons[i].interactable = false;
            }
        }
        else
        {
            for (int i = 0; i < _dunkLevelsButtons.Length; i++)
            {
                if (JSON.Instance.GetDunkData.S_DunkLevels.Contains(i)) _dunkLevelsButtons[i].interactable = true;
                else _dunkLevelsButtons[i].interactable = false;
            }
        }
        #endregion
        #region WITHOUT DEATH

        if (JSON.Instance.GetDunkData.S_DunkWithoutDeath.Count <= 0)
        {
            for (int i = 0; i < _dunkWithoutDeath.Length; i++)
            {
                JSON.Instance.GetDunkData.S_DunkWithoutDeath.Add(false);
                if (_dunkWithoutDeath[i]) _dunkWithoutDeath[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < _dunkWithoutDeath.Length; i++)
            {
                if (_dunkWithoutDeath[i]) _dunkWithoutDeath[i].gameObject.SetActive(JSON.Instance.GetDunkData.S_DunkWithoutDeath[i]);
            }
        }
        #endregion

        JSON.Instance.SaveDunkData();
        #endregion
    }
}
