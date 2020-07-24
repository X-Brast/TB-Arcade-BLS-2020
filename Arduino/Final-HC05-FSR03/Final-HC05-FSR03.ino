/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        9 Juin 2020
 *
 * Fichier :     Final-HC05-FSR03.ino
 * Description : Ce fichier va recolter des informations sur des capteurs afin des les envoyer par Bluetooth.
 *               Les capteurs utilisés sont HC-SR04 (capteur ultrason) et FSR03 (capteur pression)
 *               Le module bluetooth utilisé est le HC-05
 */

#include <SoftwareSerial.h> // Permet l'utilisation du bluetooth
#include <LiquidCrystal.h> // Permet d'ecrire sur un écran lcd

//
// Déclaration
//

SoftwareSerial      bluetooth(8,9);
LiquidCrystal       lcd(12,11,7,6,5,4);

const byte          TRIGGER_PIN_HC    = 2;
const byte          ECHO_PIN_HC       = 3;
const byte          BUTTON_PIN_MOVE   = 13;
const byte          BUTTON_PIN_SELECT = 10;
const byte          FRS_PIN           = A0;
const unsigned long MEASURE_TIMEOUT   = 25000UL;
const float         SOUND_SPEED       = 340.0 / 1000; // Vitesse du son en mm/s
const String        ARDUINO_NAME      = "Alex"; // nom unique entre tous les arduinos et application Unity

unsigned int        measureHC;
unsigned int        distanceHC;
unsigned int        measureUS;
unsigned int        distanceUS;
unsigned int        distance; // distance moyenne de distance HC et US
unsigned int        maxDistance; // Longueur maximale avant le lancement du jeu.
String              nameBlocGame;
unsigned long       timeStartGame;
unsigned long       delayEndConnexion;

boolean             isDown = true;
int                 previousDistance  = 0;
byte                codeSuccess       = 0;
byte                lastCodeSuccess   = 0;
byte                timeDisplayScore  = 0;
boolean             isConnect         = false;
boolean             isGaming          = false;
boolean             isStartProtocole  = false;

String              firstLine         = "Salut, je suis " + ARDUINO_NAME;
String              secondLine        = "";
String              thirdLine         = "";
String              fourthLineBegin   = "";
String              fourthLineEnd     = "";
boolean             isChangeDisplay   = true;

int fsrReading;
int pressure;
int basicPressure;

