create table LiveSession
(
    Id          int auto_increment
        primary key,
    LiveVideoId bigint      not null,
    Status      varchar(50) not null,
    StartedAt   datetime(6) not null,
    EndedAt     datetime(6) null
)
    charset = utf8mb4;

