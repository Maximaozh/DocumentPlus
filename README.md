# Document Manager - система управления документами с RBAC

## Технологический стек
- **Backend**: ASP.NET Core 6+ (Minimal API)
- **Frontend**: Blazor WebAssembly
- **База данных**: PostgreSQL
- **UI-компоненты**: MudBlazor (Material Design)
- **Аутентификация**: JWT Bearer Tokens

## Ключевые функции
### Управление доступом
- Ролевая модель (Администратор/Пользователь)
- Групповая политика доступа к документам
- Временные ограничения доступа
- 3 уровня прав:
  - Чтение
  - Редактирование
  - Удаление/продление

### Работа с документами
- Ограничение срока действия
- Визуализация иерархии доступа
- Поиск и фильтрация

### Управление пользователями
- CRUD операции
- Групповая работа
- Drag&Drop назначение прав
- Подробные журналы действий

## Особенности реализации
- **Архитектура**: REST API + Blazor WASM
- **Безопасность**:
  - Хеширование паролей
  - JWT с ролевыми правами
  - Валидация на всех уровнях
- **Производительность**:
  - Кэширование запросов
  - Пагинация данных
  - Оптимизированные SQL-запросы

### Примеры интерфейса

![изображение](https://github.com/user-attachments/assets/91574eac-0337-43f7-a903-a71e4f6e814b)

![изображение](https://github.com/user-attachments/assets/ef290fee-fb6c-4a13-8ba7-29780014abbf)

![изображение](https://github.com/user-attachments/assets/7d9605cf-aec6-4f78-85a1-1084ba48c183)

![изображение](https://github.com/user-attachments/assets/01085047-841c-4e7d-8fb5-1af4e126b114)

![изображение](https://github.com/user-attachments/assets/aa80e57b-22ed-4003-93a8-13bd1a71edec)

![изображение](https://github.com/user-attachments/assets/617ae310-a7d9-4340-9c4d-f9b6c60a6a56)

![изображение](https://github.com/user-attachments/assets/362abd48-2042-4aaa-884a-e09d2dbce6be)
