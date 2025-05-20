#include <Arduino.h>
#include <WiFiNINA.h>
#include <ArduinoHttpClient.h>
#include <Arduino_MKRIoTCarrier.h>

#include "configuration.h"
#include "arduino_secrets.h"

namespace program
{
    void HttpDataRequest(String);
    void HttpGetLedConfigRequest();

    MKRIoTCarrier carrier;
    WiFiClient wifi;
    HttpClient client = HttpClient(wifi, "10.133.51.113", 6970);

    char ssid[] = SECRET_SSID;
    char pass[] = SECRET_PASS;
    int status = WL_IDLE_STATUS;

    // For data handeling.
    float lastTempC;
    unsigned long lastPrintTime;
    const unsigned long printInterval = 300000;

    // For led configuration handeling.
    unsigned long lastGetRequestTime;
    const unsigned long GetPrintInterval = 10000;
    int32_t UnixTimeStamp;

    void setup()
    {
        carrier.noCase();
        carrier.begin();
        while (status != WL_CONNECTED)
        {
            status = WiFi.begin(ssid, pass);
        }
        Serial.println("WiFi Connected.");
        Serial.print("IP: ");
        Serial.println(WiFi.localIP());
    }

    void loop()
    {
        float tempC = carrier.Env.readTemperature();
        float tempF = tempC * 9.0 / 5.0 + 32.2;
        float humidity = carrier.Env.readHumidity();
        unsigned long currenTime = millis();

        bool significantChange = abs(tempC - lastTempC) >= 5.0;
        bool timeElapsed = currenTime - lastPrintTime >= printInterval;

        bool getTimeElapsed = currenTime - lastGetRequestTime >= GetPrintInterval;

        if (getTimeElapsed || lastGetRequestTime == 0) {
            HttpGetLedConfigRequest();
            lastGetRequestTime = currenTime;
        }

        if (significantChange || timeElapsed || lastPrintTime == 0) {
            JsonDocument jsonDoc;
            JsonObject temp = jsonDoc.createNestedObject("temperature");
            temp["celsius"] = tempC;
            temp["fahrenheit"] = tempF;
            jsonDoc["humidity"] = humidity;

            String jsonString;
            serializeJson(jsonDoc, jsonString);

            HttpDataRequest(jsonString);

            Serial.println(jsonString);

            lastTempC = tempC;
            lastPrintTime = currenTime;
        }
    }

    // Sends sensor data with HTTP request.
    void HttpDataRequest(String jsonBody) {
        Serial.println("Sending data :D");

        // Building the POST request.
        client.beginRequest();
        client.post("/device/postdata");
        client.sendHeader("Content-Type", "application/json");
        client.sendHeader("Content-Length", jsonBody.length());
        client.sendHeader("deviceId", configuration::get("device_id"));
        client.sendHeader("apiKey", configuration::get("api_key"));
        client.beginBody();
        client.print(jsonBody);
        client.endRequest();

        // Status code and response handeling.
        int statusCode = client.responseStatusCode();
        String response = client.responseBody();
        Serial.print("StatusCode: ");
        Serial.println(statusCode);
        Serial.print("Response: ");
        Serial.println(response);
    }

    // Send a get request to get configuration for leds.
    void HttpGetLedConfigRequest() {
        // Build the get request
        client.beginRequest();
        client.get("/device/getconfiguration");
        client.sendHeader("DeviceId", configuration::get("device_id"));
        client.sendHeader("ApiKey", configuration::get("api_key"));
        client.endRequest();
        int statusCode = client.responseStatusCode();
        String response = client.responseBody();
        Serial.print("Status code: ");
        Serial.println(statusCode);
        Serial.print("Response: ");
        Serial.println(response);
        
        // Deserialization of response.
        JsonDocument jsonObject;
        DeserializationError error = deserializeJson(jsonObject, response);
        if (error) {
            Serial.print("Failed to parse JSON: ");
            Serial.println(error.c_str());
            return;
        }

        int r;
        int g;
        int b;

        // Gets and sets led configuration. 
        if (jsonObject.containsKey("timestamp")) {
            int32_t timeStamp = jsonObject["timestamp"].as<int32_t>();
            if (UnixTimeStamp < timeStamp || isnan(UnixTimeStamp)) {
                UnixTimeStamp = timeStamp;
                if (jsonObject["config"].containsKey("led_1")) {
                    r = jsonObject["config"]["led_1"]["r"].as<int>();
                    g = jsonObject["config"]["led_1"]["g"].as<int>();
                    b = jsonObject["config"]["led_1"]["b"].as<int>();
                    carrier.leds.setPixelColor(0, carrier.leds.Color(r, g, b));
                    carrier.leds.show();
                }
                if (jsonObject["config"].containsKey("led_2")) {
                    r = jsonObject["config"]["led_2"]["r"].as<int>();
                    g = jsonObject["config"]["led_2"]["g"].as<int>();
                    b = jsonObject["config"]["led_2"]["b"].as<int>();
                    carrier.leds.setPixelColor(1, carrier.leds.Color(r, g, b));
                    carrier.leds.show();
                }
                if (jsonObject["config"].containsKey("led_3")) {
                    r = jsonObject["config"]["led_3"]["r"].as<int>();
                    g = jsonObject["config"]["led_3"]["g"].as<int>();
                    b = jsonObject["config"]["led_3"]["b"].as<int>();
                    carrier.leds.setPixelColor(2, carrier.leds.Color(r, g, b));
                    carrier.leds.show();
                }
                if (jsonObject["config"].containsKey("led_4")) {
                    r = jsonObject["config"]["led_4"]["r"].as<int>();
                    g = jsonObject["config"]["led_4"]["g"].as<int>();
                    b = jsonObject["config"]["led_4"]["b"].as<int>();
                    carrier.leds.setPixelColor(3, carrier.leds.Color(r, g, b));
                    carrier.leds.show();
                }
                if (jsonObject["config"].containsKey("led_5")) {
                    r = jsonObject["config"]["led_5"]["r"].as<int>();
                    g = jsonObject["config"]["led_5"]["g"].as<int>();
                    b = jsonObject["config"]["led_5"]["b"].as<int>();
                    carrier.leds.setPixelColor(4, carrier.leds.Color(r, g, b));
                    carrier.leds.show();
                }
            }
        }     
    }
}