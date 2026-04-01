# Journal Windows

`Journal Windows` — настольное WinForms-приложение для анализа изменений файловой системы Windows через **NTFS USN Journal** (`$UsnJrnl`).

Программа помогает быстро увидеть события:

- создание/удаление файлов
- переименование
- изменение данных и атрибутов
- изменение структуры каталогов

И подходит для базового форензик-анализа, аудита активности и технической диагностики.

## Что делает программа

- Читает USN Journal выбранного NTFS-тома через Win32 API.
- Преобразует сырые USN-записи в удобные сущности с датой/причиной/путем.
- Показывает результаты в 2 режимах:
- `Data Grid` (таблица, поиск, контекстные действия)
- `Directory Tree` (дерево каталогов + изменения по выбранной папке)
- Открывает подробную карточку конкретной записи.
- Позволяет открыть каталог записи в `Explorer`.
- Поддерживает локализацию интерфейса (`ru`/`en`).

## Как это работает внутри

Основной пайплайн сканирования:

1. Выбор диска (только `NTFS` и `IsReady`).
2. Получение handle тома.
3. Чтение текущего состояния журнала (`FSCTL_QUERY_USN_JOURNAL`).
4. Чтение записей (`FSCTL_READ_USN_JOURNAL`) по маске причин.
5. Разрешение `FileReference/ParentFileReference` в реальные пути.
6. Сбор индексов и построение данных для UI.

Используемые причины USN включают: `FILE_CREATE`, `FILE_DELETE`, `RENAME_OLD_NAME`, `RENAME_NEW_NAME`, `DATA_OVERWRITE`, `SECURITY_CHANGE`, `REPARSE_POINT_CHANGE`, `CLOSE` и другие.

## Интерфейс

Главное меню:

- `Drive -> Select` — выбор NTFS-тома.
- `Drive -> Scan` — запуск чтения журнала.
- `Layout -> Directory Tree / Data Grid` — выбор визуализации данных после скана.
- `Language` — переключение языка интерфейса.
- `Information` — окно с информацией о проекте/авторе.

Контекстное меню записи:

- копировать значение ячейки
- открыть `Entry info`
- открыть директорию записи в Проводнике

## Требования

- Windows (приложение использует Win32 API и NTFS Journal)
- NTFS-том для сканирования
- Права администратора (в `app.manifest` выставлено `requireAdministrator`)
- .NET Framework 4.8 (целевая платформа проекта)

## Сборка и запуск

### Рекомендуемый способ (Visual Studio)

1. Откройте `Journal Windows.sln` в Visual Studio 2022.
2. Убедитесь, что установлен workload **.NET desktop development**.
3. Выберите `Debug` или `Release`.
4. Соберите и запустите проект `Journal Windows`.

### Через командную строку

Проект старого формата (`.csproj` для .NET Framework 4.8).  
Для корректной сборки используйте `MSBuild` из Visual Studio Build Tools.

Пример:

```powershell
msbuild "Journal Windows.sln" /p:Configuration=Release
```

Примечание: обычный `dotnet build` на новых SDK может выдавать ошибки ресурсов (`MSB3822/MSB3823`) для этого типа проекта.

## Структура проекта

- `Journal Windows/Program.cs` — точка входа
- `Journal Windows/View/FormMain.cs` — основная форма и меню
- `Journal Windows/View/FormDrive.cs` — выбор тома
- `Journal Windows/View/FormEntryInfo.cs` — детальная информация по записи
- `Journal Windows/View/Layout/GridLayout.cs` — табличный режим
- `Journal Windows/View/Layout/TreeLayout.cs` — режим дерева директорий
- `Journal Windows/Entry/EntryManager.cs` — оркестратор сканирования и индексов
- `Journal Windows/Entry/USNEntry.cs` — модель записи журнала
- `Journal Windows/Entry/ResolvableIdentifier.cs` — разрешение ID в путь
- `Journal Windows/Native/NtfsUsnJournal.cs` — работа с USN Journal
- `Journal Windows/Native/Win32Api.cs` — P/Invoke/константы/структуры
- `Journal Windows/Native/FileID.cs` — получение пути по File ID
- `Journal Windows/Language/*` — система локализации

## Ограничения

- Работает только с NTFS-дисками.
- История ограничена глубиной USN Journal (старые записи могут быть вытеснены).
- Для удаленных объектов путь может не разрешиться (останется ID).
- Пункт меню `File -> Export` присутствует в UI, но логика экспорта в текущем коде не реализована.

## Типовые сценарии использования

- Проверка, какие файлы создавались/удалялись за период.
- Анализ подозрительной активности в папках пользователя.
- Быстрый просмотр изменений в конкретной директории через `Directory Tree`.
- Ручное расследование события с переходом в каталог и детальной карточкой записи.

## Локализация

Поддерживаются:

- Русский (`ru`)
- Английский (`en`)
