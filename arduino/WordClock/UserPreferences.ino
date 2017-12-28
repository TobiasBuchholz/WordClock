struct UserPrefs {
  int colorR;
  int colorG;
  int colorB;
  int colorA;
  bool isNightModeEnabled;
  int nightModeBrightness;
  int nightModeFromTimeMillis;
  int nightModeToTimeMillis;
};

void saveUserPrefs() {
  UserPrefs prefs = { 
    colorRed, 
    colorGreen, 
    colorBlue,
    colorAlpha,
    isNightModeEnabled,
    nightModeBrightness,
    nightModeFromTimeMillis,
    nightModeToTimeMillis
  };
  
  EEPROM.begin(512);
  EEPROM.put(0, prefs);
  Serial.println("UserPrefs saved!");
  EEPROM.commit();
  EEPROM.end();
}

void loadUserPrefs() {
  UserPrefs prefs;
  EEPROM.begin(512);
  EEPROM.get(0, prefs);
  colorRed = prefs.colorR;
  colorGreen = prefs.colorG;
  colorBlue = prefs.colorB;
  colorAlpha = prefs.colorA;
  isNightModeEnabled = prefs.isNightModeEnabled;
  nightModeBrightness = prefs.nightModeBrightness;
  nightModeFromTimeMillis = prefs.nightModeFromTimeMillis;
  nightModeToTimeMillis = prefs.nightModeToTimeMillis;
    
  Serial.println("Initialize with colors: ");
  Serial.println(colorRed);
  Serial.println(colorGreen);
  Serial.println(colorBlue);
  EEPROM.end();
}

