#include <NTPClient.h>
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <time.h>

int ntpHour, ntpMinute, ntpSecond;
// Base UTC offset (for standard time)
const long utcOffsetInSeconds = 1 * 60 * 60; // 1 hour for Central European Time
// DST offset (additional hour when DST is active)
const long dstOffsetInSeconds = 1 * 60 * 60; // 1 additional hour during DST

WiFiUDP ntpUDP;
// Initialize with standard time offset
NTPClient timeClient(ntpUDP, "europe.pool.ntp.org", utcOffsetInSeconds);

// Function to check if DST is active in Europe
bool isDSTactive() {
  time_t now = time(nullptr);
  struct tm *timeinfo = localtime(&now);
  
  // European DST rules (simplified):
  // DST starts on last Sunday of March at 01:00 UTC
  // DST ends on last Sunday of October at 01:00 UTC
  
  if (timeinfo->tm_mon < 2 || timeinfo->tm_mon > 9) {
    // January, February, November, December - no DST
    return false;
  }
  
  if (timeinfo->tm_mon > 2 && timeinfo->tm_mon < 9) {
    // April to September - DST is always active
    return true;
  }
  
  // For March and October, we need to calculate if we're before or after the last Sunday
  int lastSunday = 31 - (timeinfo->tm_wday + 31 - timeinfo->tm_mday) % 7;
  
  if (timeinfo->tm_mon == 2) { // March
    // Before last Sunday = no DST, after = DST
    return (timeinfo->tm_mday > lastSunday || 
           (timeinfo->tm_mday == lastSunday && timeinfo->tm_hour >= 1));
  } else { // October
    // Before last Sunday = DST, after = no DST
    return (timeinfo->tm_mday < lastSunday || 
           (timeinfo->tm_mday == lastSunday && timeinfo->tm_hour < 1));
  }
}

void setupNTP() {
  timeClient.begin();
  // Set system time for DST calculation
  configTime(utcOffsetInSeconds, dstOffsetInSeconds, "europe.pool.ntp.org");
}

void fetchTimeFromNTP() {
  // Update the NTP client
  timeClient.update();
  
  // Get the base time
  ntpHour = timeClient.getHours();
  ntpMinute = timeClient.getMinutes();
  ntpSecond = timeClient.getSeconds();
  
  // Check if DST is active and adjust
  if (isDSTactive()) {
    // Apply DST correction
    ntpHour = (ntpHour + 1) % 24;
    
    Serial.print("DST is active, time adjusted. ");
  } else {
    Serial.print("Standard time. ");
  }
  
  // Print the time
  Serial.print("Current time is ");
  Serial.print(ntpHour);
  Serial.print(":");
  if (ntpMinute < 10) Serial.print("0");
  Serial.print(ntpMinute);
  Serial.print(":");
  if (ntpSecond < 10) Serial.print("0");
  Serial.println(ntpSecond);
}
