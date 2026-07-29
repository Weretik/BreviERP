# Testing rules

Обирайте тести за ризиком:

- domain/application rules — unit tests;
- persistence, Result mapping і HTTP contracts — integration tests;
- зміни шарів або залежностей — architecture tests.

Перед завершенням feature виконуйте, де застосовно:

```powershell
dotnet restore BreviERP.sln
dotnet build BreviERP.sln --no-restore
dotnet test BreviERP.sln --no-build
```

Якщо перевірка заблокована або не пройшла, вкажіть точну команду, точку збою та чи є failure передіснуючим або спричиненим зміною. Не приховуйте не пов’язані failures.
