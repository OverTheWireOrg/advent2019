#!/bin/sh
set -e
set -x
if [ "$#" -ne 1 ]; then
    echo "Must specify 1 parameter: the .sql file."
fi
sudo docker cp ops/$1 battle_db_1:/$1
sudo docker exec -it battle_db_1 sh -c "psql -U postgres -d postgres -f /$1 && rm /$1"
