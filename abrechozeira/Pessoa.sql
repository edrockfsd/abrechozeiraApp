create table Pessoa
(
    Id                int auto_increment
        primary key,
    Nome              varchar(50)                 null,
    DataNascimento    datetime(6)                 null,
    Email             varchar(100)                null,
    Telefone          varchar(13)                 null,
    PessoaCategoriaId int                         not null,
    PessoaTipoId      int                         not null,
    NickName          longtext charset utf8mb4    null,
    DataInclusao      datetime(6)                 null,
    StatusId          int default 0               not null,
    PessoaGeneroId    int default 0               not null,
    CPF               varchar(50) charset utf8mb4 null,
    RG                longtext charset utf8mb4    null,
    Observacoes       longtext charset utf8mb4    null,
    constraint FK_Pessoa_PessoaCategoria_PessoaCategoriaId
        foreign key (PessoaCategoriaId) references PessoaCategoria (Id)
            on delete cascade,
    constraint FK_Pessoa_PessoaGenero_PessoaGeneroId
        foreign key (PessoaGeneroId) references PessoaGenero (Id)
            on delete cascade,
    constraint FK_Pessoa_PessoaStatus_StatusId
        foreign key (StatusId) references PessoaStatus (Id)
            on delete cascade,
    constraint FK_Pessoa_PessoaTipo_PessoaTipoId
        foreign key (PessoaTipoId) references PessoaTipo (Id)
            on delete cascade
)
    charset = utf8;

create index IX_Pessoa_PessoaCategoriaId
    on Pessoa (PessoaCategoriaId);

create index IX_Pessoa_PessoaGeneroId
    on Pessoa (PessoaGeneroId);

create index IX_Pessoa_PessoaTipoId
    on Pessoa (PessoaTipoId);

create index IX_Pessoa_StatusId
    on Pessoa (StatusId);

