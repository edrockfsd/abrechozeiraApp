create table RolePermission
(
    id            int auto_increment
        primary key,
    role_id       int         not null,
    permission_id int         not null,
    created_at    datetime(6) not null,
    constraint FK_RolePermission_Permission_permission_id
        foreign key (permission_id) references Permission (id)
            on delete cascade,
    constraint FK_RolePermission_Role_role_id
        foreign key (role_id) references Role (id)
            on delete cascade
)
    charset = utf8mb4;

create index IX_RolePermission_permission_id
    on RolePermission (permission_id);

create index IX_RolePermission_role_id
    on RolePermission (role_id);

