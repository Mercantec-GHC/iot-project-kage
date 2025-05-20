#include "bluetooth.h"

// Opret objektet for service samt karakteristik uden for namespace-blokken,
// så de er tilgængelige i hele filen. Alternativt kan de placeres inde i namespace-blokken.
BLEService customService("19B10000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic rxCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214", BLEWrite, 120);
BLECharacteristic txCharacteristic("7c2d4e17-fd29-47d4-8e2a-0d64c65e96b7", BLERead | BLENotify, 120);

namespace bluetooth
{
    void setup()
    {
        Serial.println("Starter BLE...");

        if (!BLE.begin())
        {
            Serial.println("Fejl: BLE kunne ikke startes!");
            while (1);
        }

        // Sæt lokalnavn og annoncer service.
        BLE.setLocalName("KageIot_DemoDevice");
        BLE.setAdvertisedService(customService);

        // Tilføj karakteristik og service.
        customService.addCharacteristic(rxCharacteristic);
        customService.addCharacteristic(txCharacteristic);
        BLE.addService(customService);

        // Start advertising så centrale enheder kan opdage os.
        BLE.advertise();

        Serial.println("BLE er startet og annoncerer. Vent på forbindelse af en central...");
    }

    // Denne funktion kan kaldes i din main loop for at håndtere nye forbindelser og data.
    void process(HttpCallback httpCallback)
    {
        // Check om en central enhed forsøger at forbinde.
        BLEDevice central = BLE.central();

        if (central)
        {
            Serial.print("Forbundet til central: ");
            Serial.println(central.address());

            // Håndter data, mens central er forbundet.
            while (central.connected())
            {
                if (rxCharacteristic.written())
                {
                    int len = rxCharacteristic.valueLength();
                    char message[len + 1]; // Ekstra plads til null-terminator.

                    rxCharacteristic.readValue((uint8_t *)message, len);
                    message[len] = '\0'; // Sørg for, at strengen er null-termineret.

                    Serial.print("Modtaget besked: ");
                    Serial.println(message);

                    if (httpCallback != nullptr) {
                        String httpResponse = httpCallback(String(message));
                        Serial.print("HTTP-svar modtaget: ");
                        Serial.println(httpResponse);
            
                        // Send HTTP-svaret tilbage via BLE.
                        txCharacteristic.writeValue(httpResponse.c_str());
                    }
                    break;
                }
                delay(10); // Kort delay for at kunne fange skrivninger.
            }

            Serial.print("Central afbrudt: ");
            Serial.println(central.address());
        }
    }
}