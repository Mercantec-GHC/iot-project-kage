#include <Arduino.h>
#include <Arduino_MKRIoTCarrier.h>

#include "configuration.h"
#include "bluetooth.h"

MKRIoTCarrier carrier;

bool isBluetoothEnabled = false;

String httpFunction(const String &message)
{
    // Indsæt her din HTTP-logik (fx med WiFiClient eller lignende).
    // For eksemplets skyld simuleres et svar:
    delay(100); // Simulerer netværksforsinkelse.
    return "HTTP-svar for: " + message;
}

void setup()
{
    Serial.begin(9600);
    while (!Serial)
        ;

    carrier.noCase();
    carrier.begin();
    configuration::begin();

    // carrier.Buttons.update();
    // bool reset = carrier.Buttons.getTouch(TOUCH0);
    // Serial.println(reset);

    if (!configuration::read())
    {
        // configuration::set("device_id", "TestId");
        // configuration::set("api_key", "TestKey");
        // configuration::save();
    }

    bluetooth::setup();
    isBluetoothEnabled = true;

    // Kald vores opsætningsfunktion for BLE.
    // bluetooth::setup();
}

void loop()
{
    // Processér BLE-hændelser og håndter beskeder.
    // bluetooth::process();

    if (isBluetoothEnabled)
    {
        bluetooth::process(httpFunction);
        // if (received.length() > 0)
        // {
        //     // Her kan du f.eks. behandle beskeden.
        //     Serial.print("process() aflevere besked: ");
        //     Serial.println(received);
        //     configuration::set("owner_id", received.c_str());
        //     configuration::save();
        //     isBluetoothEnabled = false;
        // }
    }
    else
    {
        Serial.println();
        Serial.print("Device Id: ");
        Serial.println(configuration::get("device_id"));
        Serial.print("API Key: ");
        Serial.println(configuration::get("api_key"));
        Serial.print("Owner Id: ");
        Serial.println(configuration::get("owner_id"));

        delay(1000);
    }
}
