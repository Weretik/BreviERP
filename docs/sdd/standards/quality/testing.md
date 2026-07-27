# Тестування

Обирайте тести за ризиком:

- domain/application rules: unit tests;
- persistence, Result mapping і HTTP contracts: integration tests;
- layer/dependency changes: architecture tests.

Запускайте, де застосовно:

```powershell
dotnet restore BreviERP.sln
dotnet build BreviERP.sln --no-restore
dotnet test BreviERP.sln --no-build
```

Якщо перевірка заблокована або не пройшла, повідомляйте точну команду, точку збою та чи є він передіснуючим або спричиненим зміною. Не приховуйте не пов’язані failures.
