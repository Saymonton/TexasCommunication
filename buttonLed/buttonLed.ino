#include "tm4c1294ncpdt.h"

bool IsButtonUp(void);
void SetLedHIGH(void);
void SetLedLOW(void);
bool IsLedHIGH(void);

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

  // Definir o led como saída:
  GPIO_PORTF_AHB_DIR_R |= (1 << 0); // PF0 é o LED

  // Definir o botão como entrada:
  GPIO_PORTJ_AHB_DIR_R &= ~(1 << 0); // PJ0 é o Botão

  // Definir os dois como digital:
  GPIO_PORTF_AHB_DEN_R |= (1 << 0);
  GPIO_PORTJ_AHB_DEN_R |= (1 << 0);

  // Definir o botão como pull-up
  GPIO_PORTJ_AHB_PUR_R |= (1 << 0);
}

bool stillPressed = false;
void loop() {
  // put your main code here, to run repeatedly:
  if(!IsButtonUp() && !stillPressed)
  {
    stillPressed = true;
    
    if(IsLedHIGH())    
      SetLedLOW();    
    else    
      SetLedHIGH();
      
    // Debounce simples
    for (volatile int i = 0; i < 100000; i++);
  }
  else if(IsButtonUp() && stillPressed)
  {
    stillPressed = false;
  }
}

bool IsLedHIGH(void)
{
  return GPIO_PORTF_AHB_DATA_R & (1 << 0);
}
bool IsButtonUp(void)
{
  return GPIO_PORTJ_AHB_DATA_R & (1 << 0);
}

void SetLedHIGH(void)
{
  GPIO_PORTF_AHB_DATA_R |= (1 << 0);
}

void SetLedLOW(void)
{
  GPIO_PORTF_AHB_DATA_R &= ~(1 << 0);
}
