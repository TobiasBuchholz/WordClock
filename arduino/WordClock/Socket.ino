WebSocketsServer webSocket = WebSocketsServer(81);

void webSocketEvent(uint8_t num, WStype_t type, uint8_t * payload, size_t lenght) {

  switch(type) {
    case WStype_DISCONNECTED:
      isConnectedToSocket = false;
      break;
    case WStype_CONNECTED:
    {
      IPAddress ip = webSocket.remoteIP(num);
      isConnectedToSocket = true;
    }
      break;
    case WStype_TEXT:
    {
      String text = String((char *) &payload[0]);

      if(text.startsWith("a")) {
        colorAlpha = text.substring(text.indexOf("a")+1, text.indexOf("r")).toInt();
        colorRed = text.substring(text.indexOf("r")+1, text.indexOf("g")).toInt();
        colorGreen = text.substring(text.indexOf("g")+1, text.indexOf("b")).toInt();
        colorBlue = text.substring(text.indexOf("b")+1, text.length()).toInt();        
      } else if(text.startsWith("nme")) {
        isNightModeEnabled = text.substring(text.indexOf("e")+1, text.length()).toInt();        
      } else if(text.startsWith("nb")) {
        nightModeBrightness = text.substring(text.indexOf("b")+1, text.length()).toInt();        
      } else if(text.startsWith("nft")) {
        nightModeFromTimeMillis = text.substring(text.indexOf("t")+1, text.length()).toInt();        
      } else if(text.startsWith("ntt")) {
        nightModeToTimeMillis = text.substring(text.lastIndexOf("t")+1, text.length()).toInt();        
      }
    }
      webSocket.sendTXT(num, payload, lenght);
      webSocket.broadcastTXT(payload, lenght);
      break;    
    case WStype_BIN:
      hexdump(payload, lenght);
      
      // echo data back to browser
      webSocket.sendBIN(num, payload, lenght);
      break;
    }
}

void setupSocket() {
  webSocket.begin();
  webSocket.onEvent(webSocketEvent);
}

void loopSocket() {
  webSocket.loop();
}
