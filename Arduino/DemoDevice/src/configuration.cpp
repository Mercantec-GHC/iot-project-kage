#include "configuration.h"
#include <Arduino_MKRIoTCarrier.h>
#include <SD.h>

JsonDocument configDoc;

namespace configuration
{
    void begin() {
        if (SD.begin(SD_CS)) {
            Serial.println("SD card initialized :D");
            delay(1000);
        }
    }

    String get(const char *key)
    {
        if (!configDoc[key].isNull())
        {
            return configDoc[key].as<String>();
        }
        return "";
    }

    void set(const char *key, const char *value)
    {
        configDoc[key] = value;
    }

    bool save()
    {
        if (SD.exists(CONFIG_LOCATION)) {
            if (SD.remove(CONFIG_LOCATION)) {
                Serial.println("Existing file removed successfully.");
            } else {
                Serial.println("Failed to remove existing file.");
            }
        }

        // Åbn filen "/config.json" på SD-kortet til skrivning.
        File configFile = SD.open(CONFIG_LOCATION, FILE_WRITE);
        if (!configFile)
        {
            Serial.println("Kunne ikke åbne ");
            Serial.print(CONFIG_LOCATION);
            Serial.println(" til skrivning");
            return false;
        }

        // Serialiser JSON-dokumentet til filen.
        if (serializeJson(configDoc, configFile) == 0)
        {
            Serial.println("Fejl: kunne ikke skrive til ");
            Serial.print(CONFIG_LOCATION);
            configFile.close();
            return false;
        }

        configFile.close();
        Serial.println("Konfiguration gemt");
        return true;
    }

    bool read()
    {
        // Tjek om filen "/config.json" eksisterer på SD-kortet.
        if (!SD.exists(CONFIG_LOCATION))
        {
            Serial.print(CONFIG_LOCATION);
            Serial.println(" eksisterer ikke");
            return false;
        }

        File configFile = SD.open(CONFIG_LOCATION, FILE_READ);
        if (!configFile)
        {
            Serial.print("Kunne ikke åbne ");
            Serial.print(CONFIG_LOCATION);
            Serial.println(" til læsning");
            return false;
        }

        // Forsøg at parse JSON-indholdet fra filen.
        DeserializationError error = deserializeJson(configDoc, configFile);
        configFile.close();

        if (error)
        {
            Serial.print("Kunne ikke parse ");
            Serial.print(CONFIG_LOCATION);
            Serial.print(": ");
            Serial.println(error.c_str());
            return false;
        }

        Serial.println("Konfiguration læst");
        return true;
    }

} // namespace configuration