
# CustodialWallet (09.08.2025)
**
## Инструкция для запуска проекта
**
**Копируем репозиторий** 

    git clone https://github.com/OneMiay/CustodialWallet.git

**Переходим в папку с проектом**

    cd CustodialWallet

**Собираем образ для докера**

    docker build -t custodialwallet .

**Запускаем контейнер**

    docker run -d -p 8080:8080 --name custodial-wallet custodialwallet

**Открываем проект в браузере**

    http://localhost:8080/

> ЗЫ: Task level - "Первый класс вторая четверть" :)


# UPD CustodialWallet SCUF :) (10.08.2025)
**
## Инструкция для запуска проекта (PostgreSQL+Dapper)
**
**Копируем репозиторий (*если он уже скачен пропускаем*)**

    git clone https://github.com/OneMiay/CustodialWallet.git

**Переходим в папку с проектом (*если он уже скачен пропускаем*)**

    cd CustodialWallet

**Переходим в ветку 4fun**

    git switch 4fun


**Собираем проект в докере (*PostgreSQL+Dapper*)**

    docker-compose up --build -d

**Открываем проект в браузере**

    http://localhost:8888

> ЗЫ: Код для меня как музыка — только без нот, но со стеком вызовов.
> 
> Хотите я напишу приложение (Windows/macOS/Android/iOS/Tizen) для работы с этим WEB API?
> 

Email: i.milashin@gmail.com

Telegram: https://t.me/OMiay
