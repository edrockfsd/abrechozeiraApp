create table Permission
(
    id          int auto_increment
        primary key,
    name        varchar(100) not null,
    description longtext     null,
    resource    varchar(50)  not null,
    action      varchar(50)  not null,
    is_active   tinyint(1)   not null,
    created_at  datetime(6)  not null,
    updated_at  datetime(6)  not null
)
    charset = utf8mb4;

