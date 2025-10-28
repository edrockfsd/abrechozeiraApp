create table PedidoStatus
(
    Id            int auto_increment
        primary key,
    Descricao     longtext    not null,
    DataAlteracao datetime(6) null
)
    charset = utf8mb4;

