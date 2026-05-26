create table UserRole
(
    id         int auto_increment
        primary key,
    user_id    int         not null,
    role_id    int         not null,
    created_at datetime(6) not null,
    constraint FK_UserRole_Role_role_id
        foreign key (role_id) references Role (id)
            on delete cascade,
    constraint FK_UserRole_User_user_id
        foreign key (user_id) references User (id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_UserRole_role_id
    on UserRole (role_id);

create index IX_UserRole_user_id
    on UserRole (user_id);

