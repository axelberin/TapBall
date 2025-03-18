using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public Action OnUpdateLanguage = delegate { };

    private Dictionary<string, string> _localizedTexts = new();
    private string _language = "en"; // Idioma por defecto
    private string _sheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSZpoJwa5CcFffrM8gBTesuOZY3UaizH6oVSAgHGDuKslJ45fE9ITGNiL_AP_qqdhtjZXm_LndbY5OV/pub?output=csv";
    private int _currentLanguageIndex = 0;
    private int _minLanguageIndex = 1;
    private int _maxLanguageIndex = 2;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(DownloadAndParseCSV());
    }

    private IEnumerator DownloadAndParseCSV()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_sheetUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                ParseCSV(request.downloadHandler.text);
            else
                Debug.LogError("Error al descargar el archivo CSV: " + request.error);
        }
    }

    private void ParseCSV(string csvText)
    {
        StringReader reader = new StringReader(csvText);
        string headerLine = reader.ReadLine(); // Ignorar la primera línea (cabeceras)

        string[] headers = headerLine.Split(',');

        int langIndex = 1; // Por defecto inglés

        for (int i = 1; i < headers.Length; i++)
        {
            Debug.Log(headers[i].Trim('"'));
            if (headers[i].Trim('"') == _language)
            {
                langIndex = i;
                break;
            }
        }

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');

            if (values.Length > langIndex)
            {
                string key = values[0].Trim('"');
                string value = values[langIndex].Trim('"');
                _localizedTexts[key] = value;
            }
        }

        Debug.Log("Traducciones cargadas correctamente.");
        OnUpdateLanguage?.Invoke();
    }

    public string GetLocalizedText(string key)
    {
        if (_localizedTexts.ContainsKey(key))
            return _localizedTexts[key];

        return "MISSING: " + key;
    }

    public void ChangeLanguage(int languageIndex)
    {
        SetCurrentLanguageIndex(languageIndex);
        _language = GetLanguageKeyFromIndex(_currentLanguageIndex);
        OnUpdateLanguage?.Invoke();
    }

    private void SetCurrentLanguageIndex(int index)
    {
        if (_currentLanguageIndex == 0 && index < 0)
            _currentLanguageIndex = _maxLanguageIndex;
        else if (_currentLanguageIndex == _maxLanguageIndex && index > 0)
            _currentLanguageIndex = _minLanguageIndex;
        else
            _currentLanguageIndex += index;
    }

    private string GetLanguageKeyFromIndex(int index)
    {
        return index switch
        {
            0 => "en",// Inglés
            1 => "es",// Español
            _ => "en",// Def
        };
    }
}

