# DownFileBot - бот по загрузке файлов в телеграм по прямым ссылкам.
## Установка и настройка 
Скачайте архив,распакуйте папку внутри архива.
В settings.ini  заполните своими данными 
- YourBotTelegreamToken=ваш токен от телеграм бота
- YourLocalServerTelegram=http://localhost:8082
 
 Запустите бота через screen
 ```sh
screen -S downbot
cd /opt/downBot/./DownFileBot
## Предварительные требования:
- linux 
- dotnet 6
- server telegram(для обхода ограничения до 2гб)
- получение api_id телеграм(для работы с server telegram)
- Screen (для удобсва запуска)

### Установка и использование Screen, .NET 6 и локального сервера Telegram на Ubuntu
- Установка dotnet6
Добавьте Microsoft пакет и ключ:
```sh
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

Обновите пакеты и установите .NET SDK:
```sh
sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0

Проверьте установку:
```sh
dotnet --version

- Установка screen 
```sh
sudo apt-get install screen
- Основные команды Screen
 Запуск окна Screen:
  ```sh
  screen

Нажмите два раза Enter, чтобы открыть окно Screen, в котором можно запускать бота или другой софт.

Свернуть окно и продолжить работу бота:
Ctrl + A + D

Просмотр запущенных окон Screen:
```sh
screen -r

Открытие нужного окна:
```sh
screen -r 1137
Где 1137 - это идентификатор запущенного окна, который можно увидеть в выводе команды screen -r.

- Установка локального сервера Telegram на Ubuntu 20-22
Шаги установки
Обновите пакеты:
```sh
apt-get update
apt-get upgrade

Установите необходимые зависимости:
```sh
apt-get install make git zlib1g-dev libssl-dev gperf cmake g++

Клонируйте репозиторий Telegram Bot API:
```sh
git clone --recursive https://github.com/tdlib/telegram-bot-api.git
cd telegram-bot-api

Создайте и перейдите в директорию сборки:
```sh
rm -rf build
mkdir build
cd build

Скомпилируйте и установите:
```sh
cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_INSTALL_PREFIX:PATH=/usr/local ..
cmake --build . --target install
cd ../..
ls -l /usr/local/bin/telegram-bot-api*

Примечание: Команда cmake --build . --target install может занять несколько часов. Рекомендуется запускать её на ночь.
Регистрация приложения в Telegram
Следуйте инструкции для получения api_id и api_hash, которые понадобятся для запуска сервера.

Проверка установки и запуска сервера
Проверьте, что сервер установлен и запущен:
```sh
ps aux | grep telegram-bot-api

Если вывод содержит строку, подобную этой:
```sh
root 250185 0.0 0.2 4028 2104 pts/5 S+ 15:58 0:00 grep --color=auto telegram-bot-api
значит, сервер установлен и запущен.

Запуск сервера с вашими данными:
```sh
telegram-bot-api --api-id="ваш api id" --api-hash="ваш api hash" --local

Проверьте снова:
```sh
ps aux | grep telegram-bot-api

В выводе должна появиться строка с вашими api_id и api_hash, что указывает на успешный запуск сервера с вашими данными.

- Использование бота:
Просто кидайте ему прямую ссылку для загрузки,он будет уведомлять ход загрузки и после отдаст файл с сервера телеграм.

