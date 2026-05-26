create table Role
(
    id          int auto_increment
        primary key,
    name        varchar(50) not null,
    description longtext    null,
    is_active   tinyint(1)  not null,
    created_at  datetime(6) not null,
    updated_at  datetime(6) not null
)
    charset = utf8mb4;

