/*
  This sketch was written for an Arduino WiFi Rev2 and uses the
  - Uduino library
  - ArduinoBLE library version 1.2.1
  Make sure that the version of the ArduinoBLE library is correct 
  as otherwise you might not have enough flash memory available
  Connects with a Garmin Tacx Smart Trainer and prints out speed, front brake & rear brake force and resistance
  Can also write resistance to the trainer
*/
#include <Uduino.h>
#include <ArduinoBLE.h>  //Version 1.2.1 IMPORTANT


Uduino uduino("IndoorBikeData");  // Declare and name your object

uint8_t SpeedLSB = 0x00;
uint8_t SpeedMSB = 0x00;
uint16_t Speed = 0;
float SpeedO;

float ResistanceAngle = 0.0;
float steeringAngle = 0.0;

int FCPinit = 0;
uint8_t Resistance = 0;


int FrontBrakeForce = 0;
int RearBrakeForce = 0;
int CombinedBrakeForce = 0;

unsigned long lastMillisIDB;
unsigned long lastMillisSteer;
unsigned long lastMillisResistance;
void setup() {
  Serial.begin(115200);
  while (!Serial)
    ;
  lastMillisIDB = millis();


  // begin initialization
  if (!BLE.begin()) {
    uduino.println("starting BLE failed!");

    while (1)
      ;
  }

  uduino.println("BLE Central - Indoor Bike Data");
  uduino.println("Make sure to turn on the device.");

  // start scanning for peripheral
  BLE.scan();
  //SET PINS CORRECTLY
  pinMode(12, OUTPUT);  //Initiates Motor Channel A pin
  pinMode(9, OUTPUT);   //Initiates Brake Channel A pin
}
void loop() {

  uduino.update();
  if (uduino.isConnected() || 1 == 1) {

    // check if a peripheral has been discovered
    BLEDevice peripheral = BLE.available();

    if (peripheral) {
      // discovered a peripheral, print out address, local name, and advertised service
      uduino.print("Found ");
      uduino.print(peripheral.address());
      uduino.print(" '");
      uduino.print(peripheral.localName());
      uduino.print("' ");
      uduino.print(peripheral.advertisedServiceUuid());
      uduino.println();

      // Check if the peripheral is a Tacx Flux-2, the local name will be:
      if (peripheral.localName() == "Tacx Flux-2 03391") {
        // stop scanning

        BLE.stopScan();
        uduino.print("Tacx found");
        monitorIndoorBikeData(peripheral);

        // peripheral disconnected, start scanning again
        BLE.scan();
      }
    }
  }
}

