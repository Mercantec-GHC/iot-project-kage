#include <Arduino.h>
#include "configuration.h"

namespace program
{
    void setup()
    {

    }

    void loop()
    {
        Serial.println();
        Serial.print("Device Id: ");
        Serial.println(configuration::get("device_id"));
        Serial.print("API Key: ");
        Serial.println(configuration::get("api_key"));

        delay(1000);
    }
}