#ifndef BLUETOOTH_H
#define BLUETOOTH_H

#include <ArduinoBLE.h>

// Indkapsl funktionerne i et namespace for at undgå navnekonflikter med Arduino's egne setup/loop.
namespace bluetooth {

  // Funktioner der vil blive tilgængelige for main.

  // Definer en callback-type, der tager den modtagne besked (som String) og returnerer HTTP-svaret.
  typedef String (*HttpCallback)(const String &message);

  void setup();
  
  void process(HttpCallback httpCallback);  // Her kan du for eksempel håndtere forbindelser og beskeder.
  
}

#endif // BLUETOOTH_H