create table Arremate
(
    Id            int auto_increment
        primary key,
    LiveId        int default 0   not null,
    ProdutoId     int             not null,
    Arrematante   varchar(50)     not null,
    ValorArremate decimal(65, 30) null,
    Observacoes   longtext        null,
    DataArremate  datetime(6)     not null,
    DataAlteracao datetime(6)     not null,
    CodigoLive    int             null,
    constraint FK_Arremate_Live_LiveId
        foreign key (LiveId) references Live (Id)
            on delete cascade,
    constraint FK_Arremate_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Arremate_LiveId
    on Arremate (LiveId);

create index IX_Arremate_ProdutoId
    on Arremate (ProdutoId);

