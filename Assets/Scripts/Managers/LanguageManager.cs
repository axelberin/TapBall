using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;
using UtilityAddressables;

public class LanguageManager : ManagersManager
{
    public static LanguageManager Instance;

    public Action OnUpdateLanguage = delegate { };

    private TMP_FontAsset _commonFont;
    private TMP_FontAsset _japaneseFont;
    private TMP_FontAsset _chineseFont;
    private TMP_FontAsset _numbersFont;

    private Dictionary<string, Dictionary<string, string>> _localizedTexts = new();
    private string _currentLanguage = "en"; // Idioma por defecto
    private string _sheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSZpoJwa5CcFffrM8gBTesuOZY3UaizH6oVSAgHGDuKslJ45fE9ITGNiL_AP_qqdhtjZXm_LndbY5OV/pub?output=csv";
    private int _currentLanguageIndex = 0;
    private int _minLanguageIndex = 1;
    private int _maxLanguageIndex = 8;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        StringReader reader = new(csvText);

        // Leer la primera línea para obtener los idiomas disponibles
        string headerLine = reader.ReadLine();
        string[] headers = headerLine.Split(',');

        // Inicializar diccionarios por idioma
        for (int i = 1; i < headers.Length; i++) // Comenzamos en 1 porque la primera columna es la clave del texto
        {
            string language = headers[i].Trim('"');
            if (!_localizedTexts.ContainsKey(language))
                _localizedTexts.Add(language, new()); // Agregamos un diccionario vacio
        }

        // Iterar sobre cada línea del CSV
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            string[] values = Regex.Matches(line, "(?:^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)")
                       .Cast<Match>()
                       .Select(m => m.Value.TrimStart(',').Trim('"'))
                       .ToArray();

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

        LoadSavedOrDetectLanguage();
    }

    public void LoadSavedOrDetectLanguage()
    {
        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.LanguageName))
        {
            // Primera vez - detectar idioma del dispositivo
            _currentLanguage = GetLanguageFromDevice();
            Debug.Log($"Detected device language: {_currentLanguage}");
        }
        else
        {
            // Cargar idioma guardado
            string savedLanguage = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.LanguageName);
            _currentLanguage = savedLanguage;
            Debug.Log($"Loaded saved language: {_currentLanguage}");
        }
        SaveAndLoadManager.SetStringValue(_currentLanguage, SaveAndLoadManager.LanguageName, true);

        _currentLanguageIndex = GetLanguageIndexFromLanguage(_currentLanguage);
        OnUpdateLanguage?.Invoke();
    }


    private string GetLanguageFromDevice()
    {
        SystemLanguage language = Application.systemLanguage;
        Debug.Log("Idioma del sistema: " + language);

        return language switch
        {
            SystemLanguage.English => GetLanguageKeyFromIndex(1),
            SystemLanguage.Spanish => GetLanguageKeyFromIndex(2),
            SystemLanguage.Italian => GetLanguageKeyFromIndex(3),
            SystemLanguage.German => GetLanguageKeyFromIndex(4),
            SystemLanguage.French => GetLanguageKeyFromIndex(5),
            SystemLanguage.Portuguese => GetLanguageKeyFromIndex(6),
            SystemLanguage.Chinese => GetLanguageKeyFromIndex(7),
            SystemLanguage.Japanese => GetLanguageKeyFromIndex(8),
            _ => GetLanguageKeyFromIndex(1),
        };
    }

    public (string, TMP_FontAsset) GetlocalizatedTextAndFont(string key)
        => (GetLocalizedText(key), GetFontByLanguage());

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

    public TMP_FontAsset GetFontByLanguage(bool isNumber = false)
    {
        if (isNumber)
            return _numbersFont;

        return _currentLanguage switch
        {
            "ch" => _chineseFont,
            "jp" => _japaneseFont,
            _ => _commonFont,
        };
    }

    public void ChangeLanguage(int languageIndex)
    {
        SetCurrentLanguageIndex(languageIndex);
        _currentLanguage = GetLanguageKeyFromIndex(_currentLanguageIndex);
        OnUpdateLanguage?.Invoke();
        SaveAndLoadManager.SetStringValue(_currentLanguage, SaveAndLoadManager.LanguageName, true);
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
            3 => "it",// Italiano
            4 => "al",//Alemán
            5 => "fr",//Francés
            6 => "br",//Portugués/Brasilero
            7 => "ch",//Chino Tradicional
            8 => "jp",//Japonés
            _ => "en",// Def
        };
    }

    public int GetLanguageIndexFromLanguage(string language)
    {
        return language switch
        {
            "en" => 1,
            "es" => 2,
            "it" => 3,
            "al" => 4,
            "fr" => 5,
            "br" => 6,
            "ch" => 7,
            "jp" => 8,
            _ => 1,
        };
    }

    public override IEnumerator InizializeManagers()
    {
        AddressablesUtility.LoadAsset<TMP_FontAsset>("CommonFont", font => _commonFont = font);
        AddressablesUtility.LoadAsset<TMP_FontAsset>("JapaneseFont", font => _japaneseFont = font);
        AddressablesUtility.LoadAsset<TMP_FontAsset>("ChineseFont", font => _chineseFont = font);
        AddressablesUtility.LoadAsset<TMP_FontAsset>("NumbersFont", font => _numbersFont = font);

        yield return StartCoroutine(DownloadAndParseCSV());

        yield return new WaitForSeconds(1f);

        _isInitialized = true;
    }
}

