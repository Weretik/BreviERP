# Локальное подключение Gmail OAuth для CRM

## Решение владельца от 2026-10-30 — обязательный рабочий план

**Отправитель: pp.brevi@gmail.com. Домен: https://office.brevi.com.ua/.** Режим Google — In production для постоянной работы; Testing с семидневным refresh token не является рабочим вариантом. Владелец выбрал только отправку и ручную проверку неопределённого результата, без автоматического чтения Gmail и повторной отправки.

**Инструкция ниже описывает прежний код и пока не должна использоваться для подключения.** Текущие helper/runtime ещё требуют gmail.readonly и проверку Gmail profile. Перед подключением их необходимо изменить на gmail.send + openid/email, проверять личность через подходящий identity endpoint и исключить автоматическую сверку почты. Эта страница не является доказательством выполненной доработки.

Обязательные release gates:

1. Подготовить и опубликовать без авторизации три страницы: https://office.brevi.com.ua/brevi-erp/index.html, https://office.brevi.com.ua/brevi-erp/privacy.html и https://office.brevi.com.ua/brevi-erp/terms.html. Исходники находятся в frontend apps/admin-react/public/brevi-erp; FE-EN-003 ведёт локальную проверку и публикацию.
2. Подтвердить юридическое наименование оператора, контакт, практики хранения/удаления и соответствие опубликованных утверждений реальному окружению. Полное наименование пока не предоставлено; не выдумывать его.
3. Завершить send-only revision helper/runtime и ручной обработки unknown. До согласования кода с политикой почту не включать.
4. Подтвердить контроль домена по требованиям Google; вписать точные homepage/privacy/terms URLs в OAuth Branding отдельного проекта BreviERP, не меняя проект калькулятора. Настроить In production и выполнить применимые требования Google по проверке приложения. Страницы не гарантируют одобрение.
5. Получить согласие владельца pp.brevi@gmail.com после обновления scope, сохранить credentials в защищённом production окружении, проверить согласованную отправку на wet13prp@gmail.com; записать фактическое принятие провайдером и ручную проверку. Только затем включать рабочую отправку.

Публикация не выполнена этой правкой. Доступность HTTPS без входа и Google approval проверяются отдельно после размещения. Тексты должны соответствовать фактической обработке данных до начала использования интеграции.

