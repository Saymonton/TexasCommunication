#include "tm4c1294ncpdt.h"

bool IsButtonUp(void);
void SetLedHIGH(void);
void SetLedLOW(void);
bool IsLedHIGH(void);

bool stillPressed = false;
int currentLEDActive = 0;
/*  Controlando botões e leds simples
 * O objetivo é controlar o led da placa a partir do botão da placa
 */
void setup() {
//  GPIO_PORTJ_AHB_LOCK_R = 0x4C4F434B;  // chave mágica
//  GPIO_PORTJ_AHB_CR_R |= (1 << 0);     // libera controle de PJ0
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
}

void loop() {
  // put your main code here, to run repeatedly:
  if(!IsButtonUp() && !stillPressed)
  {
    stillPressed = true;

    if(currentLEDActive == 3)
      currentLEDActive = 0;
    else
      currentLEDActive++;
      
    // Debounce simples
    for (volatile int i = 0; i < 100000; i++);
  }
  else if(IsButtonUp() && stillPressed)
  {
    stillPressed = false;
  }

  switch(currentLEDActive)
  {
    case 0:
      SetAllLEDsOff();
      GPIO_PORTN_DATA_R |= (1 << 1);
    break;
    case 1:
      SetAllLEDsOff();
      GPIO_PORTN_DATA_R |= (1 << 0);
    break; 
    case 2:
      SetAllLEDsOff();
      GPIO_PORTF_AHB_DATA_R |= (1 << 4);
    break;
    case 3:
      SetAllLEDsOff();
      GPIO_PORTF_AHB_DATA_R |= (1 << 0);
    break;
  }
}

void SetAllLEDsOff(void)
{
    GPIO_PORTN_DATA_R &= ~(1 << 1);
    GPIO_PORTN_DATA_R &= ~(1 << 0);
    GPIO_PORTF_AHB_DATA_R &= ~(1 << 0);
    GPIO_PORTF_AHB_DATA_R &= ~(1 << 4);
}
bool IsButtonUp(void)
{
  return GPIO_PORTJ_AHB_DATA_R & (1 << 0);
}
