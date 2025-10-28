create table Pedido
(
    Id                  int auto_increment
        primary key,
    PedidoCodigo        int                                              not null,
    DataLancamento      datetime(6)                                      not null,
    ClienteID           int                                              not null,
    DescontoPorcentagem decimal(18, 3)                                   null,
    ValorFrete          decimal(18, 3)                                   null,
    PedidoStatusID      int                                              not null,
    ValorTotal          decimal(18, 3)                                   null,
    CondicaoPagamentoID int                                              null,
    FormaPagamentoID    int                                              null,
    EnderecoEntregaID   int                                              null,
    Observacoes         longtext                                         null,
    DataAlteracao       datetime(6) default '0001-01-01 00:00:00.000000' not null,
    constraint IX_Pedido_PedidoCodigo
        unique (PedidoCodigo),
    constraint FK_Pedido_CondicaoPagamento_CondicaoPagamentoID
        foreign key (CondicaoPagamentoID) references CondicaoPagamento (Id),
    constraint FK_Pedido_Endereco_EnderecoEntregaID
        foreign key (EnderecoEntregaID) references Endereco (Id),
    constraint FK_Pedido_FormaPagamento_FormaPagamentoID
        foreign key (FormaPagamentoID) references FormaPagamento (Id),
    constraint FK_Pedido_PedidoStatus_PedidoStatusID
        foreign key (PedidoStatusID) references PedidoStatus (Id)
            on delete cascade,
    constraint FK_Pedido_Pessoa_ClienteID
        foreign key (ClienteID) references Pessoa (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Pedido_ClienteID
    on Pedido (ClienteID);

create index IX_Pedido_CondicaoPagamentoID
    on Pedido (CondicaoPagamentoID);

create index IX_Pedido_EnderecoEntregaID
    on Pedido (EnderecoEntregaID);

create index IX_Pedido_FormaPagamentoID
    on Pedido (FormaPagamentoID);

create index IX_Pedido_PedidoStatusID
    on Pedido (PedidoStatusID);

