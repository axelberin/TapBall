using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : ACanvas
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
        var dunkLevelsButtons = new List<Button>();
        for (int i = 1; i <= 100; i++)
        {
            var button = FindAndValidateButtonComponent(transform, $"DunkLevel{i}");

            if (button == null)
                break;

            dunkLevelsButtons.Add(button);
        }

        if (dunkLevelsButtons.Count > 0)
            _dunkLevelsButtons = dunkLevelsButtons.ToArray();

        for (int i = 0; i < _dunkLevelsRecords.Length; i++)
        {
            if (_dunkLevelsRecords[i] != null && SaveAndLoadManager.ContainsKey(
                SaveAndLoadManager.DunkLevelName + i))
                _dunkLevelsRecords[i].text = SaveAndLoadManager.GetIntValue(
                    SaveAndLoadManager.DunkBestName + i).ToString();
        }
        #endregion
        #region UNLOCK LEVELS
        for (int i = 0; i < _dunkLevelsButtons.Length; i++)
        {
            if (i == 0)
                _dunkLevelsButtons[i].interactable = true;
            else
                _dunkLevelsButtons[i].interactable = SaveAndLoadManager.ContainsKey(
                    SaveAndLoadManager.DunkLevelName + i);
        }
        #endregion
        #region WITHOUT DEATH
        for (int i = 0; i < _dunkWithoutDeath.Length; i++)
        {
            _dunkWithoutDeath[i].gameObject.SetActive(_dunkWithoutDeath[i]
                && SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkWithoutDeathName + i) == 1);
        }
        #endregion
        #endregion
    }

    public void ResetPlayerPrefs()
    {
        SaveAndLoadManager.DeleteData();
    }
}
