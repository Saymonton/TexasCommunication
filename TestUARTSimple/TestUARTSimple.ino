#include "tm4c1294ncpdt.h"

void BlinkLed()
{
  unsigned long millisNow = millis();
  GPIO_PORTF_AHB_DATA_R |= (1 << 0); // Liga o led
  while(millis() - millisNow <= 100);
  //for(volatile int i = 0; i < 100000; i++);
  GPIO_PORTF_AHB_DATA_R &= ~(1 << 0); // Desliga o led  
}
void setup() {
  SYSCTL_RCGCGPIO_R |= (1 << 5);  // Habilita o clock da porta F

  GPIO_PORTF_AHB_DIR_R |= (1 << 0); // Seta a direção para saída

  GPIO_PORTF_AHB_DEN_R |= (1 << 0); // Define o pino como digital

  GPIO_PORTF_AHB_PUR_R |= (1 << 0); // Habilita pull-up (o pino fica em 1 quando não pressionado)
  
  GPIO_PORTF_AHB_DATA_R &= ~(1 << 0); // Desliga o led
  
  // Inicia UART0 (porta USB debug) a 115200 baud
  Serial.begin(115200);

  // Mensagem inicial
  Serial.println("UART0 Init OK");
}

void loop() {
  // Se tiver algum caractere recebido
  if(Serial.available() > 0){
    while (Serial.available() > 0) {
      char c = Serial.read();   // lê 1 byte
      Serial.write(c);          // devolve o mesmo byte (eco)
    }
    BlinkLed();
  }
}