void updateLCD(){
if(isChangeDisplay){
    isChangeDisplay = false;
    lcd.clear();
    lcd.setCursor(0,0);
    lcd.print(firstLine);
    lcd.setCursor(0,1);
    lcd.print(secondLine);
    lcd.setCursor(0,2);
    lcd.print(thirdLine);
    lcd.setCursor(0,3);
    lcd.print(fourthLineBegin + fourthLineEnd);
  }
}

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
  if(!secondLine.equals("Recherche en cours")){
    secondLine = "Recherche en cours";
    thirdLine = "";
    fourthLineBegin = "";
    fourthLineEnd = "";
    isChangeDisplay = true;
  }
  if(!isConnect && bluetooth.available() > 0){
    isStartProtocole = true;
    String value = bluetooth.readString();
    bluetooth.flush();
    if(value.equals("Hello, I search BLS device")){
      lcd.setCursor(0,1);
      lcd.print("Connexion en cours");
      bluetooth.println("I Am BLS Device. My Name Is " + ARDUINO_NAME + " Terminate.");
      while(isStartProtocole){
        if(bluetooth.available() > 0){
          value = bluetooth.readString();
          bluetooth.flush();
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
}

void checkCorrect(){
  int idealDistance = 55;
  byte perfect = 3;
  byte excellent = perfect + 3;
  byte good = excellent + 3;
  byte correct = good + 3;

  if(isDown){
    int diffPressure = pressure - basicPressure;
    /*firstLine = pressure;
    firstLine += " - ";
    firstLine += basicPressure;
    firstLine += " = ";
    firstLine += diffPressure;
    isChangeDisplay = true;*/
    int diffDistance = maxDistance - distance;
    
    if(previousDistance * 0.95 > diffDistance){
      if(diffDistance >= idealDistance - perfect && diffDistance <= idealDistance + perfect){
        codeSuccess = 1;
        isDown = false;
      }
      else if(diffDistance >= idealDistance - excellent && diffDistance <= idealDistance + excellent){
        codeSuccess = 2;
        isDown = false;
      }
      else if(diffDistance >= idealDistance - good && diffDistance <= idealDistance + good){
        codeSuccess = 3;
        isDown = false;
      }
      else if(diffDistance >= idealDistance - correct && diffDistance <= idealDistance + correct){
        codeSuccess = 4;
        isDown = false;
      }
    }
    else {
      previousDistance = distance;
    }
  }
  else {
    codeSuccess = 0;
    if(distance > maxDistance * 0.9){
      isDown = true;
      previousDistance = 0;
    }
  }
}

void collectData(){
  
  digitalWrite(TRIGGER_PIN_HC, HIGH);
  delayMicroseconds(50);
  digitalWrite(TRIGGER_PIN_HC, LOW);
  measureHC = pulseIn(ECHO_PIN_HC, HIGH, MEASURE_TIMEOUT);
  distance = measureHC / 2.0 * SOUND_SPEED;

  fsrReading = analogRead(FRS_PIN);
  pressure = map(fsrReading, 0, 1023, 0, 5000);
}

void sendData(){
  collectData(); 
  checkCorrect();

  firstLine = maxDistance;
  firstLine += " ";
  firstLine += distance;
  isChangeDisplay = true;

  if(!thirdLine.equals("Partie en cours")){
    thirdLine = "Partie en cours";
    fourthLineBegin = "";
    fourthLineEnd = "";
    isChangeDisplay = true;
  }

  if(isDown){
    if(!fourthLineBegin.startsWith("Descends")){
      fourthLineBegin = "Descends";
      isChangeDisplay = true;
    }
  } else {
    if(!fourthLineBegin.startsWith("Monte   ")){
      fourthLineBegin = "Monte   "; // les espaces prends la place d'un caractère par rapport au mot descends.
      isChangeDisplay = true;
    }
  }  
  bluetooth.println("V" + String(isDown) + String(codeSuccess) + String(millis() - timeStartGame));
}

void setup() {
  bluetooth.begin(9600);
  lcd.begin(20,4);

  pinMode(TRIGGER_PIN_HC, OUTPUT);
  digitalWrite(TRIGGER_PIN_HC, LOW);
  pinMode(ECHO_PIN_HC, INPUT);

  pinMode(BUTTON_PIN_MOVE, INPUT);
  pinMode(BUTTON_PIN_SELECT, INPUT);
}

boolean unknownCommand = true;

void loop() {
  updateLCD();
  
  if(isConnect){
    if(bluetooth.available() > 0){
      String value = bluetooth.readString();
      bluetooth.flush();
      if(value.equals("End Connection")){
        isConnect = false;
        isGaming = false;
        unknownCommand = false;
      }
      if(isGaming && value.equals("End Game")){
        delayEndConnexion = millis();
        isGaming = false;
        unknownCommand = false;
      }
      if(!isGaming && value.equals("Start Game")){
        timeStartGame = millis();
        delayEndConnexion = millis();
        isGaming = true;
        unknownCommand = false;
      }
      if(unknownCommand){
        bluetooth.println("Unknown Command");
      } else {
        unknownCommand = true;
      }
    }
    if(isGaming && millis() >= (delayEndConnexion + 500000)) {
      isConnect = false;
      isGaming = false;
    }
    if(!secondLine.equals("Avec " + nameBlocGame)){
      secondLine = "Avec " + nameBlocGame;
      thirdLine = "";
      fourthLineBegin = "";
      fourthLineEnd = "";
      isChangeDisplay = true;
    }

    if(isGaming)
      sendData();
    else{
      if(!thirdLine.equals("Attente Partie")){
        thirdLine = "Attente Partie";
        fourthLineBegin = "";
        fourthLineEnd = "";
        isChangeDisplay = true;
      }
      bluetooth.println("I am connected with " + nameBlocGame);
      collectData();
      maxDistance = (maxDistance + distance) / 2;
      basicPressure = (basicPressure + pressure) / 2;
      int ss1 = digitalRead(BUTTON_PIN_MOVE);
      if(ss1 == HIGH)
        bluetooth.println("MOVE");
      int ss2 = digitalRead(BUTTON_PIN_SELECT);
      if(ss2 == HIGH)
        bluetooth.println("PRESS");
    }
  }
  else {
    initConnection();
    delayEndConnexion = millis();
  }

  if(bluetooth.overflow()){
    firstLine = "overflow";
    isChangeDisplay = true;
  }

  delay(50);
}
