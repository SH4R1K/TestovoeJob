# init.sql едет внутрь образа, а не монтируется с хоста — причина в README, «Разбор логов».
FROM postgres:18

COPY db/init.sql /docker-entrypoint-initdb.d/init.sql

RUN chmod 0644 /docker-entrypoint-initdb.d/init.sql
