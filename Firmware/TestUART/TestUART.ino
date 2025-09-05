#include <stdint.h>
#include "tm4c1294ncpdt.h"

// ====================== Funções UART ======================

void setup() {
  // put your setup code here, to run once:
// 1. Habilita clocks para UART0 e GPIOA
    SYSCTL_RCGCUART_R |= (1 << 0);   // UART0
    SYSCTL_RCGCGPIO_R |= (1 << 0);   // GPIOA

    // Garante tempo de estabilização
    volatile int delay = SYSCTL_RCGCGPIO_R;

    // 2. Configura PA0 (RX) e PA1 (TX) para função alternativa
    GPIO_PORTA_AHB_AFSEL_R |= (1 << 0) | (1 << 1); // Função alternativa
    GPIO_PORTA_AHB_PCTL_R &= ~0xFF;                // Limpa PCTL PA0/PA1
    GPIO_PORTA_AHB_PCTL_R |= (1 << 0) | (1 << 4);  // PA0=U0RX (0x1), PA1=U0TX (0x1)
    GPIO_PORTA_AHB_DEN_R  |= (1 << 0) | (1 << 1);  // Digital enable

    // 3. Desabilita UART0 antes de configurar
    UART0_CTL_R &= ~UART_CTL_UARTEN;

    // 4. Configura baudrate (115200 bps @ 16 MHz)
    // Fórmula: BRD = SysClk / (16 * BaudRate)
    // BRD = 16e6 / (16 * 115200) = 8.6805
    // IBRD = 8, FBRD = 44
    UART0_IBRD_R = 8;
    UART0_FBRD_R = 44;

    // 5. Formato: 8 bits, FIFO habilitado, 1 stop, sem paridade
    UART0_LCRH_R = (0x3 << 5) | (1 << 4);

    // 6. Clock source = system clock
    UART0_CC_R = 0x0;

    // 7. Habilita UART, TX e RX
    UART0_CTL_R |= (UART_CTL_UARTEN | UART_CTL_TXE | UART_CTL_RXE);
}

void UART0_WriteChar(char c)
{
    while (UART0_FR_R & UART_FR_TXFF);  // Espera FIFO TX não cheio
    UART0_DR_R = c;
}

char UART0_ReadChar(void)
{
    while (UART0_FR_R & UART_FR_RXFE);  // Espera FIFO RX não vazio
    return (char)(UART0_DR_R & 0xFF);
}

void UART0_WriteString(const char *str)
{
    while (*str)
    {
        UART0_WriteChar(*str++);
    }
}

// ====================== Programa principal ======================


void loop() {
  // put your main code here, to run repeatedly: 
    UART0_WriteString("UART0 Init OK\r\n");

    while (1)
    {
        char c = UART0_ReadChar();  // Bloqueia até receber algo
        UART0_WriteChar(c);         // Devolve o mesmo char
    }
}
