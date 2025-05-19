#include <Arduino.h>
#include <WiFiNINA.h>
#include <ArduinoJson.h>
#include <Arduino_MKRIoTCarrier.h>
#include "arduino_secrets.h"
#include <SD.h>
#include <ArduinoHttpClient.h>

// Instatiate functions. 
void Register();
bool ReadConfig();
void SaveConfig(String);
void HttpDataRequest(String);
void HttpGetLedConfigRequest();
String HttpRegisterRequest(String);

// For Wifi connection.
char ssid[] = SECRET_SSID;
char pass[] = SECRET_PASS;
int status = WL_IDLE_STATUS;

// To save the configuration.
String Id;
String ApiKey;

// For files on SD card.
WiFiClient wifi;
#define CONFIG_LOCATION "/config.txt"
HttpClient client = HttpClient(wifi, "10.133.51.113", 6970);

// For sensors.  
MKRIoTCarrier carrier;
float lastTempC = NAN;
unsigned long lastPrintTime = 0;
const unsigned long printInterval = 300000;

int32_t UnixTimeStamp = NAN;

void setup() {
    // Put your setup code here, to run once:
    Serial.begin(9600);
    carrier.begin();
    if (SD.begin(SD_CS)) {
        Serial.println("SD card initialized :D");
    }

    // Attempting to connect to the network.
    while (status != WL_CONNECTED) {
        status = WiFi.begin(ssid, pass);
    }
    Serial.println("Connected to WiFi :D");
    Serial.println(WiFi.localIP());

    // Chekcs for config file and proper information.
    if (!ReadConfig()) {
        // Registeres the device on the server and calls the HttpRequest() & SaveConfig() functions.
        Register();
    }
    HttpGetLedConfigRequest();
}

