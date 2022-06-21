using System.Text.Json;

namespace SimpleChatAppLibrary; 

internal class SimpleChatLibPrefs {
    private Dictionary<string, string>? _prefs;

    public string? GetString(string key) {
        if (_prefs == null) {
            // Load prefs from disk
            Load();
        }

        return !_prefs.ContainsKey(key) ? null : _prefs[key];
    }

    public string GetString(string key, string defaultValue) => GetString(key) ?? defaultValue;
    
    public void SetString(string key, string value) {
        if (_prefs == null) {
            // Load prefs from disk
            Load();
        }

        _prefs[key] = value;
    }
    
    // Load prefs function
    private void Load() {
        if (!File.Exists("chatdata.json")) {
            _prefs = new Dictionary<string, string>();
            return;
        }
        
        // Load prefs from disk
        _prefs = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("chatdata.json"));
        
    }
    
    // Save prefs function
    public void Save() {
        if (_prefs == null) {
            // Nothing to save
            return;
        }

        // Save prefs to disk
        File.WriteAllText("chatdata.json", JsonSerializer.Serialize(_prefs));
    }

}