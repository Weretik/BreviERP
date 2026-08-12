# Product page — ціноутворення

## Прийняте правило

Ціна `Sewing` обчислюється для кожної пари «товар — вибрана тканина». Тому один товар із кількома тканинами має окремі три ціни для кожної тканини. Ціна тканини береться з поточного `Reference.Fabric`, а ціни фурнітури — з поточних `Reference.GarmentAccessory`; Product не зберігає їх snapshot. Зміна довідникових цін або коефіцієнтів змінює наступний розрахунок.

Усі проміжні та кінцеві значення передаються повними дробовими числовими значеннями без округлення. Усі дільники мають бути більшими за нуль; якщо хоча б один з них нульовий або відсутній, ціна не обчислюється.

## Вхідні дані

| Позначення | Джерело |
| --- | --- |
| `metersPerProduct` | `Product.SewingProductDetails.MetersPerProduct` |
| `fabricPrice` | поточний `Reference.Fabric.Price` вибраної тканини |
| `accessoryQuantity`, `accessoryPrice` | відповідно Product link і поточний `Reference.GarmentAccessory.Price` для кожної фурнітури |
| `totalOperationMinutes` | сума `Min` усіх вибраних `GarmentPartOperation` |
| `srZpShvei` | `AdditionalReference.Key = sr_zp_shvei` |
| `workDay` | `AdditionalReference.Key = work_day` |
| `coefficientSeamstressAward` | `AdditionalReference.Key = coefficient_seamstress_award` |
| `coefficientFactor` | `AdditionalReference.Key = coefficient_factor` |
| `coefficientMaster` | `AdditionalReference.Key = coefficient_master` |
| `coefficientForeman` | `AdditionalReference.Key = coefficient_foreman` |
| `monthlyExpenses` | `AdditionalReference.Key = monthly_expenses` |
| `countShvei` | `AdditionalReference.Key = count_shvei` |

Значення кожного рядка `AdditionalReference` читається за його унікальним `Key`; у розрахунок передається його поточне поле `Value`. Для кожного значення з unit `%` використовується правило `percentRatio = Value / 100`: наприклад, `33%` перетворюється на `0.33`.

## Розрахунок собівартості Sewing

```text
piecesPerShift = 480 / (totalOperationMinutes × 1.25)

seamstressSalary = (srZpShvei / workDay) / piecesPerShift
seamstressAward = seamstressSalary × (coefficientSeamstressAward / 100)
cutterSalary = seamstressSalary / (coefficientFactor / 100)
masterSalary = seamstressSalary / (coefficientMaster / 100)
foremanSalary = seamstressSalary / (coefficientForeman / 100)

workshopSalary = seamstressSalary + cutterSalary + masterSalary + foremanSalary + seamstressAward
overheadExpenses = monthlyExpenses / workDay / countShvei / piecesPerShift

accessoriesCost = Σ(accessoryQuantity × accessoryPrice)
baseCost = (metersPerProduct × fabricPrice) + accessoriesCost + workshopSalary + overheadExpenses
```

`1.25` є фіксованим коефіцієнтом до загальної трудомісткості. Sewing-чернетка без операцій або товар із нульовою сумою `Min` не має `piecesPerShift` і ціни.

`seamstressSalary`, `seamstressAward`, `cutterSalary`, `masterSalary` і `foremanSalary` є витратами на один виріб. Місячна `srZpShvei` використовується лише для початкового розрахунку `seamstressSalary`.

## Три ціни для кожної тканини

Для кожної вибраної тканини формуються такі ціни:

```text
price1To10 = baseCost × (1 + profit10 / 100)
price11To39 = baseCost × (1 + profit10To40 / 100)
price40Plus = baseCost × (1 + profit40 / 100)
```

| Діапазон замовлення | Ключ `AdditionalReference` |
| --- | --- |
| 1–10 шт. | `profit_10` |
| 11–39 шт. | `profit_10_40` |
| від 40 шт. | `profit_40` |

Кожна ціна використовує поточне `Value` відповідного відсотка. Значення `33` означає націнку `33%`: `baseCost × (1 + 33 / 100) = baseCost × 1.33`. Фурнітура, зарплата цеху та накладні витрати однакові для всіх трьох цін конкретної пари «товар — тканина»; відрізняються лише `fabricPrice` і відсоток цінового діапазону.

Read-model також повертає для товару мінімальну й максимальну ціну серед усіх його тканин окремо для кожного діапазону: `price1To10`, `price11To39` і `price40Plus`. Разом із крайніми значеннями повертається `FabricId`, що сформував відповідну ціну, щоб UI міг показати конкретну тканину.

## Ppe

PPE має ручну `basePrice`, яку вводить адміністратор. Роздрібний та оптовий відсотки незалежно обираються з `AdditionalReference` або вводяться вручну:

```text
retailPrice = basePrice × (1 + retailPercent / 100)
wholesalePrice = basePrice × (1 + wholesalePercent / 100)
```

Роздрібна ціна застосовується до 1–9 шт., оптова — від 10 шт. Обидва відсотки мають unit `%` і застосовуються через `/ 100`.
