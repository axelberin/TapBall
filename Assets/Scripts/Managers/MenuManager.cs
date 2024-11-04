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
        for (int i = 0; i < _dunkLevelsRecords.Length; i++)
        {
            if (_dunkLevelsRecords[i] != null && LoadAndSaveManager.ContainsKey(
                LoadAndSaveManager.DunkLevelName + i))
                _dunkLevelsRecords[i].text = LoadAndSaveManager.GetIntValue(
                    LoadAndSaveManager.DunkBestName + i).ToString();
        }
        #endregion
        #region UNLOCK LEVELS
        for (int i = 0; i < _dunkLevelsButtons.Length; i++)
        {
            if (i == 0)
                _dunkLevelsButtons[i].interactable = true;
            else
                _dunkLevelsButtons[i].interactable = LoadAndSaveManager.ContainsKey(
                    LoadAndSaveManager.DunkLevelName + i);
        }
        #endregion
        #region WITHOUT DEATH

        for (int i = 0; i < _dunkWithoutDeath.Length; i++)
        {
            _dunkWithoutDeath[i].gameObject.SetActive(_dunkWithoutDeath[i]
                && LoadAndSaveManager.ContainsKey(LoadAndSaveManager.DunkLevelName + i) &&
                LoadAndSaveManager.GetIntValue(LoadAndSaveManager.DunkWithoutDeathName + i) == 1);
        }
        #endregion
        #endregion
    }
}
