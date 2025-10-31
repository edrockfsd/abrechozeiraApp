import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';
import { UserListComponent } from './components/user-list/user-list.component';
import { RoleListComponent } from './components/role-list/role-list.component';
import { TestUsersComponent } from './components/test-users/test-users.component';

const routes: Routes = [
  { path: '', component: UserListComponent, canActivate: [AuthGuard, PermissionGuard], data: { roles: ['ADMIN'] } },
  { path: 'roles', component: RoleListComponent, canActivate: [AuthGuard, PermissionGuard], data: { roles: ['ADMIN'] } },
  { path: 'test', component: TestUsersComponent, canActivate: [AuthGuard, PermissionGuard], data: { roles: ['ADMIN'] } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserManagementRoutingModule { }
