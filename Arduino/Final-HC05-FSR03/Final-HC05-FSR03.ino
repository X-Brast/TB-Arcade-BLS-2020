#include <SoftwareSerial.h>
#include <LiquidCrystal.h>

SoftwareSerial bluetooth(8,9);
LiquidCrystal lcd(12,11,7,6,5,4);

const byte TRIGGER_PIN_HC = 2;
const byte ECHO_PIN_HC = 3;
const byte FRS_PIN = A0;

const unsigned long MEASURE_TIMEOUT = 25000UL;
const float SOUND_SPEED = 340.0 / 1000; // work in ms/m
const int RESISTOR = 10000; 

long measureHC;
long distanceHC;
int fsrReading;
int fsrVoltage;
unsigned long fsrResistance;
unsigned long fsrConductance;
long fsrForce;
String msgUltrason;
String msgFsr;

boolean isConnect = false;
boolean isStartProtocole = false;
String nameBlocGame;
String nameArduino = "Anne";

unsigned long startTime;
unsigned long endTime;

// repris de https://stackoverflow.com/questions/29671455/how-to-split-a-string-using-a-specific-delimiter-in-arduino
String getValue(String data, char separator, int index)
{
  int found = 0;
  int strIndex[] = {0, -1};
  int maxIndex = data.length()-1;

  for(int i = 0; i <= maxIndex && found <= index; i++){
    if(data.charAt(i) == separator || i == maxIndex){
        found++;
        strIndex[0] = strIndex[1]+1;
        strIndex[1] = (i == maxIndex) ? i+1 : i;
    }
  }

  return found>index ? data.substring(strIndex[0], strIndex[1]) : "";
}

void initConnection(){
  lcd.clear();
  lcd.setCursor(0,0);
  lcd.print("Salut, je suis " + nameArduino);
  lcd.setCursor(0,1);
  lcd.print("Recherche en cours");
  if(!isConnect && bluetooth.available() > 0){
    isStartProtocole = true;
    String value = bluetooth.readString();
    lcd.clear();
    lcd.setCursor(0,0);
    lcd.print("Salut, je suis " + nameArduino);
    lcd.setCursor(0,1);
    lcd.print("Connexion en cours");
    if(value.equals("Hello, I search BLS device")){
      bluetooth.println("I Am BLS Device. My Name Is " + nameArduino + " Terminate.");
      while(isStartProtocole){
        if(bluetooth.available() > 0){
          value = bluetooth.readString();
          if(value.startsWith("Ok, my name is ")){
            nameBlocGame = getValue(value, ' ', 4);
            isConnect = true;
          }
          isStartProtocole = false;
        }
      }
    }
  }
  else{
    bluetooth.println("I am available");
  }
    
  delay(1000);
}

void setup() {
  bluetooth.begin(9600);
  lcd.begin(20,4);

  pinMode(TRIGGER_PIN_HC, OUTPUT);
  digitalWrite(TRIGGER_PIN_HC, LOW);
  pinMode(ECHO_PIN_HC, INPUT);
}

void loop() {
  if(isConnect){
    digitalWrite(TRIGGER_PIN_HC, HIGH);
    delayMicroseconds(50);
    digitalWrite(TRIGGER_PIN_HC, LOW);
    measureHC = pulseIn(ECHO_PIN_HC, HIGH, MEASURE_TIMEOUT);
    distanceHC = measureHC / 2.0 * SOUND_SPEED;
  
    msgUltrason = "mm = " + String(distanceHC);
  
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
    lcd.print("Salut, je suis " + nameArduino);
    lcd.setCursor(0,1);
    lcd.print("Avec " + nameBlocGame);
    lcd.setCursor(0,2);
    lcd.print(msgUltrason);
    //char* cMsgUltrason;
    //msgUltrason.toCharArray(cMsgUltrason, msgUltrason.length());
    //bluetooth.println(msgUltrason);
    lcd.setCursor(0,3);
    lcd.print(msgFsr);
    //char* cMsgFsr;
    //msgFsr.toCharArray(cMsgFsr, msgFsr.length());
    //bluetooth.println(msgFsr);
    String val = String(fsrForce) + ";" + String(distanceHC);
    bluetooth.println(val);

    startTime = millis();
    if(bluetooth.available() > 0){
      String value = bluetooth.readString();
      if(value.equals("End Connection")){
        isConnect = false;
        bluetooth.flush();
      }
      if(value.equals("ok")){
        endTime = millis();
      }
    }
    if(startTime >= (endTime + 60000)) {
      isConnect = false;
      bluetooth.flush();
    }
  
    delay(500);
  }
  else {
    initConnection();
    endTime = millis();
  }
}
