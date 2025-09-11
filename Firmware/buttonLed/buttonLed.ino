#include "tm4c1294ncpdt.h"
#include <stdint.h>
#define GPIO_LOCK_KEY 0x4C4F434B

// Communication
#define HEADER1 0xAA
#define HEADER2 0x55

#define CMD_GET_STATUS 0x01
#define CMD_SET_LED    0x02

// Declaração de funções
bool IsButtonUp(void);
void SetLedHIGH(void);
void SetLedLOW(void);
bool IsLedHIGH(void);
void SetAllLEDsOff(void);
void ReadSerial(void);
bool processCommand(byte cmd, byte *data, byte len);

// Declaração de variáveis
bool stillPressed = false;
bool multiLed = true;
uint8_t currentLEDActive = 0;
uint8_t currentMultiLEDsActive = 0;
unsigned long lastMillis = 0;
const long interval = 200;

/*  Controlando botões e leds simples
   O objetivo é controlar o led da placa a partir do botão da placa e via software.
*/
void setup() {
  // ====================== Iniciar GPIO's ======================
  GPIO_PORTJ_AHB_LOCK_R = GPIO_LOCK_KEY;  // chave mágica
  GPIO_PORTF_AHB_LOCK_R = GPIO_LOCK_KEY;  // chave mágica
  GPIO_PORTJ_AHB_CR_R |= (1 << 0);     // libera controle de PJ0
  GPIO_PORTF_AHB_CR_R |= (1 << 0);         // libera controle de PF0

  // put your setup code here, to run once:
  // Ativando os clocks do botão e do led
  SYSCTL_RCGCGPIO_R |= (1 << 5);
  SYSCTL_RCGCGPIO_R |= (1 << 8);
  SYSCTL_RCGCGPIO_R |= (1 << 12);

  // Definir os leds como saída:
  GPIO_PORTF_AHB_DIR_R |= (1 << 0); // PF0
  GPIO_PORTF_AHB_DIR_R |= (1 << 4); // PF4
  GPIO_PORTN_DIR_R |= (1 << 0); // PN0
  GPIO_PORTN_DIR_R |= (1 << 1); // PN1

  // Definir o botão como entrada:
  GPIO_PORTJ_AHB_DIR_R &= ~(1 << 0); // PJ0 é o Botão

  // Definir os dois como digital:
  GPIO_PORTF_AHB_DEN_R |= (1 << 0) | (1 << 4); // PF0 e PF4
  GPIO_PORTN_DEN_R |= (1 << 0) | (1 << 1); // PN0 e PN1
  GPIO_PORTJ_AHB_DEN_R |= (1 << 0); // Botão

  // Definir o botão como pull-up
  GPIO_PORTJ_AHB_PUR_R |= (1 << 0);


  // ====================== Iniciar serial ======================
  Serial.begin(115200);
  // Mensagem inicial
  Serial.println("UART0 Init OK");
}

