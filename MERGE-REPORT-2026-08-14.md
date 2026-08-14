# Отчёт о подготовке приватной копии RDP Wrapper

Дата: 2026-08-14  
Исходный репозиторий: https://github.com/stascorp/rdpwrap  
Приватный репозиторий: https://github.com/dipsx/rdpwrap-private

## Что сделано

- Создан приватный репозиторий `dipsx/rdpwrap-private`.
- В приватную копию перенесена история исходного `master`.
- Полностью интегрирован PR #3398 — `Addition of many versions and ver 10.0.22621.4655`.
- Из PR #4062 — `Add comprehensive Windows 11 25H2 and 26H1 support plus latest Windows 10 updates` — после ревью интегрирован полный rewrite с hardening-правками:
  - 30 новых INI-секций для Windows 11 25H2/26H1;
  - конфигурации для Windows 10 `10.0.19041.6456` и `10.0.19045.6466`;
  - сохранён расширенный набор версий, добавленный PR #3398;
  - C#/.NET installer, RDPConf, RDPCheck, WiX MSI и Windows CI/CD;
  - embedded OffsetFinder/Zydis для официальных сборок без запуска бинарников, скачанных во время установки;
  - ARM64 detection и выбор ARM64 DLL;
  - HTTPS-only downloads, лимит размера ответа, временные файлы и проверка exit code процессов;
- Созданы свежие distribution-архивы и опубликованы в GitHub Release вместе с собранными бинарниками.
- Полный rewrite замержен в `master` приватного репозитория через PR #1; итоговый merge-коммит: `d4e31e9`.

## Результаты ревью и исправления

До интеграции были обнаружены и исправлены устаревшие pinned-хэши `sergiye/rdpWrapper`, отсутствие ARM64-ресурса, небезопасный runtime-download OffsetFinder/Zydis и ссылки на публичный fork автора вместо приватного репозитория. Полный Windows build/runtime test перед интеграцией локально невозможен, поэтому ветка проходит отдельные GitHub Actions checks.

## Проверка

- Проверена доступность и видимость приватного репозитория.
- Выполнена проверка whitespace через `git diff --check`.
- Проверены уникальность INI-секций и наличие всех добавленных секций.
- История содержит отдельные merge-коммиты для PR #3398 и полного PR #4062 с hardening-патчами поверх него.
- Сборка Windows-бинарников в текущем macOS-окружении не запускалась: для неё требуются Windows/MSBuild/Delphi-инструменты. Workflow полного rewrite включены для проверки в GitHub Actions.
- Релизные workflow полного rewrite включены. Финальный Windows workflow `31828714509` завершился успешно по всем jobs.

## Свежий артефакт

- Release: https://github.com/dipsx/rdpwrap-private/releases/tag/v2026.08.14-full
- Включены MSI для x86/x64/ARM64, RDPWInst/RDPConf/RDPCheck, DLL для x86/x64/ARM64, `rdpwrap.ini`, OffsetFinder/Zydis и архивы `RDPWrapper.zip` и `RDPWrapper-SelfContained.zip`.
- `RDPWrapper.zip` SHA-256: `734996e91258bf962201a38cbc00788277c49b1920ddf76f5a697286d7dc7611`.
- Артефакт скачан через авторизованный GitHub CLI, ZIP прошёл `unzip -t`, опубликованный `rdpwrap.ini` совпал с рабочим деревом.
- Для unattended deployment приватный Release требует авторизацию; можно использовать embedded/offline пакет или внутреннее зеркало через `RDPWRAP_RELEASE_BASE_URL`.

## Ограничение GitHub

GitHub не позволяет создать приватный fork публичного репозитория в той же fork-сети. Поэтому создана приватная копия с сохранением git-истории; она не является fork-связью GitHub с `stascorp/rdpwrap`.
