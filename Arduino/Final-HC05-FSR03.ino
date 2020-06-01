#include <SoftwareSerial.h>
#include <LiquidCrystal.h>

SoftwareSerial bluetooth(8,9);
LiquidCrystal lcd(12,11,7,6,5,4);

const byte TRIGGER_PIN = 2;
const byte ECHO_PIN = 3;
const byte FRS_PIN = A0;

const unsigned long MEASURE_TIMEOUT = 25000UL;
const float SOUND_SPEED = 340.0 / 1000; // work in ms/m
const int RESISTOR = 10000; 

long measure;
float distance_cm;
int fsrReading;
int fsrVoltage;
unsigned long fsrResistance;
unsigned long fsrConductance;
long fsrForce;
String msgUltrason;
String msgFsr;

void setup() {
  bluetooth.begin(9600);
  lcd.begin(16,2);

  pinMode(TRIGGER_PIN, OUTPUT);
  digitalWrite(TRIGGER_PIN, LOW);
  pinMode(ECHO_PIN, INPUT);
}

void loop() {

  digitalWrite(TRIGGER_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIGGER_PIN, LOW);

  measure = pulseIn(ECHO_PIN, HIGH, MEASURE_TIMEOUT);
  distance_cm = measure / 2.0 * SOUND_SPEED / 10;

  msgUltrason = "cm = " + String(distance_cm);

  fsrReading = analogRead(FRS_PIN);
  fsrVoltage = map(fsrReading, 0, 1023, 0, 5000);
  if(fsrVoltage == 0) {
    msgFsr = "Force = 0 N";
  } else {
    fsrResistance = RESISTOR * 5000.0 / fsrVoltage - RESISTOR;
    fsrConductance = 1000000 / fsrResistance;

    if (fsrConductance <= 1000) {
      fsrForce = fsrConductance / 80;     
    } else {
      fsrForce = fsrConductance - 1000;
      fsrForce /= 30;    
    }
    
    msgFsr = "Force = " + String(fsrForce) + " N";
  }
    
  lcd.clear();
  lcd.setCursor(0,0);
  lcd.print(msgUltrason);
  bluetooth.println(msgUltrason);
  lcd.setCursor(0,1);
  lcd.print(msgFsr);
  bluetooth.println(msgFsr);

  delay(80);
}