void monitorIndoorBikeData(BLEDevice peripheral) {


  // connect to the peripheral
  uduino.println("Connecting ...");
  if (peripheral.connect()) {
    uduino.println("Connected");
  } else {
    uduino.println("Failed to connect!");
    return;
  }

  // discover peripheral attributes FTMS
  uduino.println("Discovering service 0x1826 ...");
  if (peripheral.discoverService("1826")) {
    uduino.println("Service discovered 1826");
  } else {
    uduino.println("Attribute discovery failed.");
    peripheral.disconnect();

    while (1)
      ;
    return;
  }

  // retrieve the simple key characteristic 2ad2 Indoor Bike Data
  BLECharacteristic indoorBikeDataCharacteristic = peripheral.characteristic("2ad2");

  // subscribe to the simple key characteristic
  uduino.println("Subscribing to 2ad2 Indoor Bike Data...");
  if (!indoorBikeDataCharacteristic) {
    uduino.println("no 2ad2 Indoor Bike Data found!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.canSubscribe()) {
    uduino.println("2ad2 Indoor Bike Data is not subscribable!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.subscribe()) {
    uduino.println("Sub to 2ad2 Indoor Bike Data failed!");
    peripheral.disconnect();
    return;
  } else {
    uduino.println("Subscribed");
    uduino.println("Get data from: 2ad2 Indoor Bike Data");
  }

  // retrieve the FitnessMachineControlPointCharacteristic characteristic
  BLECharacteristic FitnessMachineControlPointCharacteristic = peripheral.characteristic("2ad9");

  // subscribe to the simple key characteristic
  uduino.println("Subscribing to simple key characteristic 2ad9 FitnessMachineControlPointCharacteristic...");
  if (!indoorBikeDataCharacteristic) {
    uduino.println("no simple key characteristic 2ad9 FitnessMachineControlPointCharacteristic found!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.canSubscribe()) {
    uduino.println("simple key characteristic 2ad9 FitnessMachineControlPointCharacteristic is not subscribable!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.subscribe()) {
    uduino.println("subscription 2ad9 FitnessMachineControlPointCharacteristic failed!");
    peripheral.disconnect();
    return;
  } else {
    uduino.println("Subscribed");
    uduino.println("Empfange Daten von Characterisitic: 2ad9 FitnessMachineControlPointCharacteristic");
  }


  if (!FitnessMachineControlPointCharacteristic) {
    uduino.println("Peripheral does not have Fitness Machine Control Point Characteristic!");
    peripheral.disconnect();
    return;
  } else if (!FitnessMachineControlPointCharacteristic.canWrite()) {
    uduino.println("Peripheral does not have a writable Fitness Machine Control Point Characteristic!");
    peripheral.disconnect();
    return;
  }

  while (peripheral.connected()) {
    // while the peripheral is connected

    if ((millis() - lastMillisSteer) >= 25) {
      lastMillisSteer = millis();

      //SPEED
      uduino.print("speedOut ");
      uduino.print(SpeedO, 2);
      uduino.print(",");

      //Brakes
      uduino.print("frontbrake ");
      uduino.print(FrontBrakeForce);
      uduino.print(",");
      uduino.print("rearbrake ");
      uduino.print(RearBrakeForce);
      uduino.print(",");
      uduino.print("combined ");
      uduino.print(CombinedBrakeForce);
      uduino.print(",");
      //Resistance
      uduino.print("resistance ");
      uduino.print(Resistance);
      uduino.println("");
    }
    if ((millis() - lastMillisIDB) >= 95) {
      lastMillisIDB = millis();
      // yes, get the value, characteristic is 1 byte so use byte value
      byte value = 0;
      int descriptorValueSize = indoorBikeDataCharacteristic.valueSize();
      byte descriptorValue[descriptorValueSize];

      for (int i = 0; i < descriptorValueSize; i++) {
        descriptorValue[i] = indoorBikeDataCharacteristic.value()[i];

        if (i == 2) {
          SpeedLSB = indoorBikeDataCharacteristic.value()[i];
        }
        if (i == 3) {
          SpeedMSB = indoorBikeDataCharacteristic.value()[i];
          Speed = SpeedMSB;
          Speed <<= 8;
          Speed = Speed | SpeedLSB;
          SpeedO = Speed * 0.01;
        }

        RearBrakeForce = map(analogRead(A0), 590 , 765, 0, 100);  //Force of Front Brake
        FrontBrakeForce = map(analogRead(A1), 595, 815,0, 100);  //Force of Rear Brake
        CombinedBrakeForce = FrontBrakeForce + RearBrakeForce;
      }
    }

    if ((millis() - lastMillisResistance) >= 415) {
      lastMillisResistance = millis();

      if (FCPinit == 0) {  // Init FCP

        int i = 0;
        uint8_t FMCPCreset = 0x00;  // Reset FMCP
        byte FMCPCstart = 0x07;     // Start FMCP

        uduino.print("FMCPCreset: ");
        uduino.println(FMCPCreset, HEX);
        uduino.print("FMCPCstart: ");
        uduino.println(FMCPCstart, HEX);
        uduino.print("init = 0 reset");
        uduino.println(FMCPCreset);

        uint8_t value[2];
        FitnessMachineControlPointCharacteristic.readValue(value, 2);  //Assign value to FMCP
        uduino.print("Init Byte 1: ");
        uduino.print(value[0], HEX);
        uduino.print(" Init Byte 2: ");
        uduino.println(value[1], HEX);

        FitnessMachineControlPointCharacteristic.writeValue(FMCPCreset, 2);

        if (FitnessMachineControlPointCharacteristic.valueUpdated()) {
          uduino.println("Request Control");
          uint8_t value[3];
          FitnessMachineControlPointCharacteristic.readValue(value, 3);
          uduino.print("Byte 1: ");
          uduino.println(value[0], HEX);
          uduino.print("Byte 2: ");
          uduino.println(value[1], HEX);
          uduino.print("Byte 3: ");
          uduino.println(value[2], HEX);
        } else {
          uduino.println("no Request Control");
          uint8_t value[3];
          FitnessMachineControlPointCharacteristic.readValue(value, 3);
          uduino.print("Byte 1: ");
          uduino.println(value[0], HEX);
          uduino.print("Byte 2: ");
          uduino.println(value[1], HEX);
          uduino.print("Byte 3: ");
          uduino.println(value[2], HEX);
        }

        FitnessMachineControlPointCharacteristic.writeValue(FMCPCstart, 1);
        if (FitnessMachineControlPointCharacteristic.valueUpdated()) {
          uduino.println("Start Training");
          byte value = 0;
          FitnessMachineControlPointCharacteristic.readValue(value);
          uduino.print(" FMCPCstart Value : ");
          uduino.println(value, HEX);
        }

        FCPinit = 1;
        uduino.println("Init 1");
      }

      Resistance = map(CombinedBrakeForce, 0 , 100, 0, 10);  //angle für den Widerstand
      
      uint8_t OpCode = 0x04;
      int ResistanceSupport = Resistance * 60;
      int Divisor = 256;
      uint8_t MSB = 0;
      uint8_t LSB = 0;
      
      MSB = ResistanceSupport / Divisor;
      LSB = ResistanceSupport - (MSB * 256);

      uint8_t PotResistance[3] = {OpCode, LSB, MSB};
      
      FitnessMachineControlPointCharacteristic.writeValue(PotResistance, 3);  // 04E8030 100%
    }
  }
  uduino.println("Bike Trainer disconnected!");
}
