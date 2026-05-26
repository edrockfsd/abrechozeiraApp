create table User
(
    id         int auto_increment
        primary key,
    created_at datetime(6)  default '0001-01-01 00:00:00.000000' not null,
    email      varchar(255) default ''                           not null,
    is_active  tinyint(1)   default 0                            not null,
    name       varchar(255) default ''                           not null,
    password   varchar(255) default ''                           not null,
    updated_at datetime(6)  default '0001-01-01 00:00:00.000000' not null
)
    charset = utf8mb4;

