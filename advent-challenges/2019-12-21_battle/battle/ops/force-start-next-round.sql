WITH cte AS (
    select round from tournament
    where actual_start_time is null
    order by round asc
    limit 1
)
update tournament set scheduled_start_time = 0
from cte
where tournament.round = cte.round;