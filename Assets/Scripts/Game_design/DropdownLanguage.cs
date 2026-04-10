using UnityEngine;
using TMPro;
using YG;

namespace UI
{
    public class DropdownLanguage : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _dropdown;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _itemText;

        [Header("Translate")]
        [SerializeField] private string[] _ru = new string[2];
        [SerializeField] private string[] _en = new string[2];

        private void OnEnable()
        {
            YG2.onCorrectLang += OnSDKInitialized;
            YG2.onSwitchLang += OnLanguageSwitched;
        }

        private void OnDisable()
        {
            YG2.onCorrectLang -= OnSDKInitialized;
            YG2.onSwitchLang -= OnLanguageSwitched;
        }

        private void OnSDKInitialized(string lang)
        {
            InitializeDropdown(lang);
        }

        private void OnLanguageSwitched(string lang)
        {
            UpdateDropdownByLanguage(lang);
        }

        private void InitializeDropdown(string lang)
        {
            if (_dropdown == null || _ru == null || _en == null)
            {
                Debug.LogError("Dropdown or language arrays are not initialized.");
                return;
            }

            UpdateDropdownByLanguage(lang);
        }

        private void UpdateDropdownByLanguage(string lang)
        {
            switch (lang)
            {
                case "en":
                    _dropdown.value = 0;
                    SwitchLanguage(_en, _dropdown.value);
                    break;
                case "ru":
                    _dropdown.value = 1;
                    SwitchLanguage(_ru, _dropdown.value);
                    break;
                default:
                    Debug.LogWarning("Unknown language: " + lang + ". Using default.");
                    break;
            }
        }

        private void SwitchLanguage(string[] language, int index)
        {
            if (language.Length != _dropdown.options.Count)
            {
                Debug.LogError("Language array length does not match dropdown options count.");
                return;
            }

            for (int i = 0; i < language.Length; i++)
            {
                _dropdown.options[i].text = language[i];
            }

            _labelText.text = _dropdown.options[index].text;
        }

        public void InputLanguage(int value)
        {
            if (_dropdown == null || _ru == null || _en == null)
            {
                Debug.LogError("Dropdown or language arrays are not initialized.");
                return;
            }

            string selectedLang = "";
            string[] selectedArray = null;

            switch (value)
            {
                case 0:
                    selectedLang = "en";
                    selectedArray = _en;
                    break;
                case 1:
                    selectedLang = "ru";
                    selectedArray = _ru;
                    break;
                default:
                    Debug.LogError("Unknown language selection");
                    return;
            }

            if (selectedArray == null || selectedArray.Length == 0)
            {
                Debug.LogError("Language array is empty for: " + selectedLang);
                return;
            }

            YG2.SwitchLanguage(selectedLang);
            SwitchLanguage(selectedArray, value);
        }
    }
}