/* Пример мигания светодиодом - три цвета
   Этот пример кода находится в общественном достоянии (или лицензирован CC0, на ваше усмотрение).

   Если не требуется иное, предусмотренное законом, это
   программное обеспечение распространяется "КАК ЕСТЬ", БЕЗ КАКИХ-ЛИБО ГАРАНТИЙ ИЛИ УСЛОВИЙ,
   явных или подразумеваемых.
*/

#include <stdio.h>             // Стандартная библиотека ввода/вывода
#include "freertos/FreeRTOS.h" // Ядро FreeRTOS для управления задачами
#include "freertos/task.h"     // Функции для работы с задачами (задержки)
#include "driver/gpio.h"       // Драйвер для работы с GPIO-пинами
#include "esp_log.h"           // Система логирования ESP-IDF
#include "led_strip.h"         // Библиотека для управления адресуемыми светодиодами (WS2812)
#include "sdkconfig.h"         // Файл конфигурации проекта (создается из menuconfig)

// Тег для логирования - будет отображаться в каждом сообщении
static const char *TAG = "example";

/* Используйте меню конфигурации проекта (idf.py menuconfig) для выбора GPIO,
   или вы можете отредактировать следующую строку и установить номер здесь.
*/
#define BLINK_GPIO CONFIG_BLINK_GPIO // Номер пина берется из конфигурации

// Состояние светодиода: 0 - красный, 1 - зеленый, 2 - синий, 3 - выключен
static uint8_t s_led_state = 0;

// ============================================================================
// БЛОК ДЛЯ АДРЕСУЕМОГО СВЕТОДИОДА (LED STRIP)
// ============================================================================
#ifdef CONFIG_BLINK_LED_STRIP

// Дескриптор для управления LED-лентой
static led_strip_handle_t led_strip;

/**
 * @brief Управляет состоянием светодиода (включение/выключение с цветом)
 *
 * В зависимости от значения s_led_state:
 * - 0: зажечь красным
 * - 1: зажечь зеленым
 * - 2: зажечь синим
 * - 3: выключить
 */
static void blink_led(void)
{
    esp_err_t err;

    /* Если состояние 0, 1 или 2 - светодиод горит */
    if (s_led_state < 3)
    {
        // Выбираем цвет в зависимости от состояния
        switch (s_led_state)
        {
        case 0: // Красный цвет
            err = led_strip_set_pixel(led_strip, 0, 255, 0, 0);
            break;
        case 1: // Зеленый цвет
            err = led_strip_set_pixel(led_strip, 0, 0, 255, 0);
            break;
        default: // Синий цвет
            err = led_strip_set_pixel(led_strip, 0, 0, 0, 255);
            break;
        }

        if (err != ESP_OK)
        {
            ESP_LOGE(TAG, "led_strip_set_pixel: %s", esp_err_to_name(err));
            return;
        }

        /* Отправляем данные на светодиод - он загорается */
        err = led_strip_refresh(led_strip);
        if (err != ESP_OK)
        {
            ESP_LOGE(TAG, "led_strip_refresh: %s", esp_err_to_name(err));
        }
    }
    else
    {
        /* Выключаем светодиод - очищаем все пиксели */
        err = led_strip_clear(led_strip);
        if (err != ESP_OK)
        {
            ESP_LOGE(TAG, "led_strip_clear: %s", esp_err_to_name(err));
        }
    }
}

/**
 * @brief Настраивает адресуемый светодиод
 *
 * Инициализирует LED-ленту с указанными параметрами:
 * - Номер пина (берется из CONFIG_BLINK_GPIO)
 * - Количество светодиодов (1 штука)
 * - Выбирает бэкенд: RMT (рекомендуется) или SPI
 */
