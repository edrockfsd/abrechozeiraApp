create table PedidoProduto
(
    Id                int auto_increment
        primary key,
    PedidoId          int                                              not null,
    ProdutoId         int                                              not null,
    Quantidade        int                                              not null,
    DescontoValor     decimal(18, 3)                                   null,
    ValorFinalProduto decimal(18, 3)                                   null,
    DataAlteracao     datetime(6) default '0001-01-01 00:00:00.000000' not null,
    constraint FK_PedidoProduto_Pedido_PedidoId
        foreign key (PedidoId) references Pedido (Id)
            on delete cascade,
    constraint FK_PedidoProduto_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_PedidoProduto_PedidoId
    on PedidoProduto (PedidoId);

create index IX_PedidoProduto_ProdutoId
    on PedidoProduto (ProdutoId);

