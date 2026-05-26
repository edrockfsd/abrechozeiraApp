create table __EFMigrationsHistory
(
    MigrationId    varchar(150) not null
        primary key,
    ProductVersion varchar(32)  not null
)
    charset = utf8;