void loop() {
  ReadSerial();
  unsigned long millisNow = millis();
  if (multiLed)
  {
    if (millisNow - lastMillis >= interval)
    {
      lastMillis = millisNow;
      // bit0 de currentLEDActive -> bit1 de GPIO_PORTN
      GPIO_PORTN_DATA_R = (GPIO_PORTN_DATA_R & ~(1u << 1)) | (((currentMultiLEDsActive >> 0) & 1u) << 1);

      // bit1 de currentLEDActive -> bit0 de GPIO_PORTN
      GPIO_PORTN_DATA_R = (GPIO_PORTN_DATA_R & ~(1u << 0)) | (((currentMultiLEDsActive >> 1) & 1u) << 0);

      // bit2 de currentLEDActive -> bit4 de GPIO_PORTF_AHB
      GPIO_PORTF_AHB_DATA_R = (GPIO_PORTF_AHB_DATA_R & ~(1u << 4)) | (((currentMultiLEDsActive >> 2) & 1u) << 4);

      // bit3 de currentLEDActive -> bit0 de GPIO_PORTF_AHB
      GPIO_PORTF_AHB_DATA_R = (GPIO_PORTF_AHB_DATA_R & ~(1u << 0)) | (((currentMultiLEDsActive >> 3) & 1u) << 0);
    }

    return;
  }
  // put your main code here, to run repeatedly:

  if (!IsButtonUp() && !stillPressed && (millisNow - lastMillis >= interval))
  {
    lastMillis = millisNow;
    stillPressed = true;

    if (currentLEDActive == 3)
      currentLEDActive = 0;
    else
      currentLEDActive++;
    sendPacket(CMD_GET_STATUS, (byte*)&currentLEDActive, 1);
  }
  else if (IsButtonUp() && stillPressed)
  {
    stillPressed = false;
  }

  SetAllLEDsOff();
  switch (currentLEDActive)
  {
    case 0: GPIO_PORTN_DATA_R |= (1 << 1); break;
    case 1: GPIO_PORTN_DATA_R |= (1 << 0); break;
    case 2: GPIO_PORTF_AHB_DATA_R |= (1 << 4); break;
    case 3: GPIO_PORTF_AHB_DATA_R |= (1 << 0); break;
  }
}
void ReadSerial(void)
{
  if (Serial.available() >= 5) { // pacote mínimo
    if (Serial.peek() == HEADER1) {
      // Lê cabeçalho
      byte header1 = Serial.read();
      byte header2 = Serial.read();
      if (header2 != HEADER2) return;

      byte cmd = Serial.read();
      byte len = Serial.read();

      // Lê dados
      uint8_t data[16]; // limite arbitrário
      for (int i = 0; i < len; i++) {
        while (!Serial.available()); // espera dado
        data[i] = Serial.read();
      }

      // Lê checksum
      while (!Serial.available());
      byte checksum = Serial.read();

      // Calcula checksum esperado
      byte calc = cmd + len;
      for (int i = 0; i < len; i++) calc += data[i];

      if (checksum == calc) {
        bool sucess = processCommand(cmd, data, len);
        if (!sucess) Serial.println("Command error!");
      } else {
        Serial.println("Checksum error!");
      }
    } else {
      Serial.read(); // descarta byte inválido
    }
  }
}
bool IsValidData(uint8_t data)
{
  return data <= 0x0F; // Só aceita 0..15 no caso os 4 bits menos significantes
}
// ================= Processamento =================
bool processCommand(byte cmd, uint8_t *data, byte len) 
{
  if(multiLed)
  {
    if (cmd == CMD_GET_STATUS) {
      sendPacket(CMD_GET_STATUS, &currentMultiLEDsActive, 1);
      return true;
    }
    else if (cmd == CMD_SET_LED && len == 1 && IsValidData(*data)) {
      if(IsValidData(*data))
      {
          currentMultiLEDsActive = *data;
          sendPacket(CMD_SET_LED, &currentMultiLEDsActive, 1);
          return true;
      }
      else
      {
          Serial.println("Package error: Value received is greater than 0x0F!");
          return false;
      }

    }  
  }
  else
  {
    if (cmd == CMD_GET_STATUS) {
      sendPacket(CMD_GET_STATUS, (byte*)&currentLEDActive, 1);
      return true;
    }
    else if (cmd == CMD_SET_LED && len == 1) {
      currentLEDActive = data[0] % 4; // força 0..3
      sendPacket(CMD_SET_LED, (byte*)&currentLEDActive, 1);
      return true;
    }
  }
  return false;
}

// ================= Envio de Pacote =================

void sendPacket(byte cmd, uint8_t *data, byte len) {
  byte checksum = cmd + len;
  for (int i = 0; i < len; i++) checksum += data[i];

  Serial.write(HEADER1);
  Serial.write(HEADER2);
  Serial.write(cmd);
  Serial.write(len);
  for (int i = 0; i < len; i++) Serial.write(data[i]);
  Serial.write(checksum);
}


void SetAllLEDsOff(void)
{
  GPIO_PORTN_DATA_R &= ~((1 << 1) | (1 << 0));
  GPIO_PORTF_AHB_DATA_R &= ~((1 << 0) | (1 << 4));
}
bool IsButtonUp(void)
{
  return GPIO_PORTJ_AHB_DATA_R & (1 << 0);
}
