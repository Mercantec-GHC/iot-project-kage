#ifndef CONFIGURATION_H
#define CONFIGURATION_H

#include <Arduino.h>
#include <ArduinoJson.h>

#define CONFIG_LOCATION "config.txt"

// Vi indkapsler funktionerne i et namespace for at holde tingene overskuelige
namespace configuration {

  void begin();

  // Funktion til at hente værdien for en given nøgle fra JSON-konfigurationen.
  // Returnerer en String (kan evt. udvides med templated versioner til andre datatyper)
  String get(const char* key);

  // Funktion til at sætte (eller opdatere) værdien for en given nøgle.
  // Her benyttes ArduinoJson til at arbejde med JSON-dokumentet.
  void set(const char* key, const char* value);

  // Gemmer den nuværende konfiguration til fil (f.eks. på SPIFFS).
  // Returnerer true, hvis gemningen lykkedes.
  bool save();

  // Læser konfiguration fra fil og parser JSON-dataen ind i vores dokument.
  // Returnerer true, hvis konfigurationen blev læst korrekt.
  bool read();

}

#endif // CONFIGURATION_H