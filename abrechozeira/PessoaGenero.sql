create table PessoaGenero
(
    Id        int auto_increment
        primary key,
    Sigla     varchar(1)  not null,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

