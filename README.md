# HamburgerMenu (Holy-Nub)

Open-source desktop utility hub на `Avalonia` для Windows с модульной структурой:

- запуск внешних утилит из папки `Apps`
- просмотр Steam-аккаунтов и базовой информации
- аналитический модуль для сканирования системы
- защищенный режим доставки/запуска через `Loader` + зашифрованный пакет

Проект распространяется по лицензии **MIT**.

## Возможности

- Меню `Apps` для запуска встроенных инструментов:
- `LastActivityView`
- [`Journal Windows`](https://github.com/Solduramigopmo/Journal-Windows)
- `System Informer`
- `RegistryAnalyzer`
- `Everything`
- `USBDeview`

- Модуль `Customer`:
- поиск Steam-аккаунтов в системе
- отображение SteamID2 и SteamID64
- запрос VAC-статуса
- переход в профиль Steam

- Модуль `Analytics`:
- быстрое и полное сканирование
- проверка сигнатур/метаданных/паттернов файлов
- проверка DNS cache
- проверка Steam userdata
- экспорт отчета в текстовый файл

- Модуль `Settings`:
- информация о Windows
- дата установки ОС
- детект VM
- информация о GPU
- быстрые переходы в системные настройки

- Защищенный запуск:
- `HamburgerMenu.Loader` загружает `app.enc` и `version.json`
- расшифровывает пакет во временную папку
- запускает основной exe

## Analytics: как работает сканирование

Ниже описана фактическая логика из кода, которая используется в модуле `Analytics` и объясняет, почему скан обычно быстрый.

Основные методы:

- `FullScanAsync` (`SubmenuAnalytics.axaml.cs`)
- запускает 3 ветки параллельно: `CheckDnsCacheAsync`, `CheckSteamUserdataAsync` и сканирование дисков
- для каждого доступного HDD/USB добавляет отдельную задачу `ScanDriveAsync`

- `QuickScanAsync` (`SubmenuAnalytics.axaml.cs`)
- сканирует только системный диск через `ScanDriveAsync`
- это основной сценарий, который обычно укладывается примерно в `2–5 секунд` на обычной системе

- `ScanDriveAsync` (`SubmenuAnalytics.axaml.cs`)
- для HDD сканирует целевые каталоги, а не весь диск подряд:
- `Downloads`, `Desktop`, `Documents`, `AppData`, `LocalAppData`, `Temp`, `Windows\Prefetch`, `Games`, `Program Files`, `Program Files (x86)` и часть корня диска
- для USB-носителей может сканировать весь том

- `ScanPathAsync` (`SubmenuAnalytics.axaml.cs`)
- делает дедупликацию путей через `_scannedPaths`, чтобы один и тот же путь не обрабатывался повторно
- добавляет результаты без дублей по `FilePath`

- `ScanDirectoryAsync` (`CheatScanner.cs`)
- перечисляет файлы безопасно через `EnumerateFilesSafe`
- запускает проверку файлов в `Parallel.ForEach` с `MaxDegreeOfParallelism = Environment.ProcessorCount`
- в UI попадают только срабатывания с `Confidence >= 70`

- `EnumerateFilesSafe` (`CheatScanner.cs`)
- агрессивно пропускает тяжелые/нерелевантные папки (`SkipFolders`)
- пропускает большой список расширений (`SkipExtensions`) для кода, медиа, документов, архивов и т.д.
- за счет этого объем реально проверяемых файлов сильно уменьшается

- `ScanFile` (`CheatScanner.cs`)
- использует быстрые эвристики:
- проверки по имени, шаблонам, prefetch (`.pf`), AHK (`.ahk`)
- проверки цифровой подписи и PE-метаданных для `.exe/.dll`
- точечные проверки известных размеров/описаний
- для текстовых конфигов чтение ограничено `50 KB` (`TryReadFile`)

- `CheckDnsCacheAsync` (`CheatScanner.cs`)
- запускает `ipconfig /displaydns` и ищет совпадения по списку доменов

- `CheckSteamUserdataAsync` (`CheatScanner.cs`)
- проверяет `Steam\userdata` на подозрительные конфиги/имена

Почему это быстро:

- нет глубокого бинарного анализа или полного чтения всех файлов
- большая фильтрация по папкам/расширениям до начала тяжелых проверок
- параллельная обработка по числу ядер CPU
- ограничение на размер читаемых текстовых файлов
- ранний выход при совпадении высокоприоритетных эвристик

Примечание по времени:

- `Quick Scan`: обычно `~2–5 секунд` на SSD и типовой системе
- `Full Scan`: может быть заметно дольше в зависимости от количества файлов, подключенных дисков и USB-носителей

## Стек

- `.NET 8`
- `Avalonia 11`
- C#
- WMI / Registry API (Windows)

## Структура репозитория

- `HamburgerMenu.Avalonia` — основное UI-приложение
- `HamburgerMenu.Loader` — защищенный загрузчик
- `HamburgerMenu.Encryptor` — упаковка и шифрование сборки (`app.enc`)
- `HamburgerMenu.Shared` — общая логика (crypto, сервисы, лицензии)
- `Apps` — внешние утилиты, запускаемые из интерфейса
- `build-and-encrypt.bat` — сценарий сборки + упаковки для доставки
- `LICENSES.md` — внутренняя документация по лицензиям/кодам доступа

## Системные требования

- Windows 10/11 x64
- .NET SDK 8.0 (для сборки из исходников)

## Быстрый старт (для разработчика)

```powershell
cd D:\humburger-main\humburger-main
dotnet restore
dotnet build HamburgerMenu.Avalonia.sln -c Debug
dotnet run --project .\HamburgerMenu.Avalonia\HamburgerMenu.Avalonia.csproj
```

## Сборка Release

```powershell
dotnet build HamburgerMenu.Avalonia.sln -c Release
```

Выходные файлы:

- `HamburgerMenu.Avalonia\bin\Release\net8.0\`
- `HamburgerMenu.Loader\bin\Release\net8.0\`
- `HamburgerMenu.Encryptor\bin\Release\net8.0\`

## Защищенный режим поставки

Цепочка:

1. Собирается основной проект.
2. Содержимое пакуется в zip.
3. Zip шифруется (AES/PBKDF2) в `app.enc`.
4. Генерируется `version.json` с метаданными.
5. `Loader` скачивает и запускает пакет.

Запуск батника упаковки:

```powershell
.\build-and-encrypt.bat
```

Результат:

- `Encrypted\app.enc`
- `Encrypted\version.json`

## Управление кодами доступа

Проверка кодов доступа реализована в:

- `HamburgerMenu.Shared\Services\LicenseManager.cs`

Формат записи лицензии:

```text
email@example.com-YYYYMMDD-XXXXXXXX
```

- `email@example.com` — идентификатор пользователя
- `YYYYMMDD` — дата окончания
- `XXXXXXXX` — код доступа из 8 символов

## Известные особенности

- Проект ориентирован на Windows (часть API платформозависима).
- В `Release` включена проверка корректности запуска через loader.
- `generate-license.ps1` в текущем состоянии может требовать доработки под ваш процесс генерации ключей.

## Безопасность и дисклеймер

- Проект предоставляется как есть, без гарантий.
- Используйте встроенные утилиты только в рамках закона и правил вашей организации.
- Перед использованием в production рекомендуется провести внутренний аудит кода и зависимостей.

## Contribution

Pull Request'ы и Issue приветствуются.

Рекомендуемый процесс:

1. Создать feature-branch.
2. Внести изменения и проверить сборку `Debug/Release`.
3. Добавить описание изменений в PR.
