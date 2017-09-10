#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <WiFiServer.h>

#define PIN1 D0
#define PIN2 D1
#define PIN3 D2

#define LEFT_IP 151
#define RIGHT_IP 152

#define CMD_SET 0xA0
#define CMD_READ 0xA1

#define P1 0xB1
#define P2 0xB2
#define P3 0xB3

#define P_HIGH 0xFF
#define P_LOW 0xF0

#define ssid "Spinnennetz"
#define password "MAXIMILIAN456"

WiFiServer server(8888);
WiFiClient client;

IPAddress ip(192, 168, 178, LEFT_IP);
IPAddress dns(192, 168, 178, 1);
IPAddress gateway(192, 168, 178, 1);
IPAddress subnet(255, 255, 255, 0);

void setup() {

  WiFi.begin(ssid, password);
  WiFi.config(ip, dns, gateway, subnet);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
  }

  pinMode(PIN1, OUTPUT);
  pinMode(PIN2, OUTPUT);
  pinMode(PIN3, OUTPUT);

  Serial.begin(115200);

  server.begin();
}

void loop() {
  client = server.available();
  if (client) {
    Serial.println("Established connection");
    client.flush();
    while (client.connected()) {
      if (client.available() > 2) {

        Serial.print("Received command");
        byte command = client.read();
        byte param1 = client.read();
        byte param2 = client.read();

        if (command == CMD_SET) {
          Serial.print(" set");
          setPin(param1, param2);
        } else if (command == CMD_READ) {
          Serial.println(" read");
          readPin(client, param1);
        }
      }
    }
    Serial.println("Connection lost");
  }

}

void setPin(byte pin, byte value) {

  int pinToSet = decodePin(pin);

  Serial.print("Setting pin ");
  Serial.print(pinToSet);
  Serial.print(" to");

  if (value == P_HIGH) {
    Serial.println(" HIGH");
    digitalWrite(pinToSet, HIGH);
  } else {
    Serial.println(" LOW");
    digitalWrite(pinToSet, LOW);
  }
}

void readPin(WiFiClient c, byte param) {
  int pinToRead = decodePin(param);
  Serial.print("Reading pin ");
  Serial.print(pinToRead);
  Serial.print(". Value: ");
  if (digitalRead(pinToRead) == HIGH) {
    Serial.println("HIGH");
    c.write(P_HIGH);
  } else {
    Serial.println("LOW");
    c.write(P_LOW);

  }
}

int decodePin(byte param) {
  int pinToRead = PIN1;
  switch (param) {
    case P2:
      pinToRead = PIN2;
      break;
    case P3:
      pinToRead = PIN3;
      break;
  }
  return pinToRead;
}


