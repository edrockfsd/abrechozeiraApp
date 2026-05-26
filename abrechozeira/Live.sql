create table Live
(
    Id            int auto_increment
        primary key,
    Titulo        varchar(50)                                      not null,
    Observacoes   longtext                                         null,
    DataLive      datetime(6) default '0001-01-01 00:00:00.000000' not null,
    DataAlteracao datetime(6) default '0001-01-01 00:00:00.000000' not null
)
    charset = utf8;

