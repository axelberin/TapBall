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

    private Dictionary<string, Dictionary<string, string>> _localizedTexts = new();
    private string _currentLanguage = "en"; // Idioma por defecto
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

        // Leer la primera línea para obtener los idiomas disponibles
        string headerLine = reader.ReadLine();
        string[] headers = headerLine.Split(',');

        // Inicializar diccionarios por idioma
        for (int i = 1; i < headers.Length; i++) // Comenzamos en 1 porque la primera columna es la clave del texto
        {
            string language = headers[i].Trim('"');
            if (!_localizedTexts.ContainsKey(language))
                _localizedTexts.Add(language, new Dictionary<string, string>()); // Agregamos un diccionario vacio
        }

        // Iterar sobre cada línea del CSV
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');

            string key = values[0].Trim('"'); // Primera columna es la clave del texto

            for (int i = 1; i < headers.Length; i++)
            {
                string language = headers[i].Trim('"'); // Idioma en la cabecera
                string translation = (i < values.Length) ? values[i].Trim('"') : "";

                if (!_localizedTexts[language].ContainsKey(key))
                    _localizedTexts[language].Add(key, translation);
                else
                    _localizedTexts[language][key] = translation;
            }
        }

        Debug.Log("Traducciones cargadas correctamente.");

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.LanguageName))
        {
            _currentLanguage = GetLanguageFromDevice();
            SaveAndLoadManager.SetStringValue(_currentLanguage, SaveAndLoadManager.LanguageName, true);
        }
        else
            _currentLanguage = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.LanguageName);

        _currentLanguageIndex = GetLanguageIndexFromLanguage(_currentLanguage);
        OnUpdateLanguage?.Invoke();
    }

    private string GetLanguageFromDevice()
    {
        SystemLanguage language = Application.systemLanguage;
        Debug.Log("Idioma del sistema: " + language);

        // Puedes hacer algo específico según el idioma detectado
        return language switch
        {
            SystemLanguage.Spanish => GetLanguageKeyFromIndex(1),
            SystemLanguage.English => GetLanguageKeyFromIndex(0),
            _ => GetLanguageKeyFromIndex(1),
        };
    }

    public string GetLocalizedText(string key)
    {
        if (_localizedTexts.Count == 0 ||
            _localizedTexts[_currentLanguage] == null ||
            _localizedTexts[_currentLanguage].Count == 0 ||
            _localizedTexts[_currentLanguage][key] == null)
            return key;

        if (_localizedTexts[_currentLanguage].ContainsKey(key))
            return _localizedTexts[_currentLanguage][key];

        return key;
    }

    public void ChangeLanguage(int languageIndex)
    {
        SetCurrentLanguageIndex(languageIndex);
        _currentLanguage = GetLanguageKeyFromIndex(_currentLanguageIndex);
        OnUpdateLanguage?.Invoke();
        SaveAndLoadManager.SetStringValue(_currentLanguage, SaveAndLoadManager.LanguageName);
    }

    private void SetCurrentLanguageIndex(int index)
    {
        if (_currentLanguageIndex == _minLanguageIndex && index < 0)
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
            1 => "en",// Inglés
            2 => "es",// Español
            _ => "en",// Def
        };
    }

    private int GetLanguageIndexFromLanguage(string language)
    {
        return language switch
        {
            "en" => 1,
            "es" => 2,
            _ => 1,
        };
    }
}