static void configure_led(void)
{
    ESP_LOGI(TAG, "Пример настроен на мигание адресуемым светодиодом!");

    /* Структура конфигурации LED-ленты */
    led_strip_config_t strip_config = {
        .strip_gpio_num = BLINK_GPIO, // Номер пина для управления
        .max_leds = 1,                // Минимум один светодиод на плате
    };

#if CONFIG_BLINK_LED_STRIP_BACKEND_RMT
    /* Настройка RMT-драйвера (рекомендуемый способ) */
    led_strip_rmt_config_t rmt_config = {
        .resolution_hz = 10 * 1000 * 1000, // Частота 10 МГц для точной синхронизации
        .flags.with_dma = false,           // DMA не используем
    };
    // Создаем RMT-устройство для управления лентой
    ESP_ERROR_CHECK(led_strip_new_rmt_device(&strip_config, &rmt_config, &led_strip));

#elif CONFIG_BLINK_LED_STRIP_BACKEND_SPI
    /* Настройка SPI-драйвера (альтернативный способ) */
    led_strip_spi_config_t spi_config = {
        .spi_bus = SPI2_HOST,   // Используем второй SPI-интерфейс
        .flags.with_dma = true, // Включаем DMA для быстрой передачи
    };
    // Создаем SPI-устройство для управления лентой
    ESP_ERROR_CHECK(led_strip_new_spi_device(&strip_config, &spi_config, &led_strip));

#else
#error "неподдерживаемый бэкенд для LED-ленты"
#endif

    /* Изначально выключаем все светодиоды */
    ESP_ERROR_CHECK(led_strip_clear(led_strip));
}

// ============================================================================
// БЛОК ДЛЯ ОБЫЧНОГО GPIO-СВЕТОДИОДА
// ============================================================================
#elif CONFIG_BLINK_LED_GPIO

/**
 * @brief Управляет обычным светодиодом (вкл/выкл)
 *
 * Просто устанавливает высокий или низкий уровень на пине
 */
static void blink_led(void)
{
    /* Устанавливаем уровень на пине в зависимости от состояния */
    esp_err_t err = gpio_set_level(BLINK_GPIO, s_led_state);
    if (err != ESP_OK)
    {
        ESP_LOGE(TAG, "gpio_set_level: %s", esp_err_to_name(err));
    }
}

/**
 * @brief Настраивает обычный GPIO-светодиод
 *
 * Сбрасывает пин и настраивает его как выход
 */
static void configure_led(void)
{
    ESP_LOGI(TAG, "Пример настроен на мигание GPIO-светодиодом!");
    gpio_reset_pin(BLINK_GPIO);                                       // Сбрасываем пин в состояние по умолчанию
    ESP_ERROR_CHECK(gpio_set_direction(BLINK_GPIO, GPIO_MODE_OUTPUT)); // Настраиваем как выход
}

// ============================================================================
// БЛОК ОБРАБОТКИ ОШИБКИ (если тип светодиода не выбран)
// ============================================================================
#else
#error "неподдерживаемый тип светодиода"
#endif

// ============================================================================
// ГЛАВНАЯ ФУНКЦИЯ ПРОГРАММЫ
// ============================================================================
void app_main(void)
{
    /* Настраиваем светодиод в зависимости от выбранного типа */
    configure_led();

    // Массив названий цветов для красивого вывода в лог
    const char *color_names[] = {"КРАСНЫЙ", "ЗЕЛЕНЫЙ", "СИНИЙ", "ВЫКЛ"};

    // Бесконечный цикл - основная логика работы программы
    while (1)
    {
        // Выводим в лог информацию о текущем состоянии
        ESP_LOGI(TAG, "Состояние светодиода: %s", color_names[s_led_state]);

        // Включаем светодиод нужным цветом или выключаем
        blink_led();

        // Переключаем состояние по циклу: 0 -> 1 -> 2 -> 3 -> 0 -> ...
        s_led_state = (s_led_state + 1) % 4;

        // Задержка на заданное количество миллисекунд (берется из menuconfig)
        vTaskDelay(CONFIG_BLINK_PERIOD / portTICK_PERIOD_MS);
    }
}