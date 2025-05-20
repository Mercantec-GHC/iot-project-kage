#include <Arduino.h>
#include <Arduino_MKRIoTCarrier.h>
#include <WiFiNINA.h>
#include <ArduinoHttpClient.h>
#include <ArduinoJson.h>

#include "arduino_secrets.h"
#include "configuration.h"
#include "bluetooth.h"
#include "program.h"

MKRIoTCarrier carrier;

char ssid[] = SECRET_SSID;
char pass[] = SECRET_PASS;
int status = WL_IDLE_STATUS;

WiFiClient wifi;
HttpClient client = HttpClient(wifi, "10.133.51.113", 6970);

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

    carrier.noCase();
    carrier.begin();
    configuration::begin();

    if (configuration::read())
    {
        while (status != WL_CONNECTED)
        {
            status = WiFi.begin(ssid, pass);
        }
        Serial.println("WiFi Connected.");
        Serial.print("IP: ");
        Serial.println(WiFi.localIP());

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
