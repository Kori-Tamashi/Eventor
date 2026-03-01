## Таблицы

1) locations – таблица локаций;
2) events – таблица мероприятий;
3) registrations – таблица регистраций участников на мероприятия;
4) days – таблица дней мероприятий;
5) participation - таблица участия в конкретном дне;
6) menu – таблица меню дней мероприятий;
7) items – таблица предметов меню;
8) feedbacks – таблица отзывов участников;
9) menu_items - таблица связи меню и предметов;
10) users – таблица пользователей.

### Таблица 2.1 — Таблица locations
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| location_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор локации |
| title | Строковый | NOT NULL | Название |
| description | Строковый | NOT NULL | Описание |
| cost | Вещественный | NOT NULL, CHECK (cost >= 0) | Цена аренды на 1 день |
| capacity | Целочисленный | NOT NULL, CHECK (capacity >= 0) | Вместимость |

### Таблица 2.2 — Таблица events
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| event_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор мероприятия |
| title | Строковый | NOT NULL | Название |
| description | Строковый | NOT NULL | Описание |
| start_date | Дата | NOT NULL | Дата начала |
| location_id | UUID | NOT NULL | Идентификатор локации |
| days_count | Целый | NOT NULL, CHECK (days_count >= 0) | Количество дней в мероприятии |
| percent | Вещественный | NOT NULL, CHECK (percent >= 0) | Наценка на посещение в процентах |

### Таблица 2.3 — Таблица days
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| day_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор дня мероприятия |
| event_id | UUID | NOT NULL | Идентификатор мероприятия |
| title | Строковый | NOT NULL | Название |
| number | Целочисленный | NOT NULL, CHECK (sequence_number > 0) | Порядковый номер |
| description | Строковый | NOT NULL | Описание |
| menu_id | UUID | NOT NULL | Идентификатор меню |

### Таблица 2.4 — Таблица registrations
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| person_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор регистрации |
| event_id | UUID | NOT NULL, UNIQUE(event_id, user_id) | Идентификатор мероприятия |
| user_id | UUID | NOT NULL | Идентификатор пользователя |
| type | Перечисляемый | NOT NULL | Тип (стандартный, VIP, организатор)|
| payment | Логический | NOT NULL | Факт оплаты |

### Таблица 2.5 — Таблица participation
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| day_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор дня мероприятия |
| registration_id | UUID | NOT NULL, UNIQUE(day_id, registration_id), PRIMARY KEY | Идентификатор регистрации |

### Таблица 2.6 — Таблица menu
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| menu_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор меню |
| title | Строковый | NOT NULL | Название |
| description | Строковый | NOT NULL | Описание |

### Таблица 2.7 — Таблица items
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| item_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор предмета |
| title | Строковый | NOT NULL | Название |
| cost | Вещественный | NOT NULL, CHECK (cost >= 0) | Цена |

### Таблица 2.8 — Таблица menu_items
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| menu_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор меню |
| item_id | UUID | NOT NULL, PRIMARY KEY, UNIQUE(menu_id, item_id) | Идентификатор предмета |
| amount | double | NOT NULL | Количество предмета |

### Таблица 2.9 —Таблица feedbacks
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| feedback_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор отзыва |
| comment | Строковый | NOT NULL | Комментарий |
| rate | Целочисленный | NOT NULL, CHECK (rating >= 1 AND rating <= 5) | Рейтинг |
| registation_id | UUID | NOT NULL, FOREIGN KEY | Участник мероприятия |

### Таблица 2.10 — Таблица users
| Атрибут | Тип данных | Ограничения | Сведение |
| :--- | :--- | :--- | :--- |
| user_id | UUID | NOT NULL, PRIMARY KEY | Идентификатор пользователя |
| name | Строковый | NOT NULL | Имя |
| phone | Строковый | NOT NULL, UNIQUE | Телефон |
| gender | Перечисляемый | NOT NULL | Гендер |
| role | Перечисляемый | NOT NULL | Роль (гость, пользователь, администратор) |
| password_hash | Строковый | NOT NULL | Пароль (в зашифрованном виде) |
