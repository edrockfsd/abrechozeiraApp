create table Estoque
(
    Id            int auto_increment
        primary key,
    Quantidade    int           not null,
    Localizacao   varchar(100)  null,
    DataAlteracao datetime(6)   null,
    ProdutoId     int default 0 not null,
    CodigoEstoque int           null,
    constraint FK_Estoque_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Estoque_ProdutoId
    on Estoque (ProdutoId);

