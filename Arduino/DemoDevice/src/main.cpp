#include <Arduino.h>
#include <ArduinoJson.h>

#include "program.h"
#include "bluetooth.h"
#include "configuration.h"

bool isBluetoothEnabled = false;

bool shouldReset = false;
unsigned long resetTime = 0;

String bleConfigFunction(const String &message)
{
    JsonDocument doc;
    DeserializationError error = deserializeJson(doc, message);
    if (error)
        return "false";

    String id = doc["id"];
    String apiKey = doc["apiKey"];
    configuration::set("device_id", id.c_str());
    configuration::set("api_key", apiKey.c_str());
    configuration::save();

    shouldReset = true;
    resetTime = millis();

    return id;
}

void setup()
{
    Serial.begin(9600);
    // while (!Serial)
    //     ;

    configuration::begin();

    if (configuration::read())
    {
        program::setup();
    }
    else
    {
        bluetooth::setup();
        isBluetoothEnabled = true;
    }
}

void loop()
{
    if (shouldReset && (millis() - resetTime > 5000))
    {
        NVIC_SystemReset();
    }

    if (isBluetoothEnabled)
    {
        bluetooth::process(bleConfigFunction);
    }
    else
    {
        program::loop();
    }
}