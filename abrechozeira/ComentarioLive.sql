create table ComentarioLive
(
    Id               int auto_increment
        primary key,
    Username         varchar(200)                            not null,
    CommentText      longtext                                not null,
    CommentTimestamp datetime(6)                             not null,
    CreatedAt        datetime(6) default current_timestamp() not null,
    LiveSessionId    bigint                                  null
)
    charset = utf8mb4;

