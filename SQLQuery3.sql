select * from activesessions
select * from usersessions

drop view result


create view result as
select Companyname AS Prospect, sessionduration AS 'Session duration', CONVERT(VARCHAR(11),addedDate,104) as 'Date logged in', REVERSE(PARSENAME(REPLACE(REVERSE(username), '\', '.'), 2)) 
AS 'Demo account', CONVERT(VARCHAR(11),expiryDate,104) AS 'Trial expiry date', computername as 'Client computer name'
from (
    select companyname, 1 as 'sortingColumn', sessionduration, addeddate, companyname as o, expiryDate, userName, computername
    from UserSessions as a
    WHERE addeddate < dateadd(day,7,getdate()) AND computerName not like '%st-%'
    union all
    select 'Total time logged in - ', 2 as 'sortingColumn', CONVERT(varchar, DATEADD(ms, SUM(DATEDIFF(second, 0, sessionduration)) * 1000, 0), 108),NULL, companyname as o, NULL,NULL, NULL
    from UserSessions as a
    WHERE addeddate < dateadd(day,7,getdate()) AND computerName not like '%st-%'
    group by companyname, expiryDate, userName, computerName
) as a
order by o, sortingColumn, sessionduration
OFFSET 0 ROWS



select * from result

SELECT REVERSE(PARSENAME(REPLACE(REVERSE(username), '\', '.'), 2)) AS 'Demo account', Companyname AS Prospect, expirydate AS 'Trial expiry date', 
CONVERT(varchar, DATEADD(ms, SUM(DATEDIFF(second, 0, sessionduration)) * 1000, 0), 108) AS 'Total time logged in' FROM usersessions
--WHERE addeddate < dateadd(day,0,getdate()) AND computerName like '%st-%'
--GROUP BY sessionduration,username,companyname, expirydate
GROUP BY sessionduration,username,companyname, expirydate
order by companyname

username computername companyname expiryDate sessionduration, addeddate

select Companyname AS Prospect, sessionduration AS 'Session duration', CONVERT(VARCHAR(11),addedDate,104) as 'Date logged in', REVERSE(PARSENAME(REPLACE(REVERSE(username), '\', '.'), 2)) 
AS 'Demo account', CONVERT(VARCHAR(11),expiryDate,104) AS 'Trial expiry date', computername as 'Client computer name'
from (
    select companyname, 1 as 'sortingColumn', sessionduration, addeddate, companyname as o, expiryDate, userName, computername
    from UserSessions as a
    WHERE addeddate < dateadd(day,7,getdate()) AND computerName not like '%st-%'
    union all
    select 'Total time logged in - ', 2 as 'sortingColumn', CONVERT(varchar, DATEADD(ms, SUM(DATEDIFF(second, 0, sessionduration)) * 1000, 0), 108),NULL, companyname as o, NULL,NULL, NULL
    from UserSessions as a
    WHERE addeddate < dateadd(day,7,getdate()) AND computerName not like '%st-%'
    group by companyname, expiryDate, userName, computerName
) as a
order by o, sortingColumn, sessionduration



