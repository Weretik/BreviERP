# Gmail CRM: отправка без чтения почты

## Текущий режим

Отправитель — **pp.brevi@gmail.com**, согласованный контрольный получатель — **wet13prp@gmail.com**, домен — **https://office.brevi.com.ua/**. Рабочее Google-приложение использует In production. Настройка Google и публикация страниц сами по себе не включают отправку ERP.

P6 запрашивает только `https://www.googleapis.com/auth/gmail.send`, `openid` и `email`. Google может вернуть email permission как `https://www.googleapis.com/auth/userinfo.email`; оба обозначения распознаются. Перед использованием токена runtime проверяет `sub`, `email_verified=true` и точное совпадение адреса через `https://openidconnect.googleapis.com/v1/userinfo`. Запросов Gmail profile, поиска, чтения сообщений и автоматической сверки папки Sent нет. [Google OpenID reference](https://developers.google.com/identity/openid-connect/reference), [разрешения отправки](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/send).

При timeout, потере ответа или истечении lease результат остаётся `unknown`. ERP не объявляет такое письмо отправленным и не повторяет отправку. Оператор вручную проверяет папку «Надіслані» отправителя. Новая отправка тому же адресату для той же версии заблокирована, пока прежний исход неопределён. UI/API для регистрации подтверждённого оператором исхода ещё не реализован; текущий код не подменяет его автоматическим успехом. Запланированный unknown прежней версии при обработке один раз переводится в ручную проверку, без IO провайдера.

## Подготовка и публикация Google

Публичные страницы: [описание BreviERP](https://office.brevi.com.ua/brevi-erp/index.html), [конфиденциальность](https://office.brevi.com.ua/brevi-erp/privacy.html), [условия](https://office.brevi.com.ua/brevi-erp/terms.html). Владелец сообщил о завершении внедрения и проверки внешней настройки; это не является evidence реальной отправки из ERP. Сведения об операторе и обработке данных должны соответствовать реальной работе сервиса. Контроль домена, OAuth Branding, режим production и применимые требования Google проверяются владельцем проекта. Клиенты калькулятора и их токены автоматически не изменяются.

Если ранее был выдан токен с чтением Gmail, новый код не использует это разрешение, но не может автоматически уменьшить уже выданный Google grant. Для удаления лишнего доступа нужно обновить согласие на минимальные scopes в согласованном клиенте. Не удаляйте подключение калькулятора, использующее другой клиент.

## Получение и хранение credentials

Runtime принимает ClientId, ClientSecret и RefreshToken из защищённой конфигурации Host.Api:

- `Crm:Delivery:Gmail:OAuth:{ClientId,ClientSecret,RefreshToken}`;
- `Crm:Delivery:{ProviderKey,SenderAddress,TestRecipientAddress,Enabled,SandboxOnly}`.

Если подходящий refresh token уже получен для согласованного клиента и минимального grant, повторный запуск локального helper не требуется. Значения не передавайте в Git, логи или чат. Production использует защищённый secret store окружения. Изменение credentials требует перезапуска host для сброса singleton-кэша.

### Необязательный локальный helper

Существующий `tools/gmail-oauth-setup.py` поддерживает только OAuth-клиент **Desktop app**, временный loopback callback и PKCE. JSON Web application ему не подходит; это ограничение локального helper, а не требование заменить уже настроенный production Web-клиент.

Запуск пользователем из корня backend, если выбран именно Desktop-путь:

```powershell
python tools/gmail-oauth-setup.py --client-json "C:\private\google-desktop-client.json" --sender "pp.brevi@gmail.com" --test-recipient "wet13prp@gmail.com"
```

Helper явно открывает Google consent, запрашивает send/identity scopes, проверяет подтверждённый адрес через UserInfo и передаёт настройки через stdin в `dotnet user-secrets set`. Секреты и callback-коды не печатает. Доставка остаётся `Enabled=false`, `SandboxOnly=true`; helper не отправляет письмо. Development user-secrets не шифрует локальный JSON, поэтому для production используется secret store хостинга. [Microsoft user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

## Runtime и контрольный сценарий

OAuth refresh и UserInfo используют фиксированные HTTPS endpoints, ограниченный ответ/timeout, без redirects и retries. Ошибки grant, несовпадение/неподтверждённость аккаунта или недоступность Google блокируют токен. Кэш сериализует обновление и заканчивается до истечения access token.

После настройки выполнить отдельно согласованный контрольный сценарий только на wet13prp@gmail.com: закреплённая ready-версия, успешный PDF, реальный шаблон письма, явное подтверждение отправки. `succeeded` означает принятие Gmail API, а не доставку/прочтение адресатом. При `unknown` оператор проверяет Gmail вручную и не создаёт дубль. Автоматические тесты не обращаются к Google. Эта реализация не включает production dispatch и не является evidence живого OAuth/send.

Проверки без Google:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~GmailOAuthAccessTokenSourceTests|FullyQualifiedName~GmailQuoteDeliveryProviderTests|FullyQualifiedName~QuoteDeliveryCompositionTests"
python -B tools/tests/test_gmail_oauth_setup.py
```

[Send-only design и границы](../specs/crm/001-clients-quotes-custom-sewing/design/delivery-outbox-p6.md), [проверки реализации](../specs/crm/001-clients-quotes-custom-sewing/evidence/p6-send-only-2026-11-01.md).
