create table ComentarioLive
(
    Id               int auto_increment
        primary key,
    Username         varchar(200)                            not null,
    CommentText      longtext                                not null,
    CommentTimestamp datetime(6)                             not null,
    CreatedAt        datetime(6) default current_timestamp() not null,
    LiveSessionID    bigint                                  null
)
    charset = utf8mb4;

create table LiveSession
(
    Id          int auto_increment
        primary key,
    LiveVideoId bigint      not null,
    Status      varchar(50) not null,
    StartedAt   datetime(6) not null,
    EndedAt     datetime(6) null
)
    charset = utf8mb4;

create table NivelAcesso
(
    Id        int auto_increment
        primary key,
    Descricao varchar(200) not null
)
    charset = utf8;

create table Origem
(
    Id        int auto_increment
        primary key,
    Descricao varchar(200) not null
)
    charset = utf8;

create table PessoaCategoria
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8;

create table PessoaGenero
(
    Id        int auto_increment
        primary key,
    Sigla     varchar(1)  not null,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

create table PessoaPerfil
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

create table PessoaStatus
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

create table PessoaTipo
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8;

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

create table ProdutoGrupo
(
    Id        int auto_increment
        primary key,
    Descricao varchar(100) not null
)
    charset = utf8mb4;

create table ProdutoMarca
(
    Id        int auto_increment
        primary key,
    Descricao varchar(100) not null
)
    charset = utf8mb4;

create table ProdutoPerfil
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

create table ProdutoStatus
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

create table TipoEndereco
(
    Id        int auto_increment
        primary key,
    Descricao varchar(50) not null
)
    charset = utf8mb4;

create table Usuario
(
    Id            int auto_increment
        primary key,
    Login         varchar(50) null,
    Senha         varchar(50) null,
    NivelAcessoID int         not null,
    PessoaID      int         not null,
    constraint FK_Usuario_NivelAcesso_NivelAcessoID
        foreign key (NivelAcessoID) references NivelAcesso (Id)
            on delete cascade,
    constraint FK_Usuario_Pessoa_PessoaID
        foreign key (PessoaID) references Pessoa (Id)
            on delete cascade
)
    charset = utf8;

create table CondicaoPagamento
(
    Id                   int auto_increment
        primary key,
    Descricao            longtext    not null,
    DataAlteracao        datetime(6) null,
    UsuarioModificacaoId int         not null,
    constraint FK_CondicaoPagamento_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_CondicaoPagamento_UsuarioModificacaoId
    on CondicaoPagamento (UsuarioModificacaoId);

create table Endereco
(
    Id                   int auto_increment
        primary key,
    PessoaID             int          not null,
    CEP                  varchar(8)   null,
    Logradouro           varchar(100) not null,
    Unidade              varchar(8)   not null,
    Complemento          varchar(50)  null,
    Bairro               varchar(50)  not null,
    Localidade           varchar(50)  not null,
    CodigoLocalidadeIBGE int          not null,
    Estado               varchar(30)  not null,
    DataAlteracao        datetime(6)  null,
    UsuarioModificacaoId int          not null,
    TipoEnderecoId       int          not null,
    Observacoes          longtext     null,
    constraint FK_Endereco_Pessoa_PessoaID
        foreign key (PessoaID) references Pessoa (Id)
            on delete cascade,
    constraint FK_Endereco_TipoEndereco_TipoEnderecoId
        foreign key (TipoEnderecoId) references TipoEndereco (Id)
            on delete cascade,
    constraint FK_Endereco_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Endereco_PessoaID
    on Endereco (PessoaID);

create index IX_Endereco_TipoEnderecoId
    on Endereco (TipoEnderecoId);

create index IX_Endereco_UsuarioModificacaoId
    on Endereco (UsuarioModificacaoId);

create table FormaPagamento
(
    Id                   int auto_increment
        primary key,
    Descricao            longtext    not null,
    DataAlteracao        datetime(6) null,
    UsuarioModificacaoId int         not null,
    constraint FK_FormaPagamento_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_FormaPagamento_UsuarioModificacaoId
    on FormaPagamento (UsuarioModificacaoId);

create table Live
(
    Id                   int auto_increment
        primary key,
    Titulo               varchar(50)                                      not null,
    Observacoes          longtext                                         null,
    DataLive             datetime(6) default '0001-01-01 00:00:00.000000' not null,
    DataAlteracao        datetime(6) default '0001-01-01 00:00:00.000000' not null,
    UsuarioModificacaoId int         default 0                            not null,
    constraint FK_Live_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8;

create index IX_Live_UsuarioModificacaoId
    on Live (UsuarioModificacaoId);

create table PedidoStatus
(
    Id                   int auto_increment
        primary key,
    Descricao            longtext    not null,
    DataAlteracao        datetime(6) null,
    UsuarioModificacaoId int         not null,
    constraint FK_PedidoStatus_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create table Pedido
(
    Id                   int auto_increment
        primary key,
    PedidoCodigo         int                                              not null,
    DataLancamento       datetime(6)                                      not null,
    ClienteID            int                                              not null,
    DescontoPorcentagem  decimal(18, 3)                                   null,
    ValorFrete           decimal(18, 3)                                   null,
    PedidoStatusID       int                                              not null,
    ValorTotal           decimal(18, 3)                                   null,
    CondicaoPagamentoID  int                                              null,
    FormaPagamentoID     int                                              null,
    EnderecoEntregaID    int                                              null,
    Observacoes          longtext                                         null,
    DataAlteracao        datetime(6) default '0001-01-01 00:00:00.000000' not null,
    UsuarioModificacaoId int                                              not null,
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
            on delete cascade,
    constraint FK_Pedido_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
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

create index IX_Pedido_UsuarioModificacaoId
    on Pedido (UsuarioModificacaoId);

create index IX_PedidoStatus_UsuarioModificacaoId
    on PedidoStatus (UsuarioModificacaoId);

create table Produto
(
    Id                   int auto_increment
        primary key,
    Descricao            varchar(100)                not null,
    Tamanho              varchar(50)                 null,
    PrecoCusto           decimal(65, 2)              null,
    PrecoVenda           decimal(18, 3)              null,
    Origem               varchar(50) charset utf8mb4 null,
    GrupoID              int default 0               not null,
    DataCompra           datetime(6)                 null,
    DataAlteracao        datetime(6)                 null,
    UsuarioModificacaoId int default 0               not null,
    StatusId             int default 0               not null,
    MarcaId              int                         null,
    GeneroID             int                         not null,
    PerfilID             int                         not null,
    Condicao             varchar(1) charset utf8mb4  null,
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
            on delete cascade,
    constraint FK_Produto_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8;

create table Arremate
(
    Id                   int auto_increment
        primary key,
    LiveId               int default 0   not null,
    ProdutoId            int             not null,
    Arrematante          varchar(50)     not null,
    ValorArremate        decimal(65, 30) null,
    Observacoes          longtext        null,
    DataArremate         datetime(6)     not null,
    DataAlteracao        datetime(6)     not null,
    UsuarioModificacaoId int             not null,
    CodigoLive           int             null,
    constraint FK_Arremate_Live_LiveId
        foreign key (LiveId) references Live (Id)
            on delete cascade,
    constraint FK_Arremate_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade,
    constraint FK_Arremate_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Arremate_LiveId
    on Arremate (LiveId);

create index IX_Arremate_ProdutoId
    on Arremate (ProdutoId);

create index IX_Arremate_UsuarioModificacaoId
    on Arremate (UsuarioModificacaoId);

create table Estoque
(
    Id                   int auto_increment
        primary key,
    Quantidade           int           not null,
    Localizacao          varchar(100)  null,
    DataAlteracao        datetime(6)   null,
    UsuarioModificacaoId int default 0 not null,
    ProdutoId            int default 0 not null,
    CodigoEstoque        int           null,
    constraint FK_Estoque_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade,
    constraint FK_Estoque_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Estoque_ProdutoId
    on Estoque (ProdutoId);

create index IX_Estoque_UsuarioModificacaoId
    on Estoque (UsuarioModificacaoId);

create table PedidoProduto
(
    Id                   int auto_increment
        primary key,
    PedidoId             int                                              not null,
    ProdutoId            int                                              not null,
    Quantidade           int                                              not null,
    DescontoValor        decimal(18, 3)                                   null,
    ValorFinalProduto    decimal(18, 3)                                   null,
    DataAlteracao        datetime(6) default '0001-01-01 00:00:00.000000' not null,
    UsuarioModificacaoId int                                              not null,
    constraint FK_PedidoProduto_Pedido_PedidoId
        foreign key (PedidoId) references Pedido (Id)
            on delete cascade,
    constraint FK_PedidoProduto_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade,
    constraint FK_PedidoProduto_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_PedidoProduto_PedidoId
    on PedidoProduto (PedidoId);

create index IX_PedidoProduto_ProdutoId
    on PedidoProduto (ProdutoId);

create index IX_PedidoProduto_UsuarioModificacaoId
    on PedidoProduto (UsuarioModificacaoId);

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

create index IX_Produto_UsuarioModificacaoId
    on Produto (UsuarioModificacaoId);

create index IX_Usuario_NivelAcessoID
    on Usuario (NivelAcessoID);

create index IX_Usuario_PessoaID
    on Usuario (PessoaID);

create table Venda
(
    Id                   int auto_increment
        primary key,
    Quantidade           int             not null,
    ValorVenda           decimal(65, 30) not null,
    Desconto             decimal(65, 30) null,
    ClienteId            int             not null,
    ProdutoId            int             not null,
    CodigoLive           int             null,
    OrigemID             int             null,
    OrdemVendaLive       int             null,
    LiveId               int             null,
    DataAlteracao        datetime(6)     null,
    DataPagamento        datetime(6)     null,
    DataVenda            datetime(6)     null,
    UsuarioModificacaoId int default 0   not null,
    constraint FK_Venda_Live_LiveId
        foreign key (LiveId) references Live (Id),
    constraint FK_Venda_Origem_OrigemID
        foreign key (OrigemID) references Origem (Id),
    constraint FK_Venda_Pessoa_ClienteId
        foreign key (ClienteId) references Pessoa (Id)
            on delete cascade,
    constraint FK_Venda_Produto_ProdutoId
        foreign key (ProdutoId) references Produto (Id)
            on delete cascade,
    constraint FK_Venda_Usuario_UsuarioModificacaoId
        foreign key (UsuarioModificacaoId) references Usuario (Id)
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

create index IX_Venda_UsuarioModificacaoId
    on Venda (UsuarioModificacaoId);

create table __EFMigrationsHistory
(
    MigrationId    varchar(150) not null
        primary key,
    ProductVersion varchar(32)  not null
)
    charset = utf8;