Источники: [OAuth policies](https://developers.google.com/identity/protocols/oauth2/policies), [verification requirements](https://support.google.com/cloud/answer/13464321), [Google User Data Policy](https://developers.google.com/terms/api-services-user-data-policy).

## Историческая инструкция для прежнего кода — не запускать до обновления

Сейчас реальная отправка выключена. Для подключения нужен собственный Google OAuth client: пароля Gmail недостаточно. Helper ниже получает разрешение в системном браузере и сохраняет refresh token вне репозитория. Он **не отправляет письмо** и оставляет `Crm:Delivery:Enabled=false`, `SandboxOnly=true`.

## 1. Создать приложение Google

1. Откройте [Google Cloud Console](https://console.cloud.google.com/), выберите свой проект или создайте отдельный проект для Brevi. Убедитесь, что аккаунт и проект принадлежат вам.
2. В **APIs & Services → Library** найдите **Gmail API** и нажмите **Enable**. [Страница Gmail API](https://console.cloud.google.com/apis/library/gmail.googleapis.com).
3. Откройте **Google Auth platform**. В **Branding** заполните название приложения и контактный email. Для личного Gmail в **Audience** выберите **External**; начните с режима **Testing** и добавьте аккаунт отправителя в **Test users**. Если у организации Google Workspace доступен Internal, его применимость определяет администратор организации.
4. В **Data Access** добавьте ровно два разрешения: `https://www.googleapis.com/auth/gmail.send` и `https://www.googleapis.com/auth/gmail.readonly`. Второе необходимо для проверки результата по Message-ID; metadata-only scope не позволяет требуемый поиск. Gmail readonly относится к restricted scopes; требования Google к проверке приложения нужно разрешить перед публичным/производственным использованием. [Разрешения Gmail](https://developers.google.com/workspace/gmail/api/auth/scopes).
5. В **Clients → Create client** выберите тип **Desktop app**, задайте понятное имя и скачайте JSON клиента. Этот локальный helper использует Desktop client с loopback callback, поэтому Web application client ему не подходит. Сохраните скачанный файл в личной папке вне Git; не прикладывайте его к чату и не коммитьте.

В режиме External/Testing refresh token для этих Gmail scopes обычно действует **7 дней**. Это ограничение Google, а не ошибка кэша Brevi; после истечения потребуется повторное согласие или настройка подходящего статуса приложения. [Политика refresh tokens](https://developers.google.com/identity/protocols/oauth2#expiration).

## 2. Запустить локальное согласие

В терминале из корня backend (`BreviERP`) запустите, указав только путь к скачанному JSON:

```powershell
python tools/gmail-oauth-setup.py --client-json "C:\path\to\downloaded-desktop-client.json"
```

Если в ignored `.local/p6/gmail-settings.local.json` уже сохранены адреса, helper возьмёт их оттуда. Иначе он попросит адрес отправителя и разрешённого тестового получателя. Эти адреса можно передать через `--sender` и `--test-recipient`; секреты через аргументы не передаются.

Откроется системный браузер Google. Войдите именно в аккаунт отправителя и подтвердите оба запрошенных разрешения. Helper использует PKCE/S256, случайный state и временный порт только на `127.0.0.1`. Callback должен завершиться за пять минут. При отказе в доступе помощник завершается сразу; callback с неверным state не может прервать правильное согласие. Не копируйте URL обратного перехода в чат: он содержит одноразовый код. Loopback разрешён для Desktop clients; устаревший OOB flow не используется. [Google Desktop OAuth](https://developers.google.com/identity/protocols/oauth2/native-app), [поддержка loopback](https://developers.google.com/identity/protocols/oauth2/resources/loopback-migration).

После согласия helper проверит оба granted scopes и `users/me/profile`: адрес Google-профиля должен точно соответствовать отправителю. Он передаст настройки **через stdin**, а не секретные аргументы процесса, в существующий `dotnet user-secrets set --project ...Host.Api.csproj`. Неизменённые ключи Secret Manager сохраняются. Никакие токены, ответы Google или коды в терминал не выводятся. При ошибке будет только безопасный код остановки; не нужно присылать содержимое JSON.

## 3. Что сохранится и что осталось проверить

Host.Api получает из стандартной development-конфигурации:

- `Crm:Delivery:Gmail:OAuth:{ClientId,ClientSecret,RefreshToken}`;
- выбранные sender/test-recipient, `ProviderKey=gmail`;
- `Enabled=false`, `SandboxOnly=true`.

ASP.NET Secret Manager предназначен для локальной разработки: хранит JSON вне проекта в профиле пользователя и **не шифрует** его. Для production нужен штатный защищённый secret store окружения. Не запускайте `dotnet user-secrets list` для отправки результата в чат: команда раскрывает значения. Стандартное автоматическое чтение user-secrets у Host.Api действует в окружении **Development**. [Документация Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

После успешного выполнения достаточно сообщить «OAuth подключён» без ключей. Останется проверить runtime-аккаунт и выполнить ранее согласованную пробную отправку только на sandbox-адрес. Получение refresh token само по себе не доказывает прохождение отправки/сверки Gmail. EN-004 остаётся частично выполненной до реального контрольного сценария; включение отправки на клиентов не входит в этот локальный helper.

## Runtime и восстановление

`GmailOAuthAccessTokenSource` обновляет access token одним POST на фиксированный Google endpoint, проверяет профиль тем же токеном и только после этого возвращает его адаптеру. Ответы ограничены16KiB, общее время refresh+profile — настроенным timeout. Кэш работает в одном singleton и заканчивается на минуту раньше provider expiry; смена аккаунта/credential options требует пересоздания singleton (перезапуска host). Если scope в refresh response явно возвращён, обязательны оба; если он отсутствует, Google сохраняет исходный grant, поэтому runtime не объявляет grant заново проверенным. Отсутствие credentials, неверный профиль, отзыв согласия, ошибка сети/JSON или timeout возвращают unavailable, без вывода секретов и автоматического повторного запроса.

OAuth HttpClient обязан отключать redirects и автоматические retries: POST содержит refresh token/client secret. При `invalid_grant` повторите согласие через helper; старые пароли/токены Calculator не импортируются. Не запускайте helper из CI или фонового worker: это явное локальное действие пользователя.

Проверки без Google: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~GmailOAuthAccessTokenSourceTests` и `python -B tools/tests/test_gmail_oauth_setup.py`. Реальный OAuth/почта этим тестом не проверяются.
