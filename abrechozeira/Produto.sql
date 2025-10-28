create table Produto
(
    Id            int auto_increment
        primary key,
    Descricao     varchar(100)                not null,
    Tamanho       varchar(50)                 null,
    PrecoCusto    decimal(65, 2)              null,
    PrecoVenda    decimal(18, 3)              null,
    Origem        varchar(50) charset utf8mb4 null,
    GrupoID       int default 0               not null,
    DataCompra    datetime(6)                 null,
    DataAlteracao datetime(6)                 null,
    StatusId      int default 0               not null,
    MarcaId       int                         null,
    GeneroID      int                         not null,
    PerfilID      int                         not null,
    Condicao      varchar(1) charset utf8mb4  null,
    constraint FK_Produto_PessoaGenero_GeneroID
        foreign key (GeneroID) references PessoaGenero (Id)
            on delete cascade,
    constraint FK_Produto_ProdutoGrupo_GrupoID
        foreign key (GrupoID) references ProdutoGrupo (Id)
            on delete cascade,
    constraint FK_Produto_ProdutoMarca_MarcaId
        foreign key (MarcaId) references ProdutoMarca (Id),
    constraint FK_Produto_ProdutoPerfil_PerfilID
        foreign key (PerfilID) references ProdutoPerfil (Id)
            on delete cascade,
    constraint FK_Produto_ProdutoStatus_StatusId
        foreign key (StatusId) references ProdutoStatus (Id)
            on delete cascade
)
    charset = utf8;

create index IX_Produto_GeneroID
    on Produto (GeneroID);

create index IX_Produto_GrupoID
    on Produto (GrupoID);

create index IX_Produto_MarcaId
    on Produto (MarcaId);

create index IX_Produto_PerfilID
    on Produto (PerfilID);

create index IX_Produto_StatusId
    on Produto (StatusId);

