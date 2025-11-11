Основы применения Emmet в CSS
Принципы сокращений

Emmet предоставляет короткие алиасы для часто используемых CSS-свойств:

    Базовый формат: свойство + значение
    Не требует указания полного синтаксиса CSS
    Автоматически добавляет необходимые единицы измерения

Основные сокращения для размеров

Размеры и отступы:

    w200 → width: 200px;
    h100 → height: 100px;
    m10 → margin: 10px;
    p15 → padding: 15px;

Сокращения для позиционирования

Позиционирование и отображение:

    df → display: flex;
    dib → display: inline-block;
    pr → position: relative;
    pa → position: absolute;

Цвета и фоны

Работа с цветами:

    bgr → background: red;
    bgc#fff → background-color: #fff;
    c#000 → color: #000;

Флексбокс и выравнивание

Флексбокс-свойства:

    jcc → justify-content: center;
    aic → align-items: center;
    fww → flex-wrap: wrap;

Пример полноценного использования


.container {
    w500     /* width: 500px; */
    h300     /* height: 300px; */
    df       /* display: flex; */
    jcsb     /* justify-content: space-between; */
    bgc#f0f0 /* background-color: #f0f0f0; */
    br10     /* border-radius: 10px; */
}
    

Важно: для использования Emmet необходимо установить соответствующее расширение в вашем редакторе кода. (VS Code поддерживает "из коробки")

Emmet превращает процесс написания CSS из утомительной работы в быстрый и приятный опыт написания кода.

Замечание: вернитесь, пожалуйста, к этому тексту после того как начнёте изучать 7 модуль