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

        OnUpdateLanguage?.Invoke();
    }


    public string GetLocalizedText(string key)
    {
        if (_localizedTexts[_currentLanguage].ContainsKey(key))
            return _localizedTexts[_currentLanguage][key];

        return "MISSING: " + key;
    }

    public void ChangeLanguage(int languageIndex)
    {
        SetCurrentLanguageIndex(languageIndex);
        _currentLanguage = GetLanguageKeyFromIndex(_currentLanguageIndex);
        OnUpdateLanguage?.Invoke();
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
            0 => "en",// Inglés
            1 => "es",// Español
            _ => "en",// Def
        };
    }
}