void loop() {
    // put your main code here, to run repeatedly:
    float tempC = carrier.Env.readTemperature();
    float tempF = tempC * 9.0 / 5.0 + 32.2;
    float humidity = carrier.Env.readHumidity();
    unsigned long currenTime = millis();

    bool significantChange = !isnan(lastTempC) && abs(tempC - lastTempC) >= 5.0;
    bool timeElapsed = currenTime - lastPrintTime >= printInterval;

    if (lastPrintTime >= 60000) {
        HttpGetLedConfigRequest();
    }

    if (significantChange || timeElapsed || isnan(lastTempC)) {
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

// Registeres the device on the server.
void Register() {
    DynamicJsonDocument JsonObject(512);

    // Add Key values to JSON object.
    JsonObject["devicetype"] = "DemoDevice";
    JsonObject["ownerid"] = "fb00f216-cf0e-4ff5-8885-4448a36020cc";

    // Serialize the JSON objewct to a string.
    String jsonBody;
    serializeJson(JsonObject, jsonBody);

    // Sends the HTTP request.
    String response = HttpRegisterRequest(jsonBody);

    // Sets the separation points for the HTTP response.
    int jsonStart = response.indexOf('{');
    int jsonEnd = response.lastIndexOf('}');

    // Deserialize the HTTP response and takes the id.
    if (jsonStart != -1 && jsonEnd != -1) {
        String jsonResponse = response.substring(jsonStart, jsonEnd + 1);
        SaveConfig(jsonResponse);
        ReadConfig();
    }
}

// Sends HTTP message, and returns the response.
String HttpRegisterRequest(String jsonBody) {
    Serial.println("Connecting to server.");
    
    // Send the HTTP request 
    client.beginRequest();
    client.post("/device/register");
    client.sendHeader("Content-Type", "application/json");
    client.sendHeader("Content-Length", jsonBody.length());
    client.beginBody();
    client.print(jsonBody);
    client.endRequest();

    int statusCode = client.responseStatusCode();
    String response = client.responseBody();
    Serial.print("StatusCode: ");
    Serial.println(statusCode);
    Serial.print("Response: ");
    Serial.println(response);
    return response;
}

// Makes a file and saves the device id, in the SD card, on the MKR IoT Carrier unit.
void SaveConfig(String json) {
    // Checks if the file exists.
    // if (SD.exists(CONFIG_LOCATION)) {
    //     if (SD.remove(CONFIG_LOCATION)) {
    //         Serial.println("Existing file removed successfully.");
    //     } else {
    //         Serial.println("Failed to remove existing file.");
    //     }
    // }

    // Creates the file with the CONFIG_LOCATION as the name of the file.
    File deviceFile = SD.open(CONFIG_LOCATION, FILE_WRITE);
    
    // Writes the json message in the file. 
    if (deviceFile) {
        deviceFile.println(json);
        deviceFile.close();
        Serial.println("File written successfully.");
    } else {
        Serial.println("Failed to write to file.");
    }

    // Checks if the file was created.
    if (SD.exists(CONFIG_LOCATION)) {
        Serial.println("File Exists :D");
    }

    // Reads the file, and prints the text on Serial monitor.
    deviceFile = SD.open(CONFIG_LOCATION, FILE_READ);
    while (deviceFile.available()) {
        String line = deviceFile.readStringUntil('\n');
        Serial.println(line);
    }
    deviceFile.close();
}

// Read the config file, returns true if successfull.
bool ReadConfig() {
    // Checks if the file exists.
    if (!SD.exists(CONFIG_LOCATION)) {
        return false;
    }

    // Reads the content in the file.
    File deviceFile = SD.open(CONFIG_LOCATION, FILE_READ);
    String jsonContent;
    while (deviceFile.available())
    {
        jsonContent = deviceFile.readStringUntil('\n');
    }
    deviceFile.close();

    // Deserialization of text, and check for error.
    DynamicJsonDocument jsonObject(512);
    DeserializationError error = deserializeJson(jsonObject, jsonContent);
    if (error) {
        Serial.print("Failed to parse JSON: ");
        Serial.println(error.c_str());
        return false;
    }

    // Checks for the key: "id", in the deserialized jsonObject.  
    if (jsonObject.containsKey("id")) {
        Id = jsonObject["id"].as<String>();
        Serial.print("Extracted Id: ");
        Serial.println(Id);
    } else {
        Serial.println("Key 'Id' not found in JSON.");
        return false;
    }

    // Checks for the key: "apiKey", in the deserialized jsonObject.  
    if (jsonObject.containsKey("apiKey")) {
        ApiKey = jsonObject["apiKey"].as<String>();
        Serial.print("Extracted ApiKey: ");
        Serial.println(ApiKey);
    } else {
        Serial.println("Key 'ApiKey' not found in JSON.");
        return false;
    }
    return true;
}

// Sends sensor data with HTTP request.
void HttpDataRequest(String jsonBody) {
    Serial.println("Sending data :D");

    client.beginRequest();
    client.post("/device/postdata");
    client.sendHeader("Content-Type", "application/json");
    client.sendHeader("Content-Length", jsonBody.length());
    client.sendHeader("deviceId", Id);
    client.sendHeader("apiKey", ApiKey);
    client.beginBody();
    client.print(jsonBody);
    client.endRequest();

    int statusCode = client.responseStatusCode();
    String response = client.responseBody();
    Serial.print("StatusCode: ");
    Serial.println(statusCode);
    Serial.print("Response: ");
    Serial.println(response);
}

void HttpGetLedConfigRequest() {
    Serial.println("Making get request");

    Serial.println(Id);
    Serial.println(ApiKey);
    client.beginRequest();
    client.get("/device/getconfiguration");
    client.sendHeader("DeviceId", Id);
    client.sendHeader("ApiKey", ApiKey);
    client.endRequest();
    int statusCode = client.responseStatusCode();
    String response = client.responseBody();
    Serial.print("Status code: ");
    Serial.println(statusCode);
    Serial.print("Response: ");
    Serial.println(response);
    
    JsonDocument jsonObject;
    DeserializationError error = deserializeJson(jsonObject, response);
    if (error) {
        Serial.print("Failed to parse JSON: ");
        Serial.println(error.c_str());
        return;
    }

    uint32_t R;
    uint32_t G;
    uint32_t B;
    if (jsonObject.containsKey("timestamp")) {
        int32_t timeStamp = jsonObject["timestamp"].as<int32_t>();
        if (UnixTimeStamp > timeStamp || isnan(UnixTimeStamp)) {
            if (jsonObject.containsKey("led_1")) {
                R = jsonObject["led_1"]["r"].as<int>();
                G = jsonObject["led_1"]["g"].as<int>();
                B = jsonObject["led_1"]["b"].as<int>();
                carrier.leds.setPixelColor(1, carrier.leds.Color(R, G, B));
                carrier.leds.show();
            }
            if (jsonObject.containsKey("led_2")) {
                R = jsonObject["led_2"]["r"].as<int>();
                G = jsonObject["led_2"]["g"].as<int>();
                B = jsonObject["led_2"]["b"].as<int>();
                carrier.leds.setPixelColor(2, R, G, B);
                carrier.leds.show();

            }
            if (jsonObject.containsKey("led_3")) {
                carrier.leds.setPixelColor(3, jsonObject["led_3"]["r"].as<int>(), jsonObject["led_3"]["g"].as<int>(), jsonObject["led_3"]["b"].as<int>());
                carrier.leds.show();
            }
            // if (jsonObject.containsKey("led_4")) {
            //     R = jsonObject["led_4"]["r"].as<int>();
            //     G = jsonObject["led_4"]["g"].as<int>();
            //     B = jsonObject["led_4"]["b"].as<int>();
            //     Led4 = carrier.leds.Color(R, G, B);
            //     Serial.print("led 4: ");
            //     Serial.println(Led4);
            // }
            // if (jsonObject.containsKey("led_5")) {
            //     R = jsonObject["led_5"]["r"].as<int>();
            //     G = jsonObject["led_5"]["g"].as<int>();
            //     B = jsonObject["led_5"]["b"].as<int>();
            //     Led5 = carrier.leds.Color(R, G, B);
            //     Serial.print("led 5: ");
            //     Serial.println(Led5);
            // }
            //   if (jsonObject.containsKey("display")) {
            //     if (jsonObject["display"]["sensor"] == "temp celcius") {
            //       text = jsonObject["display"]["sensor"].as<String>();
            //       text.toCharArray(ScreenDisplay, sizeof(ScreenDisplay));
            //       Serial.println(ScreenDisplay);
            //     } else if (jsonObject["display"]["sensor"] == "temp fahrenheit") {
            //       text = jsonObject["display"]["sensor"].as<String>();
            //       text.toCharArray(ScreenDisplay, sizeof(ScreenDisplay));
            //       Serial.println(ScreenDisplay);
            //     } else if (jsonObject["display"]["sensor"] == "humidity") {
            //       text = jsonObject["display"]["sensor"].as<String>();
            //       text.toCharArray(ScreenDisplay, sizeof(ScreenDisplay));
            //       Serial.println(ScreenDisplay);
            //     }
            //   }
        }
    }     
}