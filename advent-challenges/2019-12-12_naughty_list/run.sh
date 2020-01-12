#!/bin/bash
docker-compose down
sudo rm -rf ./files/data/mysql/*
sudo rm -rf ./files/web/cache/*
sudo rm -rf ./files/logs/apache2/*.log
sudo rm -rf ./files/logs/mysql/*.log
docker-compose up -d --build
sleep 1
sudo rm -rf ./files/data/mysql/ib_buffer_pool
sleep 15
docker exec naughty_list sh -c 'exec /usr/bin/mysql -h mysql -u root -pd4t4b4s3! chall < /var/www/html/chall_users.sql'
rm -rf ./files/web/cache
mkdir ./files/web/cache
chmod 777 ./files/web/cache
