create table Venda
(
    Id             int auto_increment
        primary key,
    Quantidade     int             not null,
    ValorVenda     decimal(65, 30) not null,
    Desconto       decimal(65, 30) null,
    ClienteId      int             not null,
    ProdutoId      int             not null,
    CodigoLive     int             null,
    OrigemID       int             null,
    OrdemVendaLive int             null,
    LiveId         int             null,
    DataAlteracao  datetime(6)     null,
    DataPagamento  datetime(6)     null,
    DataVenda      datetime(6)     null,
    constraint FK_Venda_Live_LiveId
        foreign key (LiveId) references Live (Id),
    constraint FK_Venda_Origem_OrigemID
        foreign key (OrigemID) references Origem (Id),
    constraint FK_Venda_Pessoa_ClienteId
        foreign key (ClienteId) references Pessoa (Id)
            on delete cascade,
    constraint FK_Venda_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade
)
    charset = utf8;

create index IX_Venda_ClienteId
    on Venda (ClienteId);

create index IX_Venda_LiveId
    on Venda (LiveId);

create index IX_Venda_OrigemID
    on Venda (OrigemID);

create index IX_Venda_ProdutoId
    on Venda (ProdutoId);

