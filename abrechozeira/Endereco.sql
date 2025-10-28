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
    TipoEnderecoId       int          not null,
    Observacoes          longtext     null,
    constraint FK_Endereco_Pessoa_PessoaID
        foreign key (PessoaID) references Pessoa (Id)
            on delete cascade,
    constraint FK_Endereco_TipoEndereco_TipoEnderecoId
        foreign key (TipoEnderecoId) references TipoEndereco (Id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_Endereco_PessoaID
    on Endereco (PessoaID);

create index IX_Endereco_TipoEnderecoId
    on Endereco (TipoEnderecoId);

