char board[] = "ESKISTLFUNFGIZNAWZNHEZDREIVIERTELMJROVHCANGTHALBQZWOLFPNEBEISNIEWZKDREIRHFUNFREIVNUENFLEWACHTZEHNRSRHUMFSHCESB";

int es[] = { 0, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
int ist[] = { 3, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
int fuenf_minutes[] = { 7, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1, -1 };
int zehn_minutes[] = { 21, 20, 19, 18, -1, -1, -1, -1, -1, -1, -1, -1 };
int zwanzig_minutes[] = { 17, 16, 15, 14, 13, 12, 11, -1, -1, -1, -1, -1 };
int dreiviertel[] = { 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, -1 };
int viertel[] = { 26, 27, 28, 29, 30, 31, 32, -1, -1, -1, -1, -1 };
int nach[] = { 41, 40, 39, 38, -1, -1, -1, -1, -1, -1, -1, -1 };
int vor[] = { 37, 36, 35, -1, -1, -1, -1, -1, -1, -1, -1, -1};
int halb[] = { 44, 45, 46, 47, -1, -1, -1, -1, -1, -1, -1, -1 };
int uhr[] = { 101, 100, 99, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
int ein[] = { 63, 62, 61, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

int hourMap[][12] = { { 63, 62, 61, 60, -1, -1, -1, -1, -1, -1, -1, -1 }, // eins
                      { 65, 64, 63, 62, -1, -1, -1, -1, -1, -1, -1, -1 }, // zwei
                      { 67, 68, 69, 70, -1, -1, -1, -1, -1, -1, -1, -1 }, // drei
                      { 80, 79, 78, 77, -1, -1, -1, -1, -1, -1, -1, -1 }, // vier
                      { 73, 74, 75, 76, -1, -1, -1, -1, -1, -1, -1, -1 }, // fuenf
                      { 104, 105, 106, 107, 108, -1, -1, -1, -1, -1, -1, -1 }, // sechs
                      { 60, 59, 58, 57, 56, 55, -1, -1, -1, -1, -1, -1 }, // sieben
                      { 89, 90, 91, 92, -1, -1, -1, -1, -1, -1, -1, -1 }, // acht
                      { 84, 83, 82, 81, -1, -1, -1, -1, -1, -1, -1, -1 }, // neun
                      { 93, 94, 95, 96, -1, -1, -1, -1, -1, -1, -1, -1 }, // zehn
                      { 87, 86, 85, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // elf
                      { 49, 50, 51, 52, 53, -1, -1, -1, -1, -1, -1, -1 } // zwoelf
                    };

Adafruit_NeoPixel pixels = Adafruit_NeoPixel(NUMPIXELS, PIN, NEO_GRB + NEO_KHZ800); 
bool pixelValues[NUMPIXELS];

void setupTimeDisplay() {
  pixels.begin();
}

void displayTimeSentence() {
  reset();
  setWord(es);
  setWord(ist);
  setMinute();
  setHour();
  setUhr();
  setPixelBrightness();
  showPixels();
  Serial.println();
}

void reset() {
  for(int i=0; i<NUMPIXELS; i++) {
    pixelValues[i] = false;
  }
}

void setWord(int wrd[]) {
  for (int i=0; i< 12; i++) {
    int pixel = wrd[i];
    if(pixel >= 0 && pixel < 111) {
      pixelValues[pixel] = true;
      Serial.print(board[pixel]);
    }
  }
  Serial.print(" ");
}

void setMinute() {
  if (ntpMinute > 2 && ntpMinute < 8) {
    setWord(fuenf_minutes);
    setWord(nach);
  } else if (ntpMinute > 7 && ntpMinute < 13) {
    setWord(zehn_minutes);
    setWord(nach);
  } else if (ntpMinute > 12 && ntpMinute < 18) {
    setWord(viertel);
  } else if (ntpMinute > 17 && ntpMinute < 23) {
    setWord(zehn_minutes);
    setWord(vor);
    setWord(halb);
  } else if (ntpMinute > 22 && ntpMinute < 28) {
    setWord(fuenf_minutes);
    setWord(vor);
    setWord(halb);
  } else if (ntpMinute > 27 && ntpMinute < 33) {
    setWord(halb);
  } else if (ntpMinute > 32 && ntpMinute < 38) {
    setWord(fuenf_minutes);
    setWord(nach);
    setWord(halb);
  } else if (ntpMinute > 37 && ntpMinute < 43) {
    setWord(zehn_minutes);
    setWord(nach);
    setWord(halb);
  } else if (ntpMinute > 42 && ntpMinute < 48) {
    setWord(dreiviertel);
  } else if (ntpMinute > 47 && ntpMinute < 53) {
    setWord(zehn_minutes);
    setWord(vor);
  } else if (ntpMinute > 52 && ntpMinute < 58) {
    setWord(fuenf_minutes);
    setWord(vor);
  }
}

void setHour() {
  int h = ntpHour;
  if(h>12) h=h-12;
  if(h==0) h=12;
  
  if(h == 1 && ntpMinute < 3 || (h == 0 || h == 12) && ntpMinute > 57) {
    setWord(ein);
  } else if (ntpMinute >= 0 && ntpMinute < 13) {
    setWord(hourMap[h - 1]);
  } else {    
    setWord(hourMap[h == 12 ? 0 : h]);
  }
}

void setUhr() {
  if (ntpMinute > 57 || ntpMinute < 3) {
    setWord(uhr);
  }
}

void setPixelBrightness() {
  int currentTimeMillis = ntpHour*3600000 + ntpMinute*60000;
  
  if(isNightModeEnabled && 
    ((nightModeFromTimeMillis < nightModeToTimeMillis && currentTimeMillis > nightModeFromTimeMillis && currentTimeMillis < nightModeToTimeMillis) ||
     (nightModeFromTimeMillis > nightModeToTimeMillis && (currentTimeMillis > nightModeFromTimeMillis || currentTimeMillis < nightModeToTimeMillis)))) {
     pixels.setBrightness(nightModeBrightness);     
  } else {
    pixels.setBrightness(colorAlpha);      
  } 
}

void showPixels() {
  for(int i=0; i<NUMPIXELS; i++) {  
    pixels.setPixelColor(i, pixelValues[i] ? pixels.Color(colorRed, colorGreen, colorBlue) : pixels.Color(0, 0, 0)); 
  }
  pixels.show();
  Serial.println();
}

