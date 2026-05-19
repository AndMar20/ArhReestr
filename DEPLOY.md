# Развертывание arhreestr.ru

## Что уже подготовлено

- `docker-compose.prod.yml` запускает приложение, MySQL 8.4 и Caddy.
- Caddy открывает сайт на `80/443` и автоматически получает HTTPS-сертификат для `arhreestr.ru`.
- `deploy/mysql/01-init.sql` автоматически импортируется при первом создании пустого docker volume `mysql_data`.

## 1. DNS

В панели домена добавьте записи:

```text
A     arhreestr.ru      IP_ВАШЕГО_СЕРВЕРА
A     www               IP_ВАШЕГО_СЕРВЕРА
```

Если используется IPv6, можно дополнительно добавить `AAAA`.

## 2. Сервер

На сервере должны быть установлены Docker и Docker Compose. Также должны быть открыты порты:

```text
80/tcp
443/tcp
```

## 3. Переменные окружения

Скопируйте пример:

```bash
cp .env.example .env
```

Затем поменяйте пароли в `.env` на реальные сложные значения.

## 4. Запуск

Из папки проекта выполните:

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

Проверить логи:

```bash
docker compose -f docker-compose.prod.yml logs -f webapp
docker compose -f docker-compose.prod.yml logs -f caddy
```

После того как DNS укажет на сервер, сайт должен открываться по адресу:

```text
https://arhreestr.ru
```

## Важно про базу

`deploy/mysql/01-init.sql` импортируется только один раз, когда volume `mysql_data` еще пустой. Если база уже была создана, повторный запуск compose не перезапишет данные.

Для полной переинициализации базы сначала сделайте резервную копию, затем можно удалить volume:

```bash
docker compose -f docker-compose.prod.yml down -v
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

Команда `down -v` удаляет данные MySQL, поэтому используйте ее только осознанно.
