#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <WebSocketsServer.h>
#include <WiFiUdp.h>
#include <RTClib.h>
#include <Adafruit_NeoPixel.h>
#include <EEPROM.h>

#define PIN 5
#define NUMPIXELS 110

int colorAlpha = 32;
int colorRed = 255;
int colorGreen = 255;
int colorBlue = 255;
bool isNightModeEnabled = false;
int nightModeBrightness = 1;
int nightModeFromTimeMillis = 3600000;
int nightModeToTimeMillis = 28800000;

unsigned long previousMillis = 0;
unsigned long previousMillisPixels = 0;
bool isConnectedToSocket = false;

void setup() {
  Serial.begin(115200);
  loadUserPrefs();
  connectToWifi();
  setupTimeDisplay();
  setupNTP();
  setupSocket();
}

void loop() {
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis >= 10000) {
    previousMillis = currentMillis;
    fetchTimeFromNTP();
    displayTimeSentence();
  }

  if (isConnectedToSocket && currentMillis - previousMillisPixels >= 50) {
    previousMillisPixels = currentMillis;
    displayTimeSentence();
  }

  loopSocket();
}
